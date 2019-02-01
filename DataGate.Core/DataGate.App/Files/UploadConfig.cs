using DataGate.App.DataService;
using System;
using System.IO;

namespace DataGate.App.Files
{
    /// <summary>Mongo库配置实现</summary>
    public class UploadConfig : ISingleton
    {
        /// <summary>上传文件保存路径，第一级是年月(201707)，第二级是日小时(0808)</summary>
        public string UploadFilesPath { get; }

        /// <summary>上传文件临时路径</summary>
        public string TempPath { get; set; }

        /// <summary>上传文件存放目录</summary>
        public string FilesDir { get; private set; }

        /// <summary>构造实现</summary>
        public UploadConfig()
        {
            var configSection = Consts.Config.GetSection("Upload");
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
            this.FilesDir = configSection[nameof(FilesDir)].Trim('\\');
            //Path是绝对路径
            if (this.FilesDir.Contains(":"))
            {
                this.UploadFilesPath = FilesDir;
            }
            //Path是相对路径
            else
            {
                this.UploadFilesPath = Path.Combine(rootPath, FilesDir);
            }
            this.TempPath = Path.Combine(rootPath,
                configSection[nameof(TempPath)].Trim('\\'));
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            if (!Directory.Exists(UploadFilesPath))
            {
                Directory.CreateDirectory(TempPath);
            }
        }

    }
}