using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataGate.App.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataGate.Com;
using DataGate.App;

namespace DataGate.Api.Controllers
{
    /// <summary>
    /// 文件的上传和下载
    /// </summary>
    public class FilesController : BaseController
    {
        private UploadFileService _fileService;

        public FilesController(UploadFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>上传文件，支持秒传和分片</summary>
        [HttpPost]
        public async Task<UploadResult> Upload(UploadRequest request)
        {
            var userSession = GetSession();
            ServerUploadRequest request2 = new ServerUploadRequest(request);
            request2.UserId = userSession?.Id;

            if (request2.Chunk < request2.Chunks || request2.Guid.IsEmpty())
            {
                IFormFile file = Request.Form.Files[0];
                request2.FileName = file.FileName;

                string serverFile = Path.Combine(_fileService.TempPath, Guid.NewGuid().ToString());

                FileStream fs = System.IO.File.Create(serverFile);
                file.CopyTo(fs);
                fs.Close();
                request2.ServerFile = serverFile;
            }

            return await _fileService.UploadAsync(request2);
        }

        /// <summary>
        /// 根据文件ID下载文件 wang加
        /// </summary>
        /// <returns>下载流</returns>
        [HttpGet]
        public async Task<IActionResult> DownById()
        {
            string id = (string)this.ControllerContext.RouteData.Values["id"];
            string filename = (string)this.ControllerContext.RouteData.Values["filename"];
            var result = await _fileService.DownloadAsync(id);
            return File(result.Content, result.ContentType, filename ?? result.FileName);
        }
    }
}