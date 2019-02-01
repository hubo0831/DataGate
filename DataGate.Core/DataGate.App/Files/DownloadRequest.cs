using System.IO;

namespace DataGate.App.Files
{
    /// <summary>文件下载请求</summary>
    /// <remarks>
    /// 有2种下载方式
    /// 1. 通过DataId和Source下载指定的文件
    /// 2. 通过StorageType和ContentRef下载
    /// </remarks>
    public class DownloadRequest 
    {
        /// <summary>是否使用下载保存文件方式，默认是False</summary>
        public bool Download { get; set; }
        /// <summary>数据ID</summary>
        public string DataId { get; set; }
        /// <summary>文件名称</summary>
        /// <remarks>如果文件名称存在，则为下载文件名并设置ContentType头</remarks>
        public string FileName { get; set; }
        public string ContentRef { get; internal set; }
    }

}