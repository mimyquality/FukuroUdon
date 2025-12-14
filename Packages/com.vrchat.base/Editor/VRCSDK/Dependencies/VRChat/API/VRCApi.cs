using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;
using VRC.Core;

[assembly: InternalsVisibleTo("VRC.SDK3.Editor")]
[assembly: InternalsVisibleTo("VRC.SDK3A.Editor")]
namespace VRC.SDKBase.Editor.Api {
    [InitializeOnLoad]
    public static class VRCApi
    {
        private static Uri VRC_BASE_URL;
        private static Uri VRC_COOKIE_BASE_URL;
        internal static JsonSerializerSettings JSON_OPTIONS;
        private static int DEFAULT_RETRY_COUNT;
        private static float DEFAULT_RETRY_DELAY;
        private static float DEFAULT_EXP_BACKOFF;
        private static int MULTIPART_PART_SIZE;
        internal static string AVATAR_FALLBACK_TAG = "author_quest_fallback";

        static VRCApi()
        {
            
            VRC_COOKIE_BASE_URL = new Uri("https://api.vrchat.cloud");
            VRC_BASE_URL = new Uri("https://api.vrchat.cloud/api/1/");
            JSON_OPTIONS = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new DateTimeNoneHandler(),
                    new InstanceHandler()
                }
            };

            DEFAULT_RETRY_COUNT = 5;
            DEFAULT_RETRY_DELAY = 1f;
            DEFAULT_EXP_BACKOFF = 2f;
            
            MULTIPART_PART_SIZE = 100 * 1024 * 1024;

