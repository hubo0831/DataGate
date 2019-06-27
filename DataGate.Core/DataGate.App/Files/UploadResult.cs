using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.Files
{

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

        /// <summary>
        /// 文件的MD5 (v0.2.1+)
        /// </summary>
        public string Md5 { get; set; }
    }
}
