using Autofac;
using DataGate.App;
using DataGate.App.DataService;
using DataGate.App.Logs;
using DataGate.App.Models;
using DataGate.Com.DB;
using DataGate.Com.Logs;
using Microsoft.Extensions.Configuration;
using System;

namespace DataGate.Api
{
    /// <summary>
    /// DataGate.App的Autofac注入程序, DataGate.App设定为不依赖Autofac
    /// 所以需要在主程序中注入其中类型
    /// </summary>
    public class DefaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //注册app层
            builder.RegisterAssemblyTypes(typeof(AppUser).Assembly);
            builder.RegisterAssemblyTypes(typeof(AppUser).Assembly)
                   .Where(t => typeof(ISingleton).IsAssignableFrom(t))
                   .SingleInstance();

            builder.RegisterType<LogInfo>().AsSelf();
            builder.RegisterType<NLogManager>().As<ILogManager>().SingleInstance();

            builder.Register(c =>
            {
                return DBFactory.CreateDBHelper("Default");
            })
            .As<DBHelper>();
            //// .SingleInstance(); 由于事务的存在，DBHelper不能单例

            builder.RegisterGeneric(typeof(DBCrud<>));
            builder.RegisterType<MetaService>().As<IMetaService>().SingleInstance();
        }
    }
}