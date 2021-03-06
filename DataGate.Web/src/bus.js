import Vue from "vue";

Vue.use((Vue, options) => {
  //注册需要外部确认的事件,item即可以是一个具体对象也可以是一个带passed标记的args
  // 同步：外部将传入的args.passed = true表示放行，允许事件发起方执行某些操作(callback) =false表示不放行
  // 异步：外界给args.promise赋值为一个Promise对象，事件发起方异步通过then调用callback
  // 当callback为空时，事件发起方只能顺序往下根据args.passed自行判断，异步将无效
  Vue.prototype.$emitPass = function (evt, item, callback, failCallback) {
    //当没有传item过来或item不是args类型时，创建一个args
    var args = (!item || (typeof (item.passed) == 'undefined')) ? {
      item,
      passed: true,
      promise: null
    } : item;
    this.$emit(evt, args);
    if (args.promise) {
      if (typeof args.promise == "function") {
        args.promise = args.promise();
      }
      args.promise.then(() => callback && callback(item))
        .catch(() => failCallback && failCallback(item));
    } else if (args.passed) {
      callback && callback(item);
    } else {
      failCallback && failCallback(item);
    }
    return args;
  }
});

//事件总线
//事件列表：
// server-exception （服务端报错）
// update-title (更新导航条)
// session-timeout （登录过期）
// logout 退出登录
// login 登录

// register 注册新组件  两个参数(name, component) 相当于Vue.component(name, component)
//直接在项目中Vue.component好象不行 
//注册新插件 一个参数 plugins   相当于 Vue.use(plugins)  在使用源代码引用本框架时，直接在项目上中用Vue.use好象不行，原因不明
export default new Vue()
