using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App
{
    /// <summary>
    /// 常量或常量对象
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// 是否处于单元测试运行模式下
        /// </summary>
        public static bool IsTesting { get; set; }

        /// <summary>
        /// 系统默认的配置管理器
        /// </summary>
        public static IConfiguration Config { get; set; }

        /// <summary>
        /// 默认的主键名
        /// </summary>
        public const string DefaultKeyName = "id";

        /// <summary>
        /// 默认的排序列名
        /// </summary>
        public const string DefaultSortName = "ord";

        /// <summary>
        /// 表格列的元数据描述中，作为排序列的DataType名称
        /// </summary>
        public const string DataTypeSortOrder = "SortOrder";

        /// <summary>
        /// 默认分页大小
        /// </summary>
        public static int DefaultPageSize { get;  set; } = 20;

        /// <summary>
        /// 系统默认的服务提供程序，在Startup初始化时自动赋值
        /// </summary>
        public static IServiceProvider ServiceProvider{ get; set; }

        /// <summary>
        /// 提交查询时代表查询表达式的的Url地址栏参数名
        /// </summary>
        public const string FilterKey = "_filter";

        /// <summary>
        /// 提交查询时代表排序定义的Url地址栏参数名
        /// </summary>
        public const  string SortKey = "_sort";

        /// <summary>
        /// 表格列的元数据描述中，作为操作列或按钮列的列的UIType
        /// </summary>

        public const string OperatorUIType = "Operator";

        /// <summary>
        /// 用系统默认的SerivceProvider获取指定接口或类型的服务
        /// </summary>
        /// <typeparam name="T">定接口或类型</typeparam>
        /// <returns>服务对象</returns>
        public static T Get<T>()
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }
    }
}
