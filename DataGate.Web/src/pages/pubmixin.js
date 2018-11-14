import * as API from '../api'
//公共混入对象,用于顶层页面
//不要Vue.mixin全局混入，会产生冲突，如loading
export default {
  created: function () {
    var that = this;
    API.setContext(this);
    var resizeFunc = function () {
      that.pageHeight = $(window).height();
      that.pageWidth = $(window).width();
    };
    $(window).bind("resize", resizeFunc);
    resizeFunc();
  },
  watch:{
    saving(val){
      if (val){
        this.loading = false;
      }
    }
  },
  beforeRouteLeave: function (to, from, next) {
    var ok = !this.task.changed || window.confirm('有修改尚未保存，是否放弃修改？');
    if (ok) {
      next();
    } else {
      next(false);
    }
  },
  data: function () {
    return {
      pageHeight: 0, //页面高度
      pageWidth: 0,
      loading: false, //显示加载中...  
      saving: false //显示 保存中...
    };
  },

  methods: {
  }
}
