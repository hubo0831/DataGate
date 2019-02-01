using DataGate.App.DataService;
using System;
using System.IO;

namespace DataGate.App.Files
{
    /// <summary>Mongo库配置实现</summary>
    public class UploadConfig : ISingleton
    {
        /// <summary>构造实现</summary>
        public UploadConfig()
        {
            var configSection =Consts.Config.GetSection("Upload");


            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
            this.UploadFilesDir = configSection["IndexUploadFilesPath"].Trim('\\');
            //Path是绝对路径
            if (this.UploadFilesDir.Contains(":"))
            {
                this.UploadFilesPath = UploadFilesDir;
            }
            //Path是相对路径
            else
            {
                this.UploadFilesPath = Path.Combine(rootPath, UploadFilesDir);
            }
            this.UploadTempPath = Path.Combine(rootPath ,
                configSection["IndexUploadTempPath"].Trim('\\'));
            if (!Directory.Exists(UploadTempPath))
            {
                Directory.CreateDirectory(UploadTempPath);
            }
            if (!Directory.Exists(UploadFilesPath))
            {
                Directory.CreateDirectory(UploadTempPath);
            }
        }

        /// <summary>上传文件保存路径，第一级是年月(201707)，第二级是日小时(0808)</summary>
        public string UploadFilesPath { get; }
        /// <summary>上传文件临时路径</summary>
        public string UploadTempPath { get; set; }
        public string UploadFilesDir { get; private set; }
    }
}