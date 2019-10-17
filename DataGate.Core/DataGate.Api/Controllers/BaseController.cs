using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataGate.App;
using DataGate.Com;
using DataGate.Com.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nito.AsyncEx;

namespace DataGate.Api.Controllers
{
    /// <summary>
    /// 扩展没有MVC视图支持的控制器基类
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        const string LogSaveKey = "ActionLog";
        Stopwatch sw = new Stopwatch();
        protected BaseController()
        {
        }

        /// <summary>
        /// 登录用户的令牌,不要在构造函数使用, 参数需传小写的token在header中或url参数中
        /// </summary>
        protected string Token
        {
            get
            {
                return this.HttpContext.Request.Headers["token"].FirstOrDefault()
                     ?? this.HttpContext.Request.Query["token"];
            }
        }

        /// <summary>
        /// 获取用户的会话，不要在构造函数使用
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public UserSession GetSession()
        {
            return AsyncContext.Run(async () => await GetSessionAsync());
        }

        /// <summary>
        /// 获取用户的会话，不要在构造函数使用
        /// </summary>
        /// <returns></returns>
        protected async Task<UserSession> GetSessionAsync()
        {
            if (Token != null)
                return await Consts.Get<SessionProvider>().Get(Token);
            return null;
        }

        /// <summary>
        /// 当前控制器的日志数据实体对象,将在筛选器中自动生成，无需手动生成
        /// </summary>
        public LogInfo Log { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            sw.Start();
        }

        public virtual void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
        }

        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
            sw.Stop();
            Log.Costs = sw.ElapsedMilliseconds;
        }

    }
}