using DataGate.App;
using DataGate.App.DataService;
using DataGate.Com;
using DataGate.Com.DB;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataGate
{
    class Program
    {
        static void Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            Consts.Config = builder.Build();

            MetaService.RegisterDBHelper("Default", () => new DBHelper
            {
                //var cfg = Program.Config.GetSection("ConnectionStrings");
                DBComm = new DBCommOracle(),
                //  DbNameConverter = new UpperNameConverter()
            });
        }
        static DictSettings dictSettings;

        static bool Error(string err)
        {
            Console.WriteLine("错误信息:" + err);
            return false;
        }

        static bool Usage()
        {
            Console.WriteLine(@"用法：
datagate [<option> [arg]] ...
option选项：
    -m  生成数据模型文件 后跟 文件名(*Models.json),
    -k  生成数据查询文件 后跟 文件名(*Keys.json)，
    -d  生成数据字典文件 后跟 文件名(*.xlsx)，

    -t  指定要生成的表名,用逗号分隔
    -tr 指定要生成的表名正则表达式
    
    -h | -? | -help 显示此信息
例子:
完全根据appsettings.json中的选项执行：
dotnet datagate.dll

生成数据模型文件到当前目录的appmodels.json,其他选项根据appsettings.json
dotnet datagate.dll -m appmodels.json
   
    也可以在appsettings.json中配置选项，详情请见 https://github.com/bwangel
");
            return false;
        }

        static bool GetSettingsFromArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg[0] != '-')
                {
                    Error($"参数'{arg}'错误,必须以'-'开头");
                    return false;
                }
                switch (arg.Substring(1))
                {
                    case "t":
                        arg = args[i+1];
                        if (arg[0] == '-')
                        {
                            dictSettings.Tables = null;
                            continue;
                        }
                        i++;
                        dictSettings.Tables = arg.Split(",");
                        break;
                    case "tr":
                        arg = args[i+1];
                        if (arg[0] == '-')
                        {
                            dictSettings.TableReg = null;
                            continue;
                        }
                        i++;

                        dictSettings.TableReg = arg;
                        break;
                    case "m":
                        dictSettings.CreateModels = true;
                        if (i >= args.Length - 1) break;
                        arg = args[i + 1];
                        if (arg[0] == '-') continue;
                        i++;
                        dictSettings.ModelFile = arg;
                        break;
                    case "k":
                        dictSettings.CreateKeys = true;
                        if (i >= args.Length - 1) break;
                        arg = args[i + 1];
                        if (arg[0] == '-') continue;
                        i++;
                        dictSettings.KeyFile = arg;
                        break;
                    case "d":
                        dictSettings.CreateDict = true;
                        if (i >= args.Length - 1) break;
                        arg = args[i + 1];
                        if (arg[0] == '-') return Error("必须指定字典Excel的文件名");
                        i++;
                        if (!arg.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            arg += ".xlsx";
                        }
                        dictSettings.DictFile = arg;
                        break;
                    case "h":
                    case "help":
                    case "?":
                        return Usage();
                    default:
                        return Error("不能识别的选项:{arg}");
                }
            }
            return true;
        }

        /// <summary>
        /// 控制台控制自动生成数据模型Json文件
        /// </summary>
        /// <param name="args">
        /// </param>
        static void Main(string[] args)
        {
            Startup();
            dictSettings = new DictSettings();
            var cfgSection = Consts.Config.GetSection("DictSettings");
            if (cfgSection.Exists())
            {
                dictSettings.CreateDict = cfgSection.GetValue<bool>("CreateDict");
                dictSettings.CreateKeys = cfgSection.GetValue<bool>("CreateKeys");
                dictSettings.CreateModels = cfgSection.GetValue<bool>("CreateModels");
                dictSettings.DictFile = cfgSection.GetValue<string>("DictFile");
                dictSettings.ModelFile = cfgSection.GetValue<string>("ModelFile");
                dictSettings.KeyFile = cfgSection.GetValue<string>("KeyFile");
                dictSettings.TableReg = cfgSection.GetValue<string>("TableReg");
                dictSettings.Tables = cfgSection.GetValue<string>("Tables").ToStr().Split(",")
                .Select(t => t.ToStr())
               .Where(t => !t.IsEmpty()).ToArray();
            }
            if (!GetSettingsFromArgs(args))
            {
                return;
            }
            if (!cfgSection.Exists() && args.Length == 0)
            {
                Error("没有在配置文件appsettings.json中发现指定的生成信息，也没有在命令行参数中发现。");
                Usage();
            }

            DictService dictService = new DictService(new DictForOracle(), new UpperNameConverter(), dictSettings);
            dictService.Run();
            Console.Write("执行完毕，按任意键退出");
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