            Headers = new Dictionary<string, string>
            {
                {"User-Agent", "VRC.Core.BestHTTP"},
                {"X-MacAddress", API.DeviceID},
                {"X-SDK-Version", Tools.SdkVersion},
                {"X-Platform", Tools.Platform},
                {"X-Unity-Version", Application.unityVersion},
                {"Accept", "application/json"}
            };
        }

        #region API Internals
        
        private static readonly Dictionary<string, string> Headers;
        
        private static CookieContainer GetCookies(Uri url)
        {
            if (url.Host != VRC_COOKIE_BASE_URL.Host) return new CookieContainer();
            if (!ApiCredentials.IsLoaded())
            {
                ApiCredentials.Load();
            }
            var authCookie = ApiCredentials.GetAuthTokenCookie();
            var twoFa = ApiCredentials.GetTwoFactorAuthTokenCookie();
            var cookies = new CookieContainer();
            if (authCookie != null)
            {
                cookies.Add(VRC_COOKIE_BASE_URL, new Cookie(authCookie.Name, authCookie.Value));
            }

            if (twoFa != null)
            {
                cookies.Add(VRC_COOKIE_BASE_URL, new Cookie(twoFa.Name, twoFa.Value));
            }
            return cookies;

        }
        
        private static HttpClient GetClient(Uri url)
        {
            {
                var cookies = GetCookies(url);
                var handler = new HttpClientHandler
                {
                    CookieContainer = cookies,
                    UseProxy = false
                };
                var client = new HttpClient(handler);
                foreach (var header in Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                
                return client;
            }
        }

        private static void SetApiUrl()
        {
            VRCSdkControlPanel.RefreshApiUrlSetting();
            var apiUrl = API.GetApiUrlForEnvironment(VRCSdkControlPanel.ApiEnvironment);
            VRC_BASE_URL = new Uri(apiUrl);
            VRC_COOKIE_BASE_URL = new Uri($"{VRC_BASE_URL.Scheme}://{VRC_BASE_URL.Host}");
        }

        private static async Task<Uri> BuildUrl(string requestUrl, Dictionary<string, string> queryParams = null)
        {
            var uri = new UriBuilder(VRC_BASE_URL);
            if (requestUrl.StartsWith(VRC_BASE_URL.ToString()))
            {
                requestUrl = requestUrl.Remove(0, VRC_BASE_URL.ToString().Length);
            } else if (requestUrl.StartsWith("https://")) // fir any non-vrc URLs we just return them as-is
            {
                return new Uri(requestUrl);
            }
            uri.Path += requestUrl;
            if (queryParams != null)
            {
                var query = new FormUrlEncodedContent(queryParams);
                uri.Query = await query.ReadAsStringAsync();
            }

            return uri.Uri;
        }
        
        private class EmptyResponse {}
        
        #endregion

        #region REST API Methods

        [PublicAPI]
        public static async Task<(TResponse responseData, HttpResponseMessage responseMessage)> MakeRequestWithResponse<T, TResponse>(string requestUrl, HttpMethod method,
            Dictionary<string, string> queryParams = null, 
            bool forceRefresh = false, 
            T body = default,
            string contentType = null, 
            byte[] contentMD5 = null, 
            int contentLength = 0, 
            int timeout = 30,
            Action<float> onProgress = null,
            CancellationToken cancellationToken = default,
            JsonSerializerSettings jsonSettings = null)
        {
            SetApiUrl();
            var uri = await BuildUrl(requestUrl, queryParams);
            var cachedResponse = VRCApiCache.Get<TResponse>(uri.ToString(), out var isCached);
            if (method == HttpMethod.Get)
            {
                // If cache has not expired yet - return cached value
                if (isCached && !forceRefresh)
                {
                    Core.Logger.Log($"Got Cached Response for {uri.ToString()}", API.LOG_CATEGORY);
                    return (cachedResponse, null);
                }
            }
            else
            {
                if (isCached || forceRefresh)
                {
                    VRCApiCache.Invalidate(uri.ToString());
                }
            }

            var request = new HttpRequestMessage(method, uri);
            var type = typeof(T);
            var isByteArray = type.IsArray && type.GetElementType() == typeof(byte);

            if (body != null)
            {
                if (isByteArray)
                {
                    var byteArray = body as byte[] ?? Array.Empty<byte>();
                    if (onProgress != null)
                    {
                        var byteStream = new MemoryStream(byteArray);
                        request.Content = new VRCProgressContent(new StreamContent(byteStream), onProgress);
                    }
                    else
                    {
                        request.Content = new ByteArrayContent(byteArray);
                    }

                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }

                    if (contentMD5 != null)
                    {
                        request.Content.Headers.ContentMD5 = contentMD5;
                    }
                }
                else
                {
                    string json;
                    try
                    {
                        json = JsonConvert.SerializeObject(body, jsonSettings ?? JSON_OPTIONS);
                        Core.Logger.Log($"Request body - json: {json}", API.LOG_CATEGORY);

                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException("Failed to serialize data to JSON", ex);
                    }
                    request.Content = new StringContent(json);
                }
            }
            
            if (method != HttpMethod.Get && method != HttpMethod.Delete && !isByteArray && body != null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            HttpResponseMessage result;
            try
            {
                var client = GetClient(request.RequestUri);
                client.Timeout = TimeSpan.FromSeconds(timeout);
                var sendTask = client.SendAsync(request, cancellationToken);

                if (body != null)
                {
                    await Task.WhenAll(
                        sendTask,
                        VRCTools.IncreaseSendBuffer(request.RequestUri, sendTask, cancellationToken)
                    );
                    result = sendTask.Result;
                }
                else
                {
                    result = await sendTask;
                }
            }
            catch (TaskCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Core.Logger.LogError("Request cancelled", API.LOG_CATEGORY);
                }
                else
                {
                    Core.Logger.LogError("Request timeout", API.LOG_CATEGORY);
                }

                throw;
            }
            catch (Exception e)
            {
                Core.Logger.LogError("Failed to send request", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                throw;
            }

            Core.Logger.Log($"Request finished - result: {result}", API.LOG_CATEGORY);

            if (!result.IsSuccessStatusCode)
            {
                if (result.Content.Headers.ContentLength > 0)
                {
                    try
                    {
                        throw new ApiErrorException(result, await result.Content.ReadAsStringAsync());
                    }
                    catch (ApiErrorException ex)
                    {
                        Core.Logger.LogError($"Got API Error: {ex.ErrorMessage}", API.LOG_CATEGORY);
                        // Provide an actionable error message
                        if (ex.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Core.Logger.LogError("Unauthorized, try logging out and in again");
                        }
                        throw;
                    }
                    catch
                    {
                        // Fall-through to the generic error exception if encountered non-api exception 
                    }
                    Core.Logger.LogError($"Error data {await result.Content.ReadAsStringAsync()}", API.LOG_CATEGORY);
                }
                throw new RequestFailedException($"Failed to perform a request to VRChat API for {uri} and data {JsonConvert.SerializeObject(body, JSON_OPTIONS)}", result, result.StatusCode);
            }

            var responseType = typeof(TResponse);

            // Sometimes we do not care about response data
            if (responseType == typeof(EmptyResponse))
            {
                return (default, result);
            }
            

            if (responseType.IsArray && responseType.GetElementType() == typeof(byte))
            {
                var bytes = await result.Content.ReadAsByteArrayAsync();
                if (method == HttpMethod.Get)
                {
                    VRCApiCache.Add(uri.ToString(), bytes);
                }
                return ((TResponse) (object) bytes, result);
            }
            
            var text = await result.Content.ReadAsStringAsync();
            try
            {
                Core.Logger.Log($"Request finished - result: {text}", API.LOG_CATEGORY);

                var parsed = JsonConvert.DeserializeObject<TResponse>(text, JSON_OPTIONS);
                if (method == HttpMethod.Get)
                {
                    VRCApiCache.Add(uri.ToString(), parsed);
                }
                return (parsed, result);
            }
            catch (JsonException ex)
            {
                throw new JsonSerializationException($"Failed to deserialize data from JSON, original text: {text}", ex);
            }
        }
        
        [PublicAPI]
        public static async Task<TResponse> MakeRequest<T, TResponse>(string requestUrl, HttpMethod method,
            Dictionary<string, string> queryParams = null, bool forceRefresh = false, T body = default, string contentType = null, byte[] contentMD5 = null, int contentLength = 0, int timeout = 30, Action<float> onProgress = null, CancellationToken cancellationToken = default, JsonSerializerSettings jsonSettings = null)
        {
            var result = await MakeRequestWithResponse<T, TResponse>(requestUrl, method, queryParams, forceRefresh, body, contentType, contentMD5, contentLength, timeout, onProgress, cancellationToken, jsonSettings);
            return result.responseData;
        }

        [PublicAPI]
        public static async Task<T> Get<T>(string requestUrl, Dictionary<string, string> queryParams = null, bool forceRefresh = false, bool allowRetry = false, CancellationToken cancellationToken = default)
        {
            T result = default;
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                return result;
            }
            
            for (var i = 0; i < DEFAULT_RETRY_COUNT; i++)
            {
                try
                {
                    result = await MakeRequest<object, T>(requestUrl, HttpMethod.Get, queryParams, forceRefresh, cancellationToken: cancellationToken);
                    break;
                }
                catch (Exception ex)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow((i + 1) * DEFAULT_RETRY_DELAY, DEFAULT_EXP_BACKOFF));
                    if (!allowRetry)
                    {
                        throw;
                    }
                    Core.Logger.LogError(ex.Message, API.LOG_CATEGORY);
                    if (ex.InnerException != null)
                    {
                        Core.Logger.LogError(ex.InnerException.Message, API.LOG_CATEGORY);
                    }

                    result = default;
                    await Task.Delay(delay);
                }
            }

            return result;
        }

        [PublicAPI]
        public static async Task<TResponse> Post<T, TResponse>(string requestUrl, T body,
            Dictionary<string, string> queryParams = null, bool forceRefresh = false, CancellationToken cancellationToken = default, JsonSerializerSettings jsonSettings = null)
        {
            return await MakeRequest<T, TResponse>(requestUrl, HttpMethod.Post, queryParams, forceRefresh, body, cancellationToken: cancellationToken, jsonSettings: jsonSettings);
        }
        
        [PublicAPI]
        public static async Task<TResponse> Put<T, TResponse>(string requestUrl, T body,
            Dictionary<string, string> queryParams = null, bool forceRefresh = false, CancellationToken cancellationToken = default, JsonSerializerSettings jsonSettings = null)
        {
            return await MakeRequest<T, TResponse>(requestUrl, HttpMethod.Put, queryParams, forceRefresh, body, cancellationToken: cancellationToken, jsonSettings: jsonSettings);
        }
        
        [PublicAPI]
        public static async Task<TResponse> Put<TResponse>(string requestUrl,
            Dictionary<string, string> queryParams = null, bool forceRefresh = false, CancellationToken cancellationToken = default, JsonSerializerSettings jsonSettings = null)
        {
            return await MakeRequest<object, TResponse>(requestUrl, HttpMethod.Put, queryParams, forceRefresh, cancellationToken: cancellationToken, jsonSettings: jsonSettings);
        }

        [PublicAPI]
        public static async Task<T> Delete<T>(string requestUrl, Dictionary<string, string> queryParams = null, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return await MakeRequest<object, T>(requestUrl, HttpMethod.Delete, queryParams, forceRefresh, cancellationToken: cancellationToken);
        }
        
        public static async Task Delete(string requestUrl, Dictionary<string, string> queryParams = null, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            await MakeRequest<object, EmptyResponse>(requestUrl, HttpMethod.Delete, queryParams, forceRefresh, cancellationToken: cancellationToken);
        }
        
        #endregion

        #region VRC API Calls
        
        [PublicAPI]
        public static async Task<VRCWorld> GetWorld(string id, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return await Get<VRCWorld>($"worlds/{id}", forceRefresh: forceRefresh, cancellationToken: cancellationToken);
        }
        
        [PublicAPI]
        public static async Task<VRCWorld> UpdateWorldInfo(string id, VRCWorld data, CancellationToken cancellationToken = default)
        {
            var changes = new VRCWorldChanges
            {
                Name = data.Name,
                Description = data.Description,
                Tags = data.Tags,
                Capacity = data.Capacity,
                RecommendedCapacity = data.RecommendedCapacity,
                PreviewYoutubeId = data.PreviewYoutubeId
            };
            return await Put<VRCWorldChanges, VRCWorld>($"worlds/{id}", changes, cancellationToken: cancellationToken);
        }

        public static async Task<bool> GetCanPublishWorld(string id, CancellationToken cancellationToken = default)
        {
            return (await Get<JObject>($"worlds/{id}/publish", cancellationToken: cancellationToken)).Value<bool>(
                "canPublish");
        }
        
        public static async Task<JObject> PublishWorld(string id, CancellationToken cancellationToken = default)
        {
            return await Put<JObject>($"worlds/{id}/publish", cancellationToken: cancellationToken);
        }
        
        public static async Task<JObject> UnpublishWorld(string id, CancellationToken cancellationToken = default)
        {
            return await Delete<JObject>($"worlds/{id}/publish", cancellationToken: cancellationToken);
        }
        
        [PublicAPI]
        public static async Task<VRCWorld> UpdateWorldImage(string id, VRCWorld data, string pathToImage, Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            var fileName = "World - " + data.Name + " - Image - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var fileId = ApiFile.ParseFileIdFromFileAPIUrl(data.ImageUrl);
            var newImageUrl = await UploadFile(pathToImage, fileId, fileName, onProgress, cancellationToken);
            if (string.IsNullOrWhiteSpace(newImageUrl))
            {
                Debug.Log("new image url is empty, aborting");
                return data;
            }
            var imageUpdateRequest = new Dictionary<string, string>
            {
                {"imageUrl", newImageUrl}
            };
            return await Put<object, VRCWorld>($"worlds/{id}", imageUpdateRequest, cancellationToken: cancellationToken);
        }
        
        [PublicAPI]
        public static async Task<VRCWorld> UpdateWorldBundle(string id, VRCWorld data, string pathToBundle, string worldSignature,
            Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pathToBundle))
            {
                Core.Logger.LogError("Bundle path cannot be empty", API.LOG_CATEGORY);
                return data;
            }
            var fileName = "World - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
                          "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            
            string newBundleUrl = null;
            var currentAssetUrl = data.GetLatestAssetUrlForPlatform(Tools.Platform);
            
            // new platform
            if (string.IsNullOrWhiteSpace(currentAssetUrl))
            {
                newBundleUrl = await UploadFile(pathToBundle, "", fileName, onProgress, cancellationToken);
            }
            else
            {
                var fileId = ApiFile.ParseFileIdFromFileAPIUrl(currentAssetUrl);
                newBundleUrl = await UploadFile(pathToBundle, fileId, fileName, onProgress, cancellationToken);
            }
            if (string.IsNullOrWhiteSpace(newBundleUrl))
            {
                Core.Logger.LogError("New bundle url is empty, aborting", API.LOG_CATEGORY);
                return data;
            }
            var bundleUpdateRequest = new Dictionary<string, object>
            {
                {"name", data.Name},
                {"assetUrl", newBundleUrl},
                {"platform", Tools.Platform.ToString()},
                {"unityVersion", Tools.UnityVersion.ToString()},
                {"assetVersion", 4},
                {"udonProducts", data.UdonProducts},
                {"worldSignature", worldSignature},
            };
            Core.Logger.Log($"Updating with new bundle {newBundleUrl}", API.LOG_CATEGORY);
            await VRCApi.Put<object, VRCWorld>($"worlds/{id}", bundleUpdateRequest, cancellationToken: cancellationToken);
            Core.Logger.Log("Fetching latest", API.LOG_CATEGORY);
            return await VRCApi.Get<VRCWorld>($"worlds/{id}", forceRefresh: true, cancellationToken: cancellationToken);
        } 
        
        [PublicAPI]
        public static async Task<VRCWorld> CreateNewWorld(string id, VRCWorld data, string pathToBundle, string pathToImage, string worldSignature,
            Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pathToBundle) || string.IsNullOrWhiteSpace(pathToImage))
            {
                Core.Logger.LogError("Both bundle and image paths must be provided");
                return data;
            }
            var fileName = "World - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var newBundleUrl = await UploadFile(pathToBundle, "", fileName, onProgress: (status, percentage) =>
            {
                onProgress?.Invoke(status, percentage * 0.5f);
            }, cancellationToken);
            if (string.IsNullOrWhiteSpace(newBundleUrl))
            {
                Core.Logger.LogError("New bundle url is empty, aborting");
                return data;
            }
            var imageFileName = "World - " + data.Name + " - Image - " + Application.unityVersion + "_" + ApiWorld.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var newImageUrl = await UploadFile(pathToImage, "", imageFileName, onProgress: (status, percentage) =>
            {
                onProgress?.Invoke(status, 0.5f + percentage * 0.5f);
            }, cancellationToken);
            if (string.IsNullOrWhiteSpace(newImageUrl))
            {
                Core.Logger.LogError("New image url is empty, aborting");
                return data;
            }
            var newWorldData = new Dictionary<string, object>
            {
                {"id", id},
                {"name", data.Name},
                {"description", data.Description},
                {"assetUrl", newBundleUrl},
                {"imageUrl", newImageUrl},
                {"platform", Tools.Platform},
                {"unityVersion", Tools.UnityVersion.ToString()},
                {"tags", data.Tags},
                {"capacity", data.Capacity},
                {"recommendedCapacity", data.RecommendedCapacity},
                {"previewYoutubeId", data.PreviewYoutubeId},
                {"assetVersion", 4},
                {"udonProducts", data.UdonProducts},
                {"worldSignature", worldSignature},
            };
            var createdWorld = await Post<Dictionary<string, object>, VRCWorld>($"worlds", newWorldData, cancellationToken: cancellationToken);
            Core.Logger.Log("Created a new World");
            return createdWorld;
        }
        
        [PublicAPI]
        public static async Task<VRCAvatar> GetAvatar(string id, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return await Get<VRCAvatar>($"avatars/{id}", forceRefresh: forceRefresh, cancellationToken: cancellationToken);
        }

        [PublicAPI]
        public static async Task<VRCAvatar> UpdateAvatarInfo(string id, VRCAvatar data, CancellationToken cancellationToken = default)
        {
            var changes = new VRCAvatarChanges
            {
                Name = data.Name,
                Description = data.Description,
                Tags = data.Tags,
                ReleaseStatus = data.ReleaseStatus,
                // API doesn't allow null values, so we have to map to empty strings
                PrimaryStyle = data.Styles.Primary ?? string.Empty,
                SecondaryStyle = data.Styles.Secondary ?? string.Empty,
            };
            
            return await VRCApi.Put<VRCAvatarChanges, VRCAvatar>($"avatars/{id}", changes, cancellationToken: cancellationToken);
        }
        
        [PublicAPI]
        public static async Task<VRCAvatar> UpdateAvatarImage(string id, VRCAvatar data, string pathToImage, Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            var fileName = "Avatar - " + data.Name + " - Image - " + Application.unityVersion + "_" + ApiAvatar.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var fileId = ApiFile.ParseFileIdFromFileAPIUrl(data.ImageUrl);
            var newImageUrl = await UploadFile(pathToImage, fileId, fileName, onProgress, cancellationToken);
            if (string.IsNullOrWhiteSpace(newImageUrl))
            {
                Core.Logger.LogError("New image url is empty, aborting", API.LOG_CATEGORY);
                return data;
            }
            var imageUpdateRequest = new Dictionary<string, string>
            {
                {"imageUrl", newImageUrl}
            };
            return await VRCApi.Put<object, VRCAvatar>($"avatars/{id}", imageUpdateRequest, cancellationToken: cancellationToken);
        }

        [PublicAPI]
        public static async Task<VRCAvatar> UpdateAvatarBundle(string id, VRCAvatar data, string pathToBundle,
            Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pathToBundle))
            {
                Core.Logger.LogError("Bundle path cannot be empty", API.LOG_CATEGORY);
                return data;
            }
            var fileName = "Avatar - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" + ApiAvatar.VERSION.ApiVersion +
                          "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            
            string newBundleUrl = null;
            var currentAssetUrl = data.GetLatestAssetUrlForPlatform(Tools.Platform);
            
            // new platform
            if (string.IsNullOrWhiteSpace(currentAssetUrl))
            {
                newBundleUrl = await UploadFile(pathToBundle, "", fileName, onProgress, cancellationToken);
            }
            else
            {
                var fileId = ApiFile.ParseFileIdFromFileAPIUrl(currentAssetUrl);
                newBundleUrl = await UploadFile(pathToBundle, fileId, fileName, onProgress, cancellationToken);
            }
            if (string.IsNullOrWhiteSpace(newBundleUrl))
            {
                Core.Logger.LogError("new bundle url is empty, aborting", API.LOG_CATEGORY);
                return data;
            }
            var bundleUpdateRequest = new Dictionary<string, object>
            {
                {"assetUrl", newBundleUrl},
                {"platform", Tools.Platform.ToString()},
                {"unityVersion", Tools.UnityVersion.ToString()},
                {"assetVersion", 1}
            };
            Core.Logger.Log($"Updating with new bundle {newBundleUrl}", API.LOG_CATEGORY);
            await VRCApi.Put<object, VRCAvatar>($"avatars/{id}", bundleUpdateRequest, cancellationToken: cancellationToken);
            Core.Logger.Log("Fetching latest", API.LOG_CATEGORY);
            return await VRCApi.Get<VRCAvatar>($"avatars/{id}", forceRefresh: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates an avatar entry in the database.
        /// Use this method to get a new ID to assign to the avatar blueprint before building and uploading it.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onProgress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [PublicAPI]
        public static async Task<VRCAvatar> CreateAvatarRecord(VRCAvatar data, Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            var newAvatarData = new Dictionary<string, object>
            {
                {"name", data.Name},
                {"description", data.Description},
                {"tags", data.Tags},
                {"releaseStatus", data.ReleaseStatus},
                {"platform", Tools.Platform},
                {"unityVersion", Tools.UnityVersion.ToString()},
                {"assetVersion", 1}
            };
            var createdAvatar = await Post<Dictionary<string, object>, VRCAvatar>($"avatars", newAvatarData, cancellationToken: cancellationToken);
            return createdAvatar;
        }
        
        [PublicAPI]
        public static async Task<VRCAvatar> CreateNewAvatar(string id, VRCAvatar data, string pathToBundle, string pathToImage,
            Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pathToBundle) || string.IsNullOrWhiteSpace(pathToImage))
            {
                Core.Logger.LogError("Both bundle and image paths must be provided");
                return data;
            }
            
            var remoteData = await VRCApi.GetAvatar(id, forceRefresh: true, cancellationToken);
            if (!remoteData.PendingUpload)
            {
                throw new UploadException("Avatar creation is only allowed for a reserved avatar ID pending first upload");
            }
            
            var fileName = "Avatar - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" + ApiAvatar.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var newBundleUrl = await UploadFile(pathToBundle, "", fileName, onProgress: (status, percentage) =>
            {
                onProgress?.Invoke(status, percentage * 0.5f);
            }, cancellationToken);
            if (string.IsNullOrWhiteSpace(newBundleUrl))
            {
                Core.Logger.LogError("New bundle url is empty, aborting");
                return data;
            }
            var imageFileName = "Avatar - " + data.Name + " - Image - " + Application.unityVersion + "_" + ApiAvatar.VERSION.ApiVersion +
                           "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
            var fileId = ApiFile.ParseFileIdFromFileAPIUrl(data.ImageUrl);
            var newImageUrl = await UploadFile(pathToImage, "", imageFileName, onProgress: (status, percentage) =>
            {
                onProgress?.Invoke(status, 0.5f + percentage * 0.5f);
            }, cancellationToken);
            if (string.IsNullOrWhiteSpace(newImageUrl))
            {
                Core.Logger.LogError("New image url is empty, aborting");
                return data;
            }
            var newAvatarData = new Dictionary<string, object>
            {
                {"name", data.Name},
                {"description", data.Description},
                {"assetUrl", newBundleUrl},
                {"imageUrl", newImageUrl},
                {"tags", data.Tags},
                {"releaseStatus", data.ReleaseStatus},
                {"platform", Tools.Platform},
                {"unityVersion", Tools.UnityVersion.ToString()},
                {"assetVersion", 1}
            };
            // The expects either a style ID or no style keys
            if (data.Styles.Primary != null)
            {
                newAvatarData["primaryStyle"] = data.Styles.Primary;
            }
            if (data.Styles.Secondary != null)
            {
                newAvatarData["secondaryStyle"] = data.Styles.Secondary;
            }
            var createdAvatar = await Put<Dictionary<string, object>, VRCAvatar>($"avatars/{remoteData.ID}", newAvatarData, cancellationToken: cancellationToken);
            return createdAvatar;
        }

        [PublicAPI]
        public static async Task<VRCAvatar> DeleteAvatar(string id)
        {
            return await VRCApi.Delete<VRCAvatar>($"avatars/{id}", cancellationToken: CancellationToken.None);
        }

        [PublicAPI]
        public static async Task<VRCAvatar> SetAvatarAsFallback(string id, VRCAvatar data)
        {
            if (!data.Tags.Contains(AVATAR_FALLBACK_TAG))
            {
                data.Tags.Add(AVATAR_FALLBACK_TAG);
                data = await VRCApi.UpdateAvatarInfo(id, data);
            }
            await Put<EmptyResponse>($"avatars/{id}/selectFallback");
            return await Get<VRCAvatar>($"avatars/{id}", forceRefresh: true);
        }

        [PublicAPI]
        public static async Task<List<VRCAvatarStyle>> GetAvatarStyles()
        {
            return await VRCApi.Get<List<VRCAvatarStyle>>("avatarStyles");
        }

        public static async Task<VRCAgreement> ContentUploadConsent(VRCAgreement data)
        {
            return await Post<VRCAgreement, VRCAgreement>("agreement", data);
        }

        public static async Task<VRCAgreementCheckResponse> CheckContentUploadConsent(VRCAgreementCheckRequest data)
        {
            return await Get<VRCAgreementCheckResponse>("agreement", new Dictionary<string, string>
            {
                {"agreementCode", data.AgreementCode},
                {"contentId", data.ContentId},
                {"version", data.Version.ToString()}
            }, forceRefresh: true);
        }

        internal static async Task SubmitAssetReviewNotes(string id, string notes)
        {
            var avatarData = await GetAvatar(id, forceRefresh: true);
            if (string.IsNullOrWhiteSpace(avatarData.ActiveAssetReviewId))
            {
                Core.Logger.LogError("Avatar review doesn't exist, cannot submit notes");
                return;
            }
            
            var request = new VRCAssetReviewNotesRequest
            {
                ReviewNotes = notes
            };
            await Put<VRCAssetReviewNotesRequest, EmptyResponse>($"assetReview/{avatarData.ActiveAssetReviewId}/notes", request);
        }
        
        
        
        #endregion

        #region Public Utilities
        
        [PublicAPI]
        public static async Task<Texture2D> GetImage(string url, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            var imageBytes = await Get<byte[]>(url, forceRefresh: forceRefresh, cancellationToken: cancellationToken);
            var image = new Texture2D(512, 512)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            image.LoadImage(imageBytes);
            return image;
        }
        
        #endregion
        
        #region VRC API Internals

        private static async Task<string> UploadFile(string filename, string fileId, string friendlyFileName, Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        {
            // This setting often gets cleared on assembly reload, so we re-enable it here
            if (UnityEditor.EditorPrefs.GetBool("apiLoggingEnabled"))
            {
                Core.Logger.EnableCategory(API.LOG_CATEGORY);
            }
            
            var extension = Path.GetExtension(filename);
            var mimeType = VRCTools.GetMimeTypeFromExtension(extension);
            var creatingNewFile = fileId == "";
            VRCFile currentFile = default;
            if (!creatingNewFile)
            {
                currentFile = await Get<VRCFile>("file/" + fileId, forceRefresh: true, cancellationToken: cancellationToken);
                Core.Logger.Log($"Updating existing file {fileId}", API.LOG_CATEGORY);
            }
            else
            {
                var requestData = new Dictionary<string, string>
                {
                    {"name", friendlyFileName},
                    {"mimeType", mimeType},
                    {"extension", extension}
                };
                currentFile = await Post<Dictionary<string, string>, VRCFile>("file", requestData, cancellationToken: cancellationToken);
                Core.Logger.Log($"Created a new file {currentFile.ID}", API.LOG_CATEGORY);
            }
            
            onProgress?.Invoke("Preparing for file upload...", 0.0f);

            if (string.IsNullOrWhiteSpace(currentFile.ID))
            {
                Core.Logger.LogError("Failed to load file info, aborting", API.LOG_CATEGORY);
                return null;
            }

            if (currentFile.HasQueuedOperation())
            {
                Core.Logger.Log("Existing file is not fully uploaded, cleaning up", API.LOG_CATEGORY);
                await Delete($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}", cancellationToken: cancellationToken);
                Core.Logger.Log("Cleaned up leftover queued uploads", API.LOG_CATEGORY);
                await Task.Delay(1000, cancellationToken);
                // reload the file without a queued version
                currentFile = await Get<VRCFile>("file/" + fileId, forceRefresh: true, cancellationToken: cancellationToken);
            }

            if (currentFile.IsLatestVersionErrored())
            {
                Core.Logger.Log("Existing file failed to upload, cleaning up", API.LOG_CATEGORY);
                await Delete($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}", cancellationToken: cancellationToken);
                Core.Logger.Log("Cleaned up leftover errored uploads", API.LOG_CATEGORY);
                await Task.Delay(1000, cancellationToken);
                // reload the file without a broken version
                currentFile = await Get<VRCFile>("file/" + fileId, forceRefresh: true, cancellationToken: cancellationToken);
            }

            onProgress?.Invoke("Processing file...", 0.05f);

            byte[] fileMD5bytes;
            string fileMD5b64;
            try
            {
                fileMD5bytes = VRCTools.GetFileMD5(filename);
                fileMD5b64 = Convert.ToBase64String(fileMD5bytes);
            }
            catch (Exception e)
            {
                Core.Logger.LogError("Failed to get file MD5, exiting upload", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                return null;
            }

            var signatureFilePath = VRC.Tools.GetTempFileName(".sig", out var tempFileError, new GUID().ToString());
            if (!string.IsNullOrWhiteSpace(tempFileError))
            {
                Core.Logger.LogError("Failed to create temp file for signature, exiting upload", API.LOG_CATEGORY);
                Core.Logger.LogError(tempFileError, API.LOG_CATEGORY);
                return null;
            }

            await VRCTools.GenerateFileSignature(filename, signatureFilePath);
            VRC.Tools.GetFileSize(signatureFilePath, out var sigFileSize, out var sigFileSizeErrorStr);
            
            if (!string.IsNullOrWhiteSpace(sigFileSizeErrorStr))
            {
                Core.Logger.LogError("failed to get signature file size, exiting upload", API.LOG_CATEGORY);
                return null;
            }

            byte[] sigMD5bytes;
            string sigMD5b64;
            var sigMimeType = VRCTools.GetMimeTypeFromExtension(".sig");
            try
            {
                sigMD5bytes = VRCTools.GetFileMD5(signatureFilePath);
                sigMD5b64 = Convert.ToBase64String(sigMD5bytes);
            }
            catch (Exception e)
            {
                Core.Logger.LogError("Failed to get signature MD5, exiting upload", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message);
                return null;
            }
            
            VRC.Tools.GetFileSize(filename, out var fullFileSize, out var fileSizeErrorStr);

            if (!string.IsNullOrWhiteSpace(fileSizeErrorStr))
            {
                Core.Logger.LogError("failed to get file size, exiting upload", API.LOG_CATEGORY);
                return null;
            }
            
            onProgress?.Invoke("Checking existing records...", 0.1f);
            
            // Check for existing files with same hashes first
            var isRetrying = false;
            if (currentFile.HasExistingOrPendingVersion())
            {
                // if there is a file with the same hash
                if (string.Equals(fileMD5b64, currentFile.Versions[currentFile.GetLatestVersion()]?.File?.MD5 ?? ""))
                {
                    if (!currentFile.IsLatestVersionWaiting())
                    {
                        Core.Logger.LogError("file with the same hash is already uploaded", API.LOG_CATEGORY);
                        throw new UploadException("This file was already uploaded");
                    }

                    // file already exists and is in waiting state - we should try to upload
                    isRetrying = true;
                    Core.Logger.Log("File MD5 match, going to retry the file upload", API.LOG_CATEGORY);
                } else if (currentFile.IsLatestVersionWaiting())
                {
                    Core.Logger.Log("Latest file upload failed and we have a new file, we're going to clean up", API.LOG_CATEGORY);
                    await Delete($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}", cancellationToken: cancellationToken);
                    await Task.Delay(1000, cancellationToken);
                    // reload the file without a broken version
                    currentFile = await Get<VRCFile>("file/" + fileId, forceRefresh: true, cancellationToken: cancellationToken);
                }
            }
            
            // check that we're trying to re-upload the exact same file on retry
            var versionAlreadyExists = false;
            if (isRetrying)
            {
                var version = currentFile.Versions[currentFile.GetLatestVersion()];
                var isMatch = fullFileSize == version.File.SizeInBytes &&
                              string.Equals(fileMD5b64, version.File.MD5) &&
                              sigFileSize == version.Signature.SizeInBytes &&
                              string.Equals(sigMD5b64, version.Signature.MD5);
                if (isMatch)
                {
                    versionAlreadyExists = true;
                    Core.Logger.Log("Files match, will attempt to re-upload", API.LOG_CATEGORY);
                }
                else
                {
                    Core.Logger.Log("Files do not fully match, removing latest version", API.LOG_CATEGORY);
                    await Delete($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}", cancellationToken: cancellationToken);
                    await Task.Delay(1000, cancellationToken);
                    // reload the file without a broken version
                    currentFile = await Get<VRCFile>("file/" + fileId, forceRefresh: true, cancellationToken: cancellationToken);
                }
            }

            // if not retrying the last upload - create a new version entry
            if (!versionAlreadyExists)
            {
                var requestData = new Dictionary<string, object>
                {
                    {"signatureMd5", sigMD5b64},
                    {"signatureSizeInBytes", sigFileSize},
                    {"fileMd5", fileMD5b64},
                    {"fileSizeInBytes", fullFileSize},
                };
                var updatedFile = await Post<Dictionary<string, object>, VRCFile>($"file/{currentFile.ID}", requestData, cancellationToken: cancellationToken);
                if (string.IsNullOrWhiteSpace(updatedFile.ID))
                {
                    Core.Logger.LogError("Failed to create new file version, exiting upload", API.LOG_CATEGORY);
                    return null;
                }
                Core.Logger.Log($"Created new record. {currentFile.GetLatestVersion()} -> {updatedFile.GetLatestVersion()}", API.LOG_CATEGORY);
                currentFile = updatedFile;
                await Task.Delay(1000, cancellationToken);
            }
            else
            {
                Core.Logger.Log("File already exists, skipping record creation", API.LOG_CATEGORY);
            }

            Core.Logger.Log($"Is target version waiting? {currentFile.Versions[currentFile.GetLatestVersion()].File.Status == "waiting"}", API.LOG_CATEGORY);

            var fileDescriptor = currentFile.Versions[currentFile.GetLatestVersion()].File;
            var fileCategory = fileDescriptor.Category;
            
            Core.Logger.Log("File upload type: " + fileCategory, API.LOG_CATEGORY);
            
            onProgress?.Invoke("Starting file upload...", 0.15f);

            if (currentFile.Versions[currentFile.GetLatestVersion()].File.Status == "waiting")
            {
                Core.Logger.Log("Starting file upload", API.LOG_CATEGORY);
                if (fileCategory != "simple")
                {
                    if (await UploadMultipart(filename, FileUploadType.File, currentFile, mimeType, fileMD5bytes,
                            (status, percentage) => { onProgress?.Invoke(status, 0.15f + percentage * 0.75f); }, cancellationToken))
                    {
                        // cleanup the file if we created it
                        if (creatingNewFile)
                        {
                            Core.Logger.Log("Cleanup, deleting created file", API.LOG_CATEGORY);
                            await Delete($"file/{currentFile.ID}", cancellationToken: cancellationToken);
                        }

                        throw new UploadException("Failed to upload file");
                    }
                }
                else
                {
                    if (await UploadSimple(filename, FileUploadType.File, currentFile, mimeType, fileMD5bytes,
                            (status, percentage) => { onProgress?.Invoke(status, 0.15f + percentage * 0.75f); }, cancellationToken))
                    {
                        // cleanup the file if we created it
                        if (creatingNewFile)
                        {
                            Core.Logger.Log("Cleanup, deleting created file", API.LOG_CATEGORY);
                            await Delete($"file/{currentFile.ID}", cancellationToken: cancellationToken);
                        }

                        throw new UploadException("Failed to upload file");
                    }
                }
                onProgress?.Invoke("File Uploaded!", 0.9f);
                await Task.Delay(1000, cancellationToken);
            }
            else
            {
                Core.Logger.Log("File upload not waiting, thus isn't needed", API.LOG_CATEGORY);
                throw new UploadException("This file was already uploaded, you should make a new build");
            }

            fileDescriptor = currentFile.Versions[currentFile.GetLatestVersion()].Signature;
            fileCategory = fileDescriptor.Category;
            
            onProgress?.Invoke("Starting signature upload...", 0.95f);
            
            if (currentFile.Versions[currentFile.GetLatestVersion()].Signature.Status == "waiting")
            {
                Core.Logger.Log("Starting signature upload", API.LOG_CATEGORY);
                if (fileCategory != "simple")
                {
                    if (await UploadMultipart(signatureFilePath, FileUploadType.Signature, currentFile,
                            sigMimeType, sigMD5bytes,
                            (status, percentage) => { onProgress?.Invoke(status, 0.95f + percentage * 0.04f); }, cancellationToken))
                    {
                        // cleanup the file if we created it
                        if (creatingNewFile)
                        {
                            Core.Logger.Log("Cleanup, deleting created file", API.LOG_CATEGORY);
                            await Delete($"file/{currentFile.ID}", cancellationToken: cancellationToken);
                        }

                        return null;
                    }
                }
                else
                {
                    if (await UploadSimple(signatureFilePath, FileUploadType.Signature, currentFile,
                            sigMimeType, sigMD5bytes,
                            (status, percentage) => { onProgress?.Invoke(status, 0.95f + percentage * 0.04f); }, cancellationToken))
                    {
                        // cleanup the file if we created it
                        if (creatingNewFile)
                        {
                            Core.Logger.Log("Cleanup, deleting created file", API.LOG_CATEGORY);
                            await Delete($"file/{currentFile.ID}", cancellationToken: cancellationToken);
                        }

                        return null;
                    }
                }
                onProgress?.Invoke("Signature Uploaded!", 0.99f);
                await Task.Delay(1000, cancellationToken);
            }
            else
            {
                Core.Logger.Log("Signature upload not waiting, thus isn't needed", API.LOG_CATEGORY);
                return null;
            }
            
            Core.Logger.Log("Everything should be now uploaded", API.LOG_CATEGORY);
            currentFile = await Get<VRCFile>($"file/{currentFile.ID}", forceRefresh: true, cancellationToken: cancellationToken);
            var latestVersion = currentFile.Versions[currentFile.GetLatestVersion()];
            
            Core.Logger.Log($"File upload complete? {latestVersion.File.Status == "complete"}", API.LOG_CATEGORY);
            Core.Logger.Log($"Signature upload complete? {latestVersion.Signature.Status == "complete"}", API.LOG_CATEGORY);
            
            Core.Logger.Log("waiting for file to finish processing", API.LOG_CATEGORY);
            onProgress?.Invoke("Refreshing data...", 0.99f);
            Core.Logger.Log("waiting for 5s", API.LOG_CATEGORY);
            await Task.Delay(5000, cancellationToken);
            
            Core.Logger.Log("Everything should be good now", API.LOG_CATEGORY);

            onProgress?.Invoke($"Cleaning up Temp Files...", 0.99f);
            await VRCTools.CleanupTempFiles(currentFile.ID);

            currentFile = await Get<VRCFile>($"file/{currentFile.ID}", forceRefresh: true, cancellationToken: cancellationToken);
            
            onProgress?.Invoke("File upload finished", 1.0f);

            return currentFile.Versions[currentFile.GetLatestVersion()].File.URL;
        }

        private enum FileUploadType
        {
            File,
            Signature
        }

        private static async Task<bool> UploadSimple(string filename, FileUploadType fileUploadType, VRCFile currentFile, string mimeType, byte[] fileContent, Action<string, float> onProgress, CancellationToken cancellationToken = default)
        {
            var startUploadResp = await Put<JObject>($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}/{fileUploadType.ToString().ToLowerInvariant()}/start", cancellationToken: cancellationToken);
            var uploadUrl = startUploadResp.Value<string>("url");

            Core.Logger.Log($"got upload url {uploadUrl}", API.LOG_CATEGORY);
            if (string.IsNullOrWhiteSpace(uploadUrl))
            {
                Core.Logger.LogError("Got invalid upload url, exiting upload", API.LOG_CATEGORY);
                return true;
            }
            
            var fileData = await File.ReadAllBytesAsync(filename, cancellationToken);

            onProgress?.Invoke($"Uploading {fileUploadType.ToString().ToLowerInvariant()}...", 0f);
            try
            {
                await MakeRequest<byte[], EmptyResponse>(uploadUrl, HttpMethod.Put, body: fileData, contentType: mimeType,
                    contentMD5: fileContent,
                    timeout: 60 * 60,
                    onProgress: (percentage) => { onProgress?.Invoke($"Uploading {fileUploadType.ToString()} ({(percentage * 100):F0}%)...", percentage); }, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                Core.Logger.LogError($"failed to upload {fileUploadType.ToString()} to {uploadUrl}, exiting upload", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                return true;
            }

            await Put<EmptyResponse>($"file/{currentFile.ID}/{currentFile.GetLatestVersion()}/{fileUploadType.ToString().ToLowerInvariant()}/finish", cancellationToken: cancellationToken);
            Core.Logger.Log($"{fileUploadType.ToString()} upload complete", API.LOG_CATEGORY);
            
            return false;
        }

        private static async Task<bool> UploadMultipart(string filename, FileUploadType fileUploadType, VRCFile currentFile,
            string mimeType, byte[] fileContent, Action<string, float> onProgress, CancellationToken cancellationToken = default)
        {
            JObject uploadStatus;
            try
            {
                uploadStatus =
                    await Get<JObject>(
                        $"file/{currentFile.ID}/{currentFile.GetLatestVersion()}/{fileUploadType.ToString().ToLowerInvariant()}/status",
                        cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                Core.Logger.LogError("Failed to get current file status, aborting upload", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                return true;
            }
            var nextPartNumber = 1;
            var etags = new List<string>();
            nextPartNumber += uploadStatus.Value<int>("nextPartNumber");

            var statusEtags = uploadStatus.Value<JArray>("etags").ToObject<List<string>>();
            etags.AddRange(statusEtags);
            Core.Logger.Log($"Loaded up existing etags: {string.Join(", ", statusEtags)}", API.LOG_CATEGORY);

            await using var fileStream = File.OpenRead(filename);
            var parts = Mathf.Max(1, Mathf.FloorToInt((float)fileStream.Length / (float)MULTIPART_PART_SIZE));
                
            if (nextPartNumber > 1)
            {
                onProgress?.Invoke("Resuming upload...", (float) nextPartNumber / parts);
            }

            var perPartProgress = 1f / parts;

            // intermediary buffer
            var buffer = new byte[MULTIPART_PART_SIZE * 2];
            for (var partNumber = nextPartNumber; partNumber <= parts; partNumber++)
            {
                var partProgressStart = (float) (partNumber - 1) / parts;
                if (cancellationToken.IsCancellationRequested)
                {
                    Core.Logger.Log("Upload cancelled", API.LOG_CATEGORY);
                    return true;
                }

                JObject startUploadResp;
                try
                {
                    startUploadResp = await Put<JObject>(
                        $"file/{currentFile.ID}/{currentFile.GetLatestVersion()}/{fileUploadType.ToString().ToLowerInvariant()}/start",
                        queryParams: new Dictionary<string, string> { { "partNumber", partNumber.ToString() }},
                        cancellationToken: cancellationToken);
                }
                catch (Exception e)
                {
                    Core.Logger.LogError($"Failed to start upload for part {partNumber}", API.LOG_CATEGORY);
                    Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                    return true;
                }

                var uploadUrl = startUploadResp.Value<string>("url");
                    
                Core.Logger.Log($"got upload url {uploadUrl}", API.LOG_CATEGORY);
                if (string.IsNullOrWhiteSpace(uploadUrl))
                {
                    Core.Logger.LogError("Got invalid upload url, exiting upload", API.LOG_CATEGORY);
                    return true;
                }

                onProgress?.Invoke($"Uploading {fileUploadType.ToString()}...", partProgressStart);
                    
                var bytesToRead = partNumber < parts ? MULTIPART_PART_SIZE : (int)(fileStream.Length - fileStream.Position);
                var bytesRead = 0;
                try
                {
                    bytesRead = await fileStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);
                }
                catch (Exception e)
                {
                    Core.Logger.LogError("Could not read file, aborting", API.LOG_CATEGORY);
                    Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                    return true;
                }
                    
                Core.Logger.Log($"Loaded {bytesRead} from file", API.LOG_CATEGORY);

                var sizedArray = new byte[bytesRead];
                Array.Copy(buffer, sizedArray, bytesRead);
                    
                try
                {
                    var result = await MakeRequestWithResponse<byte[], EmptyResponse>(
                        uploadUrl, 
                        HttpMethod.Put, 
                        body: sizedArray,
                        timeout: 60 * 60,
                        onProgress: (percentage) =>
                        {
                            onProgress?.Invoke($"Uploading {fileUploadType.ToString()} ({(percentage * 100):F0}%)...", partProgressStart + percentage * perPartProgress);
                        }, 
                        cancellationToken: cancellationToken
                    );
                    if (result.responseMessage.Headers.ETag != null)
                    {
                        Core.Logger.Log($"Got an etag {result.responseMessage.Headers.ETag.Tag.Trim('"', '\'')} from S3", API.LOG_CATEGORY);
                        etags.Add(result.responseMessage.Headers.ETag.Tag.Trim('"', '\''));
                    }
                }
                catch (Exception e)
                {
                    Core.Logger.LogError($"failed to upload {fileUploadType.ToString()} to {uploadUrl}, exiting upload", API.LOG_CATEGORY);
                    Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                    return true;
                }

                await Task.Delay(1000, cancellationToken);
                Core.Logger.Log($"Uploaded part {partNumber} out of {parts}", API.LOG_CATEGORY);
            }

            try
            {
                var requestData = new Dictionary<string, List<string>>
                {
                    {"etags", etags}
                };
                await Put<Dictionary<string, List<string>>, EmptyResponse>(
                    $"file/{currentFile.ID}/{currentFile.GetLatestVersion()}/{fileUploadType.ToString().ToLowerInvariant()}/finish",
                    cancellationToken: cancellationToken, body: requestData);
            }
            catch (Exception e)
            {
                Core.Logger.LogError("Failed to finish upload", API.LOG_CATEGORY);
                Core.Logger.LogError(e.Message, API.LOG_CATEGORY);
                return true;
            }
                
            Core.Logger.Log($"{fileUploadType.ToString()} upload complete", API.LOG_CATEGORY);

            return false;
        }

        #endregion

        #region Handlers

        internal class DateTimeNoneHandler : JsonConverter<DateTime>
        {
            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var value = reader.Value.ToString();
                if (reader.TokenType == JsonToken.String)
                {
                    if (value == "none")
                    {
                        return DateTime.MinValue;
                    }
                }
                return DateTime.Parse(value);
            }

            public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
            {
                if (value == DateTime.MinValue)
                {
                    writer.WriteValue("none");
                }
                else
                {
                    writer.WriteValue(value.ToString());
                }
            }
        }

        // Instances come in as an array of arrays, like [ [ Id, Occupant count ], ... ]
        internal class InstanceHandler : JsonConverter<VRCWorld.Instance>
        {
            public override VRCWorld.Instance ReadJson(JsonReader reader, Type objectType, VRCWorld.Instance existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                var array = JArray.Load(reader);
                return new VRCWorld.Instance
                {
                    ID = (string) array[0],
                    Occupants = (int) array[1]
                };
            }
            
            public override void WriteJson(JsonWriter writer, VRCWorld.Instance value, JsonSerializer serializer)
            {
                var array = new JArray
                {
                    value.ID,
                    value.Occupants
                };
                array.WriteTo(writer);
            }
        }
        #endregion
    }
}
