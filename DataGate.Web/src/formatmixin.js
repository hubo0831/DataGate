import util from "./common/util"
import Vue from "vue";
import appConfig from "./appConfig"
import userState from "./userState"
Vue.mixin({
  data() {
    return {
      appConfig, //系统全局配置项
      userState, //用户信息: { currentUser :{ account, name, email, tel, menus:[ ... ]...}
    }
  },
  //Vue.mixin全局混入，用于常用格式化
  filters: {
    //年月日YYYY-MM-dd
    formatDate(date) {
      return util.formatDate(date)
    },
    //年月日时分秒
    formatDateTime(date) {
      return util.formatDate(date, "yyyy-MM-dd hh:mm:ss")
    },
    //年月日时分
    formatDateTime2(date) {
      return util.formatDate(date, "yyyy-MM-dd hh:mm")
    },
    //xx万元
    formatWan(num) {
      return util.formatWan(num);
    },
    // xx亿元 或xx万元，取决于数字大小
    formatWanYi(row, col, num, idx) {
      return util.formatWanYi(num);
    },
    // 百分数
    formatPercent(num) {
      if (isNaN(num)) return '';
      var v = parseInt(num * 1000000);
      return v / 1000000 + '%';
    },
    formatLess1Percent(num) {
      if (isNaN(num)) return '';
      var v = parseInt(num * 1000000);
      return v / 10000 + '%';
    }
  },
  methods: {
    //统一处理模态对话框，不让它点空白处关闭
    handleDlgClose: function (done) {},
    //以下是配合element-ui的table的格式化方法, 如要单独使用，则只用传一个参数
    //年月日YYYY-MM-dd
    formatDate(row, col, date, idx) {
      if (arguments.length < 3) date = row;
      return util.formatDate(date)
    },
    //年月日时分秒
    formatDateTime(row, col, date, idx) {
      var format = "yyyy-MM-dd hh:mm:ss";
      if (arguments.length < 3) {
        date = row;
        if (arguments.length == 2) format = col;
      }
      return util.formatDate(date, format)
    },
    //xx万元
    formatWan(row, col, num, idx) {
      if (arguments.length < 3) num = row;
      return util.formatWan(num);
    },
    // xx亿元 或xx万元，取决于数字大小
    formatWanYi(row, col, num, idx) {
      if (arguments.length < 3) num = row;
      return util.formatWanYi(num);
    },
    // 百分数
    formatPercent(row, col, num, idx) {
      if (arguments.length < 3) num = row;
      if (isNaN(num)) return '';
      var v = parseInt(num * 10000);
      return v / 10000 + '%';
    },
    formatLess1Percent(row, col, num, idx) {
      if (arguments.length < 3) num = row;
      if (isNaN(num)) return '';
      var v = parseInt(num * 10000);
      return v / 100 + '%';
    },
  }
});
