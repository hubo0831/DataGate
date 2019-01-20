using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DataGate.Api.Controllers;
using DataGate.App;
using DataGate.App.DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataGate.Api.Filters
{
    /// <summary>
    /// 暂时用此Filter来拦截判断权限
    /// </summary>
    public class TokenValidateFilter : IActionFilter
    {
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.ActionDescriptor is ControllerActionDescriptor action))
            {
                return;
            }

            if (!(context.Controller is BaseController controller))
            {
                return;
            }

            if (ExcludedActions.Contains($"{context.Controller.GetType()}.{action.ActionName}"))
            {
                return;
            }

            controller.Log = controller.Log ?? DefaultFilter.GetLogInfo(context);

            if (Consts.IsTesting)
            {
                return;
            }

            var token = context.HttpContext.Request.Headers["token"].FirstOrDefault();
            if (token == null)
            {
                token = context.HttpContext.Request.Query["token"];
            }
            if (String.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(MSG.NotLogined);
                controller.Log.Message = MSG.NotLogined.Message;
            }
            else
            {
                var sessionProvider = Consts.ServiceProvider.GetService<SessionProvider>();
                var session = sessionProvider.Get(token);
                if (session == null)
                {
                    context.Result = new JsonResult(MSG.SessionExpired);
                    controller.Log.Message = MSG.SessionExpired.Message;
                }
            }
        }

        static string[] ExcludedActions =
        {
           $"{typeof(CheckController)}.{nameof(CheckController.Login)}",
           $"{typeof(HomeController)}.{nameof(HomeController.Index)}"
        };
    }
}
