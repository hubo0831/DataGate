using DataGate.Com;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataGate.App.Files
{
    /// <summary>上传文件处理器</summary>
    public class UploadFileService
    {
        SysFileMan _fileMan;
        /// <summary>构造上传文件处理器</summary>
        public UploadFileService(SysFileMan fileMan, UploadConfig config)
        {
            _fileMan = fileMan;
            this._uploadPath = config.UploadFilesPath;
            this.TempPath = config.TempPath;
            //  ClearTempFiles();
        }

        /// <summary>上传路径</summary>
        string _uploadPath;

        /// <summary>上传临时路径</summary>
        public string TempPath { get; private set; }

        /// <summary>处理文件上传</summary>
        public async Task<UploadResult> UploadAsync(ServerUploadRequest request)
        {
            UploadResult result = null;
            if (!request.Md5.IsEmpty())
            {
                result = await HandleClientMD5Async(request);
                if (!result.Id.IsEmpty())
                {
                    return result;
                }
            }
            if (request.Guid.IsEmpty())
            {
                result = await HandleSingleAsync(request);
            }
            else
            {
                result = await HandleChunkAsync(request);
            }
            return result;
        }

        /// <summary>秒传,需要客户端提供文件的MD5</summary>
        private async Task<UploadResult> HandleClientMD5Async(ServerUploadRequest request)
        {
            UploadResult result = new UploadResult();
            if (request.Md5.IsEmpty()) return result;
            var existsDoc = await _fileMan.GetByMd5Async(request.Md5);
            if (existsDoc != null)
            {
                var docFile = _uploadPath + existsDoc.RelativePath;
                if (File.Exists(docFile))
                {
                    //如果相同的文件已存在，则新增一条指向原有文件的记录
                    existsDoc.Id = null;
                    existsDoc.Name = request.FileName;
                    existsDoc.Path = request.FilePath;
                    await _fileMan.InsertAsync(existsDoc);
                    result.Id = existsDoc.Id;
                }
                result.Dup = true;
            }
            return result;
        }

        /// <summary>处理单文件上传</summary>
        private async Task<UploadResult> HandleSingleAsync(ServerUploadRequest request)
        {
            UploadResult result = new UploadResult();
            var doc = await BuildNewCheckExists(request, result);
            result.Id = doc.Id;
            result.Md5 = doc.Md5;
            return result;
        }

        /// <summary>处理分片文件上传</summary>
        private async Task<UploadResult> HandleChunkAsync(ServerUploadRequest request)
        {
            UploadResult result = new UploadResult();

            if (request.Chunk < 0 || request.Chunks < 1 || request.Chunk > request.Chunks)
            {
                throw new Exception("分片参数无效！");
            }
            var uploadMD5 = request.Md5;
            result.Chunk = request.Chunk;
            if (request.Chunk < request.Chunks)
            {
                if (request.ServerFile.IsEmpty())
                {
                    throw new Exception("分片上传时流不存在！");
                }
                var chunkFile = GetNewChunkFile(request, request.Chunk);
                if (File.Exists(chunkFile))
                {
                    File.Delete(chunkFile);
                }
                File.Move(request.ServerFile, chunkFile);
            }
            else
            {
                #region 合并分片文件
                var mergeFile = GetNewMergeFile(request);
                List<string> chunkFiles = new List<string>();
                using (var mergeStream = new FileStream(mergeFile, FileMode.Create, FileAccess.Write))
                {
                    for (var i = 0; i < request.Chunks; i++)
                    {
                        var chunkFile = GetNewChunkFile(request, i);
                        using (var chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read))
                        {
                            await chunkStream.CopyToAsync(mergeStream);
                        }
                        chunkFiles.Add(chunkFile);
                    }
                }
                request.ServerFile = mergeFile;
                var doc = await BuildNewCheckExists(request, result);
                result.Md5 = doc.Md5;
                result.Id = doc.Id;
                chunkFiles.ForEach(File.Delete);
                #endregion
            }
            return result;
        }

        /// <summary>生成新文档，如已存在则直接返回已存在文档 wang加</summary>
        private async Task<SysFile> BuildNewCheckExists(ServerUploadRequest request, UploadResult result)
        {
            var doc = BuildNew(request);
            var uploadMD5 = request.Md5;
            doc.Md5 = BuildMD5(request.ServerFile, uploadMD5);
            doc.Size = new FileInfo(request.ServerFile).Length;
            var existsDoc = await _fileMan.GetByMd5Async(doc.Md5);
            if (existsDoc == null)
            {
                var docFile = _uploadPath + doc.RelativePath;
                File.Move(request.ServerFile, docFile);
            }
            else //服务端去重， wang加
            {
                var docFile = _uploadPath + existsDoc.RelativePath;
                if (!File.Exists(docFile)) //万一找到的旧文件不存在，就复制新传的文件
                {
                    docFile = _uploadPath + doc.RelativePath;
                    File.Move(request.ServerFile, docFile);
                    await this._fileMan.UpdateManyAsync(new { doc.RelativePath, existsDoc.Md5 });
                }
                else //如果相同的文件已存在，则新增一条指向原有文件的记录
                {
                    doc.RelativePath = existsDoc.RelativePath;
                }
                result.Dup = true;
            }

            var dict = CommOp.ToStrObjDict(doc);
            foreach(var kv in request.Metadata)
            {
                dict[kv.Key] = kv.Value;
            }
            doc.Id = await _fileMan.InsertAsync(dict);
            return doc;
        }

        private SysFile BuildNew(ServerUploadRequest request)
        {
            var doc = new SysFile
            {
                Name = request.FileName,
                CreateTime = DateTime.Now,
                UserId = request.UserId
            };

            var ext = Path.GetExtension(request.FileName);

            var level1Dir = doc.CreateTime.ToString("yyyyMM");
            var level2Dir = doc.CreateTime.ToString("ddHH");
            var docPath = $@"{this._uploadPath}/{level1Dir}/{level2Dir}";
            if (!Directory.Exists(docPath)) Directory.CreateDirectory(docPath);
            doc.RelativePath = $@"/{level1Dir}/{level2Dir}/{Guid.NewGuid().ToString("N")}{ext}";
            doc.ContentType = IOHelper.GetContentType(request.FileName);
            return doc;
        }

        /// <summary>生成MD5</summary>
        private string BuildMD5(string serverFile, string uploadMD5)
        {
            var md5 = IOHelper.GetMD5HashFromFile(serverFile).ToUpperInvariant();
            if (!uploadMD5.IsEmpty() && !md5.Equals(uploadMD5, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("上传文件MD5校验失败！");
            }
            return md5;
        }

        /// <summary>获得新分片文件路径</summary>
        private string GetNewChunkFile(ServerUploadRequest request, int chunk)
        {
            var ext = Path.GetExtension(request.FileName);
            return $@"{this.TempPath}/{request.Guid}_PART_{chunk.ToString()}{ext}";
        }

        /// <summary>获得分片合并文件路径</summary>
        private string GetNewMergeFile(ServerUploadRequest request)
        {
            var ext = Path.GetExtension(request.FileName);
            return $@"{this.TempPath}/{request.Guid}{ext}";
        }

        /// <summary>根据FileID获得相关文件流, wang加</summary>
        public async Task<DownloadResult> DownloadAsync(string id)
        {
            var result = new DownloadResult();
            var file = await _fileMan.GetByIdAsync(id);
            if (file == null)
            {
                throw new Exception("指定文件不存在");
            }
            return Download(file);
        }

        /// <summary>
        /// 根据指定SysFile文件实体信息下载文件，即使SysFile不在数据库中
        /// </summary>
        /// <param name="file">SysFile文件实体信息</param>
        /// <returns>文件下载结果</returns>
        public DownloadResult Download(SysFile file)
        {
            var result = new DownloadResult();
            result.FileName = file.Name;
            var relativePath = file.RelativePath;
            if (result.FileName.IsEmpty()) result.FileName = Path.GetFileName(relativePath);
            var filePath = this._uploadPath + relativePath;
            if (File.Exists(filePath))
                result.Content = File.OpenRead(filePath);
            else
                throw new Exception("指定文件不存在");
            result.ContentType = file.ContentType;
            return result;
        }

        /// <summary>获得上传文件夹下全部子文件夹</summary>
        public Task<List<UploadFolder>> GetUploadFoldersAsync()
        {
            var path = this._uploadPath;
            var folders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                .Select(e => new UploadFolder() { FullName = e.Substring(path.Length) })
                .ToList();
            for (int i = 0; i < folders.Count; i++)
            {
                var folder = folders[i];
                folder.Id = i + 1;
                folder.Name = folder.FullName.Substring(folder.FullName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                folder.Level = folder.FullName.Count(e => e == Path.DirectorySeparatorChar);
                folder.FullName = folder.FullName.Replace(Path.DirectorySeparatorChar, '/');
            }
            for (int i = 0; i < folders.Count; i++)
            {
                var folder = folders[i];
                if (folder.Level == 1) continue;
                folder.ParentId = folders.First(e => e.Level == folder.Level - 1 && folder.FullName.StartsWith(e.FullName, StringComparison.OrdinalIgnoreCase)).Id;
            }
            return Task.FromResult(folders);
        }

        /// <summary>获得某个上传文件夹下全部文件列表</summary>
        public Task<List<string>> GetUploadFolderFilesAsync(string folderFullName)
        {
            folderFullName = folderFullName.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);
            var path = $@"{this._uploadPath}/{folderFullName}";
            var files = Directory.GetFiles(path).Select(e => e.Substring(path.Length + 1)).ToList();
            return Task.FromResult(files);
        }

        /// <summary>清除临时文件夹中的过期文件</summary>
        public void ClearTempFiles()
        {
            var files = Directory.GetFiles(this.TempPath);
            var now = DateTime.Now;
            var delay = TimeSpan.FromHours(1);
            foreach (var file in files)
            {
                if ((now - File.GetCreationTime(file)) > delay)
                {
                    //删除超过一小时的
                    File.Delete(file);
                }
            }
        }
    }
}