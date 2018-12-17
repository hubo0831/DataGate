import 'babel-polyfill' //解决IE有些方法不支持，如startsWith,endsWith
// import "html5shiv"
global.jQuery = require('jquery');
global.$ = global.jQuery;
$.support.cors = true; //ie9跨域
//import "jquery-slimscroll"

require("./assets/scripts/jquery.slimscroll")
//在此处存放不随环境而改变的配置值
var appConfig = {
  elSize: 2, //控件和字体默认大小 1代表小 2代表中 3代表大
  debug: true, //决定是否弹出服务端的详细错误信息
  logo:require("./assets/images/logo.png"),
  copyright:"&copy;Copyright Jurassic Software 2018 <small>datagate.web v0.1.9</small>",
  systemName:"DataGate Management System",
  titleHtml:"DataGate Management System",
  defaultValueFunc:{}, //默认值的定义
  appSecret: 'somesecrettodefinebyserver' //这个后面应该放服务端
};

//合并两个配置文件，以static/globalConfig中的配置优先
$.extend(appConfig, window.globalConfig);
delete window.globalConfig;

export default appConfig;
