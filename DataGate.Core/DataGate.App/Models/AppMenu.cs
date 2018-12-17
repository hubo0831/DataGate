using DataGate.Com;

namespace DataGate.App.Models
{
    /// <summary>
    /// 用户菜单功能实体
    /// </summary>
    public class AppMenu : IdName<string>
    {
        /// <summary>
        ///显示方式: 导航菜单=UserMenu 功能页=''
        /// </summary>
        public string ShowType { get; set; }

        /// <summary>
        /// 访问级别: 权限控制=Auth 所有用户=AllUsers 暂时停用=Forbidden
        /// </summary>
        public string AuthType { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 图标样式名称
        /// </summary>
        public string IconCls { get; set; }

        /// <summary>
        /// 导航URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 导航路由定义
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public double Ord { get; set; }
    }
}
