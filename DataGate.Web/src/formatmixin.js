import util from "./common/util"
import Vue from "vue";
import appConfig from "./appConfig"
import userState from "./userState"
Vue.mixin({
  data() {
    return {
      appConfig,
      userProfile:userState,
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
      return num * 100 + '%';
    }
  },
  methods: {
    //统一处理模态对话框，不让它点空白处关闭
    handleDlgClose: function (done) {},
    //以下是配合element-ui的table的格式化方法
    //年月日YYYY-MM-dd
    formatDate(row, col, date, idx) {
      return util.formatDate(date)
    },
    //年月日时分秒
    formatDateTime(row, col, date, idx) {
      return util.formatDate(date, "yyyy-MM-dd HH:mm:ss")
    },
    //xx万元
    formatWan(row, col, num, idx) {
      return util.formatWan(num);
    },
    // xx亿元 或xx万元，取决于数字大小
    formatWanYi(row, col, num, idx) {
      return util.formatWanYi(num);
    },
    // 百分数
    formatPercent(row, col, num, idx) {
      var v = parseInt(num * 1000000);
      return v  / 10000 + '%';
    },
  }
});
