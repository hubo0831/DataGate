using DataGate.App.DataService;
using DataGate.Com.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App.Files
{
    public class SysFileMan : DBCrud<SysFile>
    {
        public SysFileMan() : base(DBFactory.CreateDBHelper("Default")) { }

        /// <summary>
        /// 复制一个相同的文件信息记录，指向同一个物理文件，只是逻辑文件名不同
        /// </summary>
        /// <returns></returns>
        public async Task<int> Duplicate(SysFile file)
        {
            file.Id = Guid.NewGuid().ToString("N");
            return await Helper.InsertModelAsync<SysFile>(file);
        }

        /// <summary>
        /// 根据MD5查找第一个匹配的文件
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        public async Task<SysFile> GetByMd5Async(string md5)
        {
            var doc = await GetModelByWhereAsync("Md5=@Md5", Helper.CreateParameter("Md5", md5));
            return doc;
        }
    }
}
