using DataGate.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DataGate.Com;
using DataGate.App.Files;

namespace DataGate.Tests
{
    public class UploadTest : AppDataTest
    {
        public UploadTest()
        {
            _client = base.CreateClient();
        }

        /// <summary>
        /// 测试文件上传
        /// </summary>
        /// <param name="filePath">以当前运行目录作为根目录的文件相对路径</param>
        /// <returns>上传后的文件ID</returns>
        [Theory]
        [InlineData("TestData/Endless Space 2.jpg")]
        [InlineData("TestData/witcher3.png")]
        public async Task TestUploadFile(string filePath)
        {
            string fullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            string fileId = await UploadFile(new FileInfo(fullName), null);
            Assert.False(fileId.IsEmpty());
        }

        HttpClient _client;
        //测试分块，单元测试的分块设置得小一点看效果
        static readonly int BLOCK_SIZE = 32768;
        async Task<string> UploadFile(FileInfo fi, Action<int> uploadPorgress = null)
        {
            var fileStream = fi.OpenRead();
            UploadRequest request = new UploadRequest
            {
                FileName = fi.Name,
                Chunk = 0,
                Chunks = (int)((fileStream.Length - 1) / BLOCK_SIZE + 1),
                Guid = Guid.NewGuid().ToString("N"),
            };
            if (fi.Length <= BLOCK_SIZE)
            {
                request.Guid = null;
            }
            UploadResult result = null;
            byte[] buffer = new byte[BLOCK_SIZE];
            int offset = 0;
            int readCount = 0;
            while (offset < fi.Length && (readCount = fileStream.Read(buffer, 0, BLOCK_SIZE)) > 0)
            {
                offset += readCount;
                var memoryStream = new MemoryStream(buffer, 0, readCount);
                result = await UploadAsync<UploadResult>($"/api/dg/u?FileName={request.FileName}&Chunk={request.Chunk}" +
                    $"&Chunks={request.Chunks}&Guid={request.Guid}", request.FileName, memoryStream);
                memoryStream.Dispose();
                request.Chunk++;
                uploadPorgress?.Invoke(request.Chunk * 100 / request.Chunks);
            }
            //大文件分段上传时，要多一个请求以返回合并后的实际文件ID
            if (request.Guid != null)
            {
                var resultUrl = $"/api/dg/u?FileName={request.FileName}&Chunk={request.Chunks}" +
                        $"&Chunks={request.Chunks}&Guid={request.Guid}";
                var response = await _client.PostAsync(resultUrl, null);
                result = await response.Content.ReadAsAsync<UploadResult>();
            }
            return result.Id;
        }

        /// <summary>上传文件</summary>
        async Task<T> UploadAsync<T>(string url, string fileName, Stream stream)
        {
            var httpContent = BuildMultipartFormDataContent(fileName, stream);
            var response = await SendAsync(HttpMethod.Post, url, httpContent);
            return await ReadContentAsync<T>(response);
        }
        /// <summary>检查是否成功</summary>
        protected virtual void EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
        }

        /// <summary>读取应答数据</summary>
        private async Task<T> ReadContentAsync<T>(HttpResponseMessage response)
        {
            EnsureSuccess(response);
            if (response.Content.Headers.ContentType != null)
            {
                string str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }

        /// <summary>发送</summary>
        private async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string url, HttpContent httpContent)
        {
            var request = new HttpRequestMessage(httpMethod, url);
            request.Method = httpMethod;
            request.Content = httpContent;
            //if (this.TokenProvider != null)
            //{
            //    var token = this.TokenProvider.GetToken(this.ExecuteContext);
            //    if (!token.IsNullOrEmpty()) request.Headers.Authorization = new AuthenticationHeaderValue(PKSWebExtension.AuthorizationSchema, token);
            //}
            //OnSending(request);
            return await _client.SendAsync(request);
        }

        /// <summary>生成多部分内容</summary>
        private HttpContent BuildMultipartFormDataContent(string fileName, Stream stream)
        {
            var httpContent = new MultipartFormDataContent();
            var streamContent = BuildStreamContent(fileName, stream);
            httpContent.Add(streamContent);
            //httpContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            return httpContent;
        }

        /// <summary>生成文件流内容</summary>
        private StreamContent BuildStreamContent(string fileName, Stream stream)
        {
            var streamContent = new StreamContent(stream);
            UseContentDisposition(streamContent, fileName, true);
            // if (!charSet.IsNullOrEmpty()) streamContent.Headers.ContentType.CharSet = charSet;
            return streamContent;
        }

        /// <summary>设置文件内容头</summary>
        static void UseContentDisposition(HttpContent httpContent, string fileName, bool setContentType = true)
        {
            fileName = Path.GetFileName(fileName);
            httpContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = @"""attachment""",
                FileName = $@"""{fileName}"""
            };
            if (setContentType)
            {
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(IOHelper.GetContentType(fileName));
            }
        }

        private async Task<string> PostJsonGetString<T>(string url, T request = default(T))
        {
            var result = await _client.PostAsJsonAsync(url, request);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
