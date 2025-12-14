using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("VRC.SDK3A.Editor")]
[assembly: InternalsVisibleTo("VRC.SDK3.Editor")]
namespace VRC.SDKBase.Editor.Api
{
    /// <summary>
    /// VRCApi request failed
    /// </summary>
    public class RequestFailedException : Exception
    {
        public HttpResponseMessage HttpMessage;
        public HttpStatusCode StatusCode;
        
        public RequestFailedException(string message) : base(message)
        {
        }
        
        public RequestFailedException(string message, HttpResponseMessage httpMessage) : base(message)
        {
            HttpMessage = httpMessage;
        }
        
        public RequestFailedException(string message, HttpResponseMessage httpMessage, HttpStatusCode statusCode) : base(message)
        {
            HttpMessage = httpMessage;
            StatusCode = statusCode;
        }
    }
    
    public class JsonSerializationException : Exception
    {
        public JsonSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// VRChat API returned an error
    /// </summary>
    public class ApiErrorException : Exception
    {
        public HttpResponseMessage HttpMessage;
        public HttpStatusCode StatusCode;
        public string ErrorMessage;

        public ApiErrorException(HttpResponseMessage httpMessage, string jsonContent)
        {
            var json = JsonConvert.DeserializeObject<VRCApiError>(jsonContent, VRCApi.JSON_OPTIONS);
            HttpMessage = httpMessage;
            StatusCode = httpMessage.StatusCode;
            ErrorMessage = json.Error.Message;
        }
    }
}