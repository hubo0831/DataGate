using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com
{
    /// <summary>
    /// 标识框架版本的静态类
    /// </summary>
    public sealed class Frame
    {
        public const string Version = "0.1.4";

        public const string CompanyName = "bwangel@163.com";

#if DEBUG
        public const string Build = "Debug";
#else
        public const string Build = "Release";
#endif
        public const string Year = "2018";

        public const string Copyright = "Copyright Jurassic Software";

        public const string Trademark = "DataGate";
    }
}
