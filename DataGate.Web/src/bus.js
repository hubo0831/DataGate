import Vue from "vue";

Vue.use((Vue, options) => {
  //注册需要外部确认的事件,item即可以是一个具体对象也可以是一个带passed标记的args
  // 同步：外部将传入的args.passed = true表示放行，允许事件发起方执行某些操作(callback) =false表示不放行
  // 异步：外界给args.promise赋值为一个Promise对象，事件发起方异步通过then调用callback
  // 当callback为空时，事件发起方只能顺序往下根据args.passed自行判断，异步将无效
  Vue.prototype.$emitPass = function (evt, item, callback) {

    //当没有传item过来或item不是args类型时，创建一个args
    var args = (!item || (typeof (item.passed) == 'undefined')) ? {
      item,
      passed: true,
      promise: null
    } : item;
    this.$emit(evt, args);
    if (args.promise) {
      args.promise().then(() => callback && callback(item));
    } else if (args.passed) {
      callback && callback(item);
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
// app-start App应用程序开始运行
// login 登录
// register 注册新组件
export default new Vue()
