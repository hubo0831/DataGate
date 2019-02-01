using DataGate.Com;
using System.Collections.Generic;
namespace DataGate.App.Files
{
    /// <summary>文件上传请求</summary>
    public class UploadRequest
    {
        /// <summary>上传文件的MD5</summary>
        public string Md5 { get; set; }
        /// <summary>文件分片上传时的唯一标识</summary>
        public string Guid { get; set; }

        /// <summary>分片索引</summary>
        public int Chunk { get; set; } = -1;
        /// <summary>分片数量</summary>
        public int Chunks { get; set; } = -1;
        /// <summary>文件名称</summary>
        public string FileName { get; set; }
        /// <summary>字符集</summary>
        public string CharSet { get; set; }
        /// <summary>生成JSON串</summary>
    }

    /// <summary>服务端文件上传请求</summary>
    public class ServerUploadRequest : UploadRequest
    {
        /// <summary>文件上传请求</summary>
        public ServerUploadRequest() { }
        /// <summary>文件上传请求</summary>
        public ServerUploadRequest(UploadRequest request)
        {
            if (request == null) return;
            this.Md5 = request.Md5;
            this.Guid = request.Guid;
            this.Chunk = request.Chunk;
            this.Chunks = request.Chunks;
            this.FileName = request.FileName;
            this.CharSet = request.CharSet;
        }

        /// <summary>服务端接收文件</summary>
        public string ServerFile { get; set; }
        /// <summary>离线上传文件路径</summary>
        public string RelativePath { get; set; }
        /// <summary>生成JSON串</summary>
    }

    /// <summary>上传结果</summary>
    public class UploadResult
    {
        /// <summary>文件ID</summary>
        public string Id { get; set; }

        /// <summary>分片索引</summary>
        public int Chunk { get; set; } = -1;

        /// <summary>
        /// 上传中，检测到重复的文件 wang加
        /// </summary>
        public bool Dup { get; set; }
    }
}