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

        /// <summary>
        /// 上传时的文件所在路径
        /// </summary>
        public string FilePath { get; set; }
    }
}