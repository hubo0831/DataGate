using DataGate.Api.Controllers;
using DataGate.App;
using DataGate.Com;
using DataGate.Com.Logs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DataGate.Api.Filters
{
    /// <summary>
    /// 暂时用此Filter来拦截记录常规访问日志
    /// </summary>
    public class DefaultFilter : IActionFilter, IResultFilter, IExceptionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Controller is BaseController controller)
            {
                controller.OnActionExecuted(context);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is BaseController controller)
            {
                controller.Log = GetLogInfo(context);
                controller.Log.Account = controller.GetSession()?.Account;
                controller.OnActionExecuting(context);
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Controller is BaseController controller)
            {
                controller.OnResultExecuting(context);
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.Controller is BaseController controller && controller.Log != null)
            {
                controller.OnResultExecuted(context);
                LogHelper.Write(controller.Log);
            }
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            if (ex == null || context.ExceptionHandled) return;

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.HttpContext.Response.ContentType = "application/json;charset=utf-8"; //此处要加上utf-8编码

            //如果不加此句，服务器返回的数据到浏览器会拒绝
            context.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
            var logInfo = GetLogInfo(context);
            //   logInfo.Costs = (DateTime.Now - logInfo.OpTime).TotalMilliseconds;
            LogHelper.Write(logInfo, ex);
            var errObj = new
            {
                message = ex.Message,
                stackTrace = ex.StackTrace,
                exceptionType = ex.GetType().Name
            };// $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";

            context.Result = new JsonResult(errObj);
        }

        internal static LogInfo GetLogInfo(FilterContext context)
        {
            LogInfo logInfo = Consts.Get<LogInfo>();
            var action = context.ActionDescriptor as ControllerActionDescriptor;
            var httpContext = context.HttpContext;
            logInfo.UserAgent = httpContext.Request.Headers["User-Agent"];
            logInfo.Action = action.ActionName;
            logInfo.ClientIP = httpContext.Connection.RemoteIpAddress?.ToString();
            logInfo.Module = action.ControllerName;
            logInfo.OpTime = DateTime.Now;
            logInfo.LogLevel = LogType.Info;
            logInfo.Request = httpContext.Request.Method + " " + httpContext.Request.QueryString.ToString();
         //   context.ActionDescriptor.Properties[LogSaveKey] = logInfo;
            return logInfo;
        }

        /// <summary>   
        /// 根据 User Agent 获取操作系统名称
        /// </summary>   
        // void GetBrowserName(string userAgent, out string browserName, out string ver)
        //{
        //    string fullBrowserName = string.Empty;
        //    browserName = string.Empty;
        //    ver = string.Empty;
        //    // IE
        //    string regexStr = @"msie ([\d.]+)";
        //    Regex r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    Match m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "IE";
        //        ver = m.Groups["ver"].Value;
        //        return;
        //    }
        //    // Firefox
        //    regexStr = @"firefox\/([\d.]+)";
        //    r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "Firefox";
        //        ver = m.Groups[1].Value;
        //        return;
        //    }
        //    // Chrome
        //    regexStr = @"chrome\/([\d.]+)";
        //    r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "Chrome";
        //        ver = m.Groups[1].Value;
        //        return;
        //    }
        //    // Opera
        //    regexStr = @"opera.([\d.]+)";
        //    r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "Opera";
        //        ver = m.Groups[1].Value;
        //        return;
        //    }
        //    // Safari
        //    regexStr = @"version\/([\d.]+).*safari";
        //    r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "Safari";
        //        ver = m.Groups[1].Value;
        //        return;
        //    }
        //    // Edge
        //    regexStr = @"Edge/([\d.]+)";
        //    r = new Regex(regexStr, RegexOptions.IgnoreCase);
        //    m = r.Match(userAgent);
        //    if (m.Success)
        //    {
        //        browserName = "Edge";
        //        ver = m.Groups[1].Value;
        //        return;
        //    }
        //}

        /// <summary>   
        /// 根据 User Agent 获取操作系统名称
        /// </summary>   
        //private static string GetOSNameByUserAgent(string userAgent)
        //{
        //    if (String.IsNullOrEmpty(userAgent)) return userAgent;
        //    userAgent = userAgent.ToLower();
        //    if (userAgent.Contains("nt 6.1"))
        //    {
        //        return "Windows 7";
        //    }
        //    if (userAgent.Contains("nt 5.1"))
        //    {
        //        return "Windows XP";
        //    }
        //    if (userAgent.Contains("nt 5.2"))
        //    {
        //        return "Windows Server 2003";
        //    }
        //    if (userAgent.Contains("nt 6.0"))
        //    {
        //        return "Vista/Server 2008";
        //    }
        //    if (userAgent.Contains("nt 6.2"))
        //    {
        //        return "Windows 8";
        //    }
        //    if (userAgent.Contains("nt 6."))
        //    {
        //        return "Windows 8.1";
        //    }
        //    if (userAgent.Contains("nt 5"))
        //    {
        //        return "Windows 2000";
        //    }
        //    if (userAgent.Contains("nt 10."))
        //    {
        //        return "Windows 10";
        //    }
        //    if (userAgent.Contains("ipad"))
        //    {
        //        return "iPad";
        //    }
        //    if (userAgent.Contains("iphone"))
        //    {
        //        return "iPhone";
        //    }
        //    if (userAgent.Contains("android"))
        //    {
        //        return "android";
        //    }
        //    if (userAgent.Contains("mac"))
        //    {
        //        return "Mac";
        //    }
        //    if (userAgent.Contains("unix"))
        //    {
        //        return "UNIX";
        //    }
        //    if (userAgent.Contains("linux"))
        //    {
        //        return "Linux";
        //    }
        //    if (userAgent.Contains("nt 4"))
        //    {
        //        return "Windows NT4";
        //    }
        //    if (userAgent.Contains("me"))
        //    {
        //        return "Windows Me";
        //    }
        //    if (userAgent.Contains("98"))
        //    {
        //        return "Windows 98";
        //    }
        //    if (userAgent.Contains("95"))
        //    {
        //        return "Windows 95";
        //    }
        //    if (userAgent.Contains("sunos"))
        //    {
        //        return "SunOS";
        //    }
        //    return "Unknown";
        //}


    }
}
