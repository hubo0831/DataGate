using Autofac;
using DataGate.App;
using DataGate.App.DataService;
using DataGate.App.Models;
using DataGate.Com.DB;
using Microsoft.Extensions.Configuration;
using System;

namespace DataGate.Api
{
    public class DefaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //注册app层
            builder.RegisterAssemblyTypes(typeof(AppUser).Assembly);
            builder.RegisterAssemblyTypes(typeof(AppUser).Assembly)
                   .Where(t => typeof(ISingleton).IsAssignableFrom(t))
                   .SingleInstance();

            //builder.Register(c =>
            //{
            //    var db = new DBHelper
            //    {
            //        //var cfg = Program.Config.GetSection("ConnectionStrings");
            //        ConnStr = Consts.Config.GetConnectionString("Default"),
            //        DBComm = new DBCommOracle(),
            //        DbNameConverter = new UpperNameConverter()
            //    };
            //    return db;
            //})
            //.As<DBHelper>();
            //// .SingleInstance(); 由于事务的存在，DBHelper不能单例

            builder.RegisterGeneric(typeof(DBCrud<>));
            builder.RegisterType<MetaService>().As<IMetaService>().SingleInstance();
        }
    }
}