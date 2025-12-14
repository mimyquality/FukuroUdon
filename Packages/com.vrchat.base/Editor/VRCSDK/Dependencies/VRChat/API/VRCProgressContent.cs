using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VRC.Core;

namespace VRC.SDKBase.Editor.Api
{
    internal class VRCProgressContent: HttpContent
    {
        private readonly HttpContent _source;
        private readonly Action<float> _onProgress;
        
        public VRCProgressContent(HttpContent source, Action<float> onProgress)
        {
            _source = source;
            _onProgress = onProgress;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var sendBuffer = new byte[4 * 1024 * 1024];
            var totalBytesRead = 0L;

            using (var inputStream = await _source.ReadAsStreamAsync())
            {
                var totalBytes = inputStream.Length;
                var bytesRead = 0;

                while ((bytesRead = await inputStream.ReadAsync(sendBuffer, 0, sendBuffer.Length)) > 0)
                {
                    await stream.WriteAsync(sendBuffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    _onProgress((float)totalBytesRead / totalBytes);
                    Core.Logger.Log($"Sent {totalBytesRead} out of {totalBytes}", API.LOG_CATEGORY);
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _source.Headers.ContentLength.GetValueOrDefault();
            return true;
        }
    }
}