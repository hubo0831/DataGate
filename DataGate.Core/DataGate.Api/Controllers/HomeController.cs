using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DataGate.Api.Controllers
{
    /// <summary>
    /// 转到主页的控制器
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// 转到wwwroot下的index.html
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return RedirectPermanent("~/index.html");
        }
    }
}