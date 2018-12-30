//此处配置随环境而改变
// 此对象将在与appConfig合并时消除
// 所有属性定义转到appConfig下
window.globalConfig = {
  //   apiUrl: 'http://192.168.1.249:31300',
   apiUrl: 'http://localhost:60743',
    appName: 'XXX管理系统',
    elSize: 2, //控件和字体默认大小 1代表小 2代表中 3代表大
    //上传控件的初始配置值
    uploadOptions: {
        fileNumLimit: 300, //最多一次上传文件数300
        fileSizeLimit: 1000 * 1024 * 1024,    // 最多一次上传量 1000 M
        fileSingleSizeLimit: 100 * 1024 * 1024    //单个文件最大值 100 M
    },
    //追加或改写原有的元数据定义
    defaultMetadata: [
        //此处重新定义油藏(res)元数据，除name外，只需要定义与基础元数据定义不同的属性，相同的属性可省略
        //如果元数据定义中已经是下拉列表则不需要此对象
        //{
        //    name: 'res', //必须
        //    uitype: 'DropdownList', //重新定义输入方式为下拉列表
        //    usage: 'keyName'  //元数据应用到的表
        //}
    ],
    //定义元数据默认值的求值算法
    defaultValueFunc: {
        // DEMO: 定义createdate的默认值,命名规则是： 元数据name_value
        // obj=当前成果对象，vue：调用方的Vue对象，
        //createdate_value: function(obj, vue){
        //    return new Date()
        //}
        //startyear_value: function (obj, vue) {
        //    return "2016";//暂时先占位
        //},
        //endyear_value: function (obj, vue) {
        //    return "2020"; 
        //}

    },
    //定义下拉列表的列表值算法，可以直接返回一个字符串数组，或一个$.ajax异步对象
    itemsFunc: {
        //定义下拉列表, 命名规则是 元数据name_items
        // obj=当前成果对象，meta=元数据定义, vue：调用方的Vue对象
        //xxxx_items: function (obj, meta, vue) {
        //    return vue.API.POST(....);
        //}
    }

}
