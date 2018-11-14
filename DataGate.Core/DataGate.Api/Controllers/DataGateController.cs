using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DataGate.App.Models;
using DataGate.Com.DB;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using DataGate.App.DataService;
using Microsoft.AspNetCore.Mvc;
using DataGate.Com;
using Newtonsoft.Json.Linq;

namespace DataGate.Api.Controllers
{
    /// <summary>
    /// 通用数据接口
    /// </summary>
    public class DataGateController : BaseController
    {
        DataGateService _dg;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dg"></param>
        public DataGateController(DataGateService dg)
        {
            _dg = dg;
        }

        /// <summary>
        /// 通过指定的key配置查询列表或对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<object>> Query()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => (object)kv.Value.FirstOrDefault());
            var result = await _dg.QueryAsync(key, dict);
            //throw new ArgumentException("手动引发的异常");
            return result;
        }

        /// <summary>
        /// 通过指定的key配置执行数据增删改操作
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<object>> Submit([FromBody]DataSubmitRequest request)
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            //string requestString = IOHelper.StreamToStr(Request.Body);
            //var request = JsonConvert.DeserializeObject<DataSaveRequest>(requestString);
            var result = await _dg.SubmitAsync(key, request);
            //throw new ArgumentException("手动引发的异常");
            return result as object;
        }

        /// <summary>
        /// 获取某Key值对应的字段元数据描述
        /// </summary>
        /// <returns>元数据描述对象数组</returns>
        [HttpGet]
        public  IEnumerable<FieldMeta> Metadata()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            var result =  _dg.Metadata(key);
            //throw new ArgumentException("手动引发的异常");
            return result;
        }
    }
}
