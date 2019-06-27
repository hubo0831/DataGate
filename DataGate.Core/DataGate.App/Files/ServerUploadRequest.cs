using DataGate.Com;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.Files
{
    /// <summary>服务端文件上传请求</summary>
    public class ServerUploadRequest : UploadRequest
    {
        /// <summary>文件上传请求</summary>
        public ServerUploadRequest(UploadRequest request)
        {
            if (request == null) return;
            this.Md5 = request.Md5;
            this.Guid = request.Guid;
            this.Chunk = request.Chunk;
            this.Chunks = request.Chunks;
            this.FileName = request.FileName;
        }

        public ServerUploadRequest()
        {

        }

        /// <summary>服务端接收文件</summary>
        public string ServerFile { get; set; }

        /// <summary>
        /// 上传文件的用户ID
        /// </summary>
        public string UserId { get; set; }

    }
}
