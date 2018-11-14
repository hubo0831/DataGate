using DataGate.App.DataService;
using DataGate.App.Models;
using DataGate.Com.DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataGate.App
{
    public class MenuMan : DBCrud<AppMenu>, ISingleton
    {
        public MenuMan(DBHelper helper) : base(helper)
        {

        }

        /// <summary>
        /// 获取所有菜单和功能，用于功能管理页
        /// </summary>
        /// <returns></returns>
        public async Task<IList<AppMenu>> GetAllAsync()
        {
            return (await GetListAsync())
                .OrderBy(m => m.Ord).ToList();
        }
    }
}
