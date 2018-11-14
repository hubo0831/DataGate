using DataGate.Com;
using System.Reflection;
// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。

//关于.netcore2.0 以下标签报 ‘XXX特性重复’的问题，参考：
//http://www.qingpingshan.com/m/view.php?aid=331255 
[assembly: AssemblyConfiguration(Frame.Build)]
[assembly: AssemblyDescription(AssemblyRef.Description)]
[assembly: AssemblyTitle(AssemblyRef.Name)]
[assembly: AssemblyProduct(AssemblyRef.Product + " - " + Frame.Build + "版")]

[assembly: AssemblyCompany(Frame.CompanyName)]
[assembly: AssemblyCopyright(Frame.Copyright)]
[assembly: AssemblyTrademark(Frame.Trademark)]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion(Frame.Version)]
[assembly: AssemblyFileVersion(Frame.Version)]

class AssemblyRef
{
    internal const string Name = "DataGate测试库";
    internal const string Product = "DataGate测试库";
    internal const string Description = @"使用此包中的BaseTest和AppDataTest基础类来创建自己的测试类.";
}