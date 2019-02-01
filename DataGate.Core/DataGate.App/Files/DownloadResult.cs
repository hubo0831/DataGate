using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataGate.App.Files
{
    /// <summary>文件下载结果</summary>
    public class DownloadResult
    {
        /// <summary>文件名</summary>
        public string FileName { get; set; }
        /// <summary>内容类型</summary>
        public string ContentType { get; set; }
        /// <summary>内容流</summary>
        public Stream Content { get; set; }
    }
}
