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
    [ApiController]
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
            IFormFile file = Request.Form.Files.FirstOrDefault();
            ServerUploadRequest request2 = new ServerUploadRequest()
            {
                Chunk = request.Chunk,
                Chunks = request.Chunks,
                Guid = request.Guid,
                Md5 = request.Md5,
                UserId = userSession?.Id
            };
            if (file != null)
            {
                string serverFile = Path.Combine(_fileService.TempPath, Guid.NewGuid().ToString());

                using (FileStream fs = System.IO.File.Create(serverFile))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                request2.ServerFile = serverFile;
                request2.FileName = file.FileName;
            }

            return await _fileService.UploadAsync(request2);
        }

        /// <summary>
        /// 根据文件ID下载文件 wang加
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <param name="filename">（可选)下载后的文件名称</param>
        /// <returns>下载流</returns>
        [HttpGet]
        public async Task<IActionResult> DownById(string id, string filename = null)
        {
            filename = filename ?? (string)this.ControllerContext.RouteData.Values["filename"];
            var result = await _fileService.DownloadAsync(id);
            return File(result.Content, result.ContentType, filename ?? result.FileName);
        }
    }
}