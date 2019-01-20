using DataGate.Com;
using DataGate.Com.DB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// DBHelper类的工厂，自动根据配置文件的连接串构造DBHelper
    /// </summary>
    public class DBFactory
    {
        static Dictionary<string, Func<DBHelper>> _dbDict = new Dictionary<string, Func<DBHelper>>();

        /// <summary>
        /// 在启动时，自动注册所有的DBHelper
        /// </summary>
        static DBFactory()
        {
            var conns = Consts.Config.GetSection("ConnectionStrings").GetChildren();
            foreach (var kv in conns)
            {
                _dbDict[kv.Key.ToLower()] = CreateDBHelperFunc(kv.Value);
            }
        }

        /// <summary>
        /// 注册DBHelper的生成器
        /// </summary>
        /// <param name="connName">连接字符串或名称</param>
        /// <param name="dbFunc">生成DBHelper的方法</param>
        public static void RegisterDBHelper(string connName, Func<DBHelper> dbFunc)
        {
            connName = connName.ToLower();
            _dbDict[connName] = dbFunc;
        }

        public static DBHelper CreateDBHelper(string connStrOrName)
        {
           return  _dbDict[connStrOrName.ToLower()]();
        }

        /// <summary>
        /// 根据指定字符串连接名称或连拉字符串本身获取新的DBHelper
        /// </summary>
        /// <param name="connName"></param>
        /// <returns>DBHelper</returns>
        internal static Func<DBHelper> CreateDBHelperFunc(string connStrOrName)
        {
            if (connStrOrName.IsEmpty()) connStrOrName = "Default";
            return () =>
            {
                DBHelper helper = new DBHelper();

                if (connStrOrName.Length <= 30 && !connStrOrName.Contains('='))
                {
                    helper.ConnStr = Consts.Config.GetConnectionString(connStrOrName);
                }
                else
                {
                    helper.ConnStr = connStrOrName;
                }
                helper.DbNameConverter = new UpperNameConverter();
                helper.DBComm = CreateDBComm(helper.ConnStr);
                return helper;
            };
        }

        private static IDBComm CreateDBComm(string connStr)
        {
            if (connStr.ToUpper().Contains("DESCRIPTION"))
            {
                return new DBCommOracle();
            }
            return new DBCommSql();
        }
    }
}
