using DataGate.Com;
using System;

namespace DataGate.App.Files
{
    /// <summary>上传文件</summary>
    public class SysFile : IId<string>
    {
        /// <summary>文件ID</summary>
        public string Id { get; set; }

        /// <summary>客户端时的文件名称</summary>
        public string Name { get; set; }

        /// <summary>文件在客户端时的路径</summary>
        public string Path { get; set; }

        /// <summary>服务端的相对上传路径</summary>
        public string RelativePath { get; set; }

        /// <summary>Http媒体类型头值</summary>
        public string ContentType { get; set; }

        /// <summary>文件MD5</summary>
        public string Md5 { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 上传人的用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Size { get; internal set; }
    }
}