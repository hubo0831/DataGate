using DataGate.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 用户信息返回值
    /// </summary>
    public class UserInfoResult : ApiResult
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 用户能访问的菜单或功能列表
        /// </summary>
        public IList<AppMenu> Menus { get; set; }
    }
}
