using DataGate.App.DataService;
using DataGate.Com;
using DataGate.Com.DB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App.Files
{
    public class SysFileMan
    {
        private DataGateService _dg;

        public SysFileMan(DataGateService dg)
        {
            _dg = dg;
        }

        /// <summary>
        /// 复制一个相同的文件信息记录，指向同一个物理文件，只是逻辑文件名不同
        /// </summary>
        /// <returns></returns>
        public async Task<string> Duplicate(SysFile file)
        {
            file.Id = null;
            return await _dg.InsertOneAsync("SaveFile", file);
        }

        /// <summary>
        /// 根据MD5查找第一个匹配的文件
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public async Task<SysFile> GetByMd5Async(string md5)
        {
            var doc = await _dg.QueryAsync("GetByMd5", new { md5 });
            return doc == null ? null : JObject.FromObject(doc).ToObject<SysFile>();
        }

        public async Task<SysFile> GetByIdAsync(string id)
        {
            var doc = await _dg.QueryAsync("GetById", new { id });
            return doc == null ? null : JObject.FromObject(doc).ToObject<SysFile>();
        }

        public async Task UpdateManyAsync(object p)
        {
            await _dg.UpdateAsync("UpdatePath", p);
        }

        public async Task<string> InsertAsync(object file)
        {
            var id = await _dg.InsertOneAsync("SaveFile", file) ;
            return id;
        }
    }
}
