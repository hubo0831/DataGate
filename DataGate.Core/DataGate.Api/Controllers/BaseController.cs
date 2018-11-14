using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataGate.Api.Controllers
{
    /// <summary>
    /// 扩展没有MVC视图支持的控制器基类
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// 登录用户的令牌
        /// </summary>
        public string Token
        {
            get
            {
                return this.HttpContext.Request.Headers["token"].FirstOrDefault();
            }
        }
    }
}