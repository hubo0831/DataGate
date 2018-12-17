using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate
{
    /// <summary>
    /// 生成数据配置文件的配置项
    /// </summary>
    public class DictSettings
    {
        /// <summary>
        /// 指定要生成的表名正则表达式
        /// 当同时存在Tables和TableReg时取两者并集
        /// </summary>
        public string TableReg { get; set; }

        /// <summary>
        /// 是否生成数据模型json文件
        /// </summary>
        public bool CreateModels { get; internal set; }

        /// <summary>
        /// 指定要生成的表名列表
        /// </summary>
        public string[] Tables { get; set; }

        /// <summary>
        /// 是否生成数据字典excel文件
        /// </summary>
        public string DictFile { get; internal set; }

        /// <summary>
        /// 是否生成Keys.json数据操作文件
        /// </summary>
        public bool CreateKeys { get; internal set; }

        /// <summary>
        /// 指定数据操作文件名
        /// </summary>
        public string KeyFile { get; internal set; }

        /// <summary>
        /// 数据字典文件名
        /// </summary>
        public bool CreateDict { get; internal set; }

        /// <summary>
        /// 数据模型文件名
        /// </summary>
        public string ModelFile { get; internal set; }
    }
}
