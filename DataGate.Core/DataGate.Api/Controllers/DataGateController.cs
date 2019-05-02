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
using Microsoft.AspNetCore.Mvc.Filters;

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
            _dg.LogAction = (gkey, sql, ps) =>
             {
                 Log.Abstract = gkey.Name;
                 Log.Message = "--" + gkey.Name + Environment.NewLine + sql;
                 Log.Message += Environment.NewLine + String.Join(",", ps.Select(p => p.ParameterName + "=" + p.Value));
                 if (Log.Message.Length > 4000) Log.Message = Log.Message.Remove(4000);
                 Log.ObjectId = gkey.Key;
             };
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            _dg.Session = this.GetSession();
        }

        /// <summary>
        /// 通过指定的key配置查询列表或对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<object>> Query()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = key;
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => (object)kv.Value.FirstOrDefault());
            dict.Remove("_"); //排除jQuery的随机数
            var result = await _dg.QueryAsync(key, dict);
            //throw new ArgumentException("手动引发的异常");
            return result;
        }

        /// <summary>
        /// 将指定查询变为Excel导出，将不考虑分页，所以查出的数据量太大可能出问题 v0.2.0+
        /// </summary>
        /// <returns></returns>
        public async Task<FileResult> Export()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = key;
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => (object)kv.Value.FirstOrDefault());

            string fileName = (string)dict["filename"] ?? "导出.xlsx";
            var result = await _dg.GetExcelStreamAsync(key, dict);
            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// 执行非查询的语句
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<int>> NonQuery()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = key;
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => (object)kv.Value.FirstOrDefault());
            dict.Remove("_"); //排除jQuery的随机数
            var result = await _dg.NonQueryAsync(key, dict);
            return result;
        }

        /// <summary>
        /// 通过指定的key配置执行数据增删改操作 v0.2.0+
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<object>> Submit([FromBody]DataSubmitRequest request)
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = key;
            //string requestString = IOHelper.StreamToStr(Request.Body);
            //var request = JsonConvert.DeserializeObject<DataSaveRequest>(requestString);
            var result = await _dg.SubmitAsync(key, request);
            return result as object;
        }

        /// <summary>
        /// 获取某Key值对应的字段元数据描述数组,
        /// 如果是，分隔的多个key,则返回元数据数组的数组 
        /// </summary>
        /// <returns>元数据描述对象数组</returns>
        [HttpGet]
        public object Metadata()
        {
            var keyv = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = keyv;
            var keys = keyv.Split(',').Select(k => k.Trim());
            var result = keys.Select(_dg.Metadata);

            if (keys.Count() == 1) return result.First();
            return result; //V0.1.7+
        }

        /// <summary>
        /// 通过指定的key配置同时查询元数据和数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<object>> MetaQuery()
        {
            var key = (string)this.ControllerContext.RouteData.Values["key"];
            Log.ObjectId = key;
            var dict = Request.Query.ToDictionary(kv => kv.Key, kv => (object)kv.Value.FirstOrDefault());
            dict.Remove("_"); //排除jQuery的随机数
            var meta = _dg.Metadata(key);
            var data = await _dg.QueryAsync(key, dict);
            //throw new ArgumentException("手动引发的异常");
            return new { meta, data };
        }
    }
}
