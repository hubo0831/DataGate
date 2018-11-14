using Newtonsoft.Json;

namespace DataGate.App
{
    /// <summary>
    /// 公共的API数据返回类
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// 错误信息代码，无错误应该返回0
        /// 加$是为了避免普通数据的属性同名
        /// </summary>
        [JsonProperty("$code")]
        public int Code { get; set; }

        /// <summary>
        /// 错误信息描述
        /// </summary>
        [JsonProperty("$message")]
        public string Message { get; set; }
    }
}