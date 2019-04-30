using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DataGate.Api.Filters;
using DataGate.App;
using DataGate.App.DataService;
using DataGate.Com.DB;
using DataGate.Com.Logs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Extensions.Logging;

namespace DataGate.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Consts.Config = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        //}
        public IContainer ApplicationContainer { get; private set; }

        //先于Configure执行
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors();//跨域
            services.AddMvc(options =>
            {
                options.Filters.Add(new TokenValidateFilter());
                options.Filters.Add(typeof(DefaultFilter));
                //options.Filters.Add(new DefaultActionFilter());
                //options.Filters.Add(new DefaultResultFilter());
                //options.Filters.Add(new DefaultExceptionFilter());
            })
             .AddJsonOptions(options =>
            {
                //忽略循环引用
                //options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不更改元数据的key的大小写
                //options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //不序列化为null的属性
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                //不序列化等于默认值的属性
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var builder = new ContainerBuilder();//实例化 AutoFac  容器                  
            builder.Populate(services);
            //注册其他程序集中的模块
            builder.RegisterAssemblyModules(AppDomain.CurrentDomain.GetAssemblies());

            ApplicationContainer = builder.Build();

            //AutoFac声明以下方法已过时，因此不可能在程序运行中动态改变已注册的注入
            //builder.Update(ApplicationContainer);

            MetaService.RegisterDataGate("SaveUser", new UsersGate());
            Consts.ServiceProvider = new AutofacServiceProvider(ApplicationContainer);//第三方IOC接管 core内置DI容器
            return Consts.ServiceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection().UseCors(builder =>
              builder.AllowAnyOrigin()
              .AllowAnyMethod().AllowAnyHeader());//.WithOrigins("http://example.com"));

            //GlobalDiagnosticsContext.Set("configDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            //GlobalDiagnosticsContext.Set("connectionString", Configuration.GetConnectionString("Default"));

            loggerFactory.AddNLog();
            LogHelper.Init(Consts.Get<ILogManager>(), "*");
           
            //默认文件设置为index.html
            var defaultFilesOptions = new DefaultFilesOptions();
            app.UseDefaultFiles(defaultFilesOptions);

            app.UseMvc(routes =>
            {
                //批量保存增删改结果
                routes.MapRoute(name: "datagate-save",
                   template: "api/dg/s/{key}",
                   defaults: new { controller = "DataGate", action = "Submit" });

                //获取某key值对应的数据表的metatata
                routes.MapRoute(name: "datagate-metadata",
                   template: "api/dg/m/{key}",
                   defaults: new { controller = "DataGate", action = "Metadata" });

                //非查询执行数据库操作
                routes.MapRoute(name: "datagate-nonquery",
                   template: "api/dg/n/{key}",
                   defaults: new { controller = "DataGate", action = "NonQuery" });

                //导出Excel的路由
                routes.MapRoute(name: "datagate-export",
                    template: "api/dg/x/{key}",
                    defaults: new { controller = "DataGate", action = "Export" });

                //文件上传的路由
                routes.MapRoute(name: "datagate-upload",
                    template: "api/dg/u",
                    defaults: new { controller = "Files", action = "Upload" });

                //文件下载的路由，如果通过a链接下载，可能要带上?token=用户的Token作为身份标识
                routes.MapRoute(name: "datagate-download",
                 template: "api/dg/d/{id}/{filename?}",
                 defaults: new { controller = "Files", action = "DownById" });

                //通用的数据查询路由
                routes.MapRoute(name: "datagate-query",
                    template: "api/dg/{key}",
                    defaults: new { controller = "DataGate", action = "Query" });

                AddRoutes(routes);
                //  routes.MapRoute(name: "api-default",
                //template: "api/[controller]/[action]");

                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            }).UseStaticFiles(); //访问wwwroot下的静态文件

            app.UseCookiePolicy();
        }

        /// <summary>
        /// 添加应用程序自己的一些路由
        /// </summary>
        /// <param name="routes"></param>
        protected virtual void AddRoutes(IRouteBuilder routes)
        {

        }
    }
}
