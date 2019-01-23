import * as API from '../api'
//公共混入对象,用于顶层页面,负责提供loading, saving, pageHeight pageWidth公共对象和自动调整dg-fit的高度
//不要Vue.mixin全局混入，会产生冲突，如loading
export default {
  created: function () {
    API.setContext(this);
    $(window).bind("resize", this.autoSize);
  },
  watch: {
    saving(val) {
      if (val) {
        this.loading = false;
      }
    }
  },
  mounted() {
    this.autoSize();
    if ($(".dg-scr").length > 0)
      $(".dg-scr").slimScroll({});
  },
  destoryed() {
    $(window).unbind("resize", this.autoSize);
  },
  data: function () {
    return {
      heightFactor: 20, //高度调整的减数，靠试
      heightSelectors: {},
      pageHeight: 0, //页面高度
      pageWidth: 0,
      loading: false, //显示加载中...  
      saving: false //显示 保存中...
    };
  },
  // directives: {
  //   fitHeight: {
  //     bind(el, binding, vnode) {
  //       if (binding.value === false) return;
  //       var d = $(el);
  //       var y = d.offset().top;
  //       var height = this.pageHeight - y - this.heightFactor;
  //       // console.log('height=' +height);
  //       if (vnode.componentInstance)
  //         vnode.componentInstance.height = height;
  //       else
  //         d.height(height);
  //     }
  //   },
  //   autoScroll: {
  //     inserted(el, binding) {
  //       if (binding.value === false) return;
  //       $(el).slimScroll({});
  //     }
  //   }
  // },
  methods: {
    autoSize() {
      this.pageHeight = $(window).height();
      this.pageWidth = $(window).width();

      //自动调整class=dg-fit的元素的高度自适应到页面底部
      //避免每个页面都要手动调整为pageHeight减一个固定值
      setTimeout(() => {
        $('.dg-fit').each((idx, div) => {
          var d = $(div);
          var y = d.offset().top;
          var height = this.pageHeight - y - this.heightFactor;
          // console.log('height=' +height);
          d.height(height);
        });
      }, 0);

      for (var selector in this.heightSelectors) {
        this.fitHeight(selector);
      }
    },
    fitHeight(selector) {
      var d = $(selector);
      //   console.log(this.heightSelectors); //当命名为_hieghtSelectors找不到对象？
      if (d.length == 0) {
        this.$set(this.heightSelectors, selector, null);
      } else {
        var y = d.offset().top;
        this.heightSelectors[selector] = this.pageHeight - y - 3; //-3完全是靠试
      }
      return this.heightSelectors[selector];
    }
  }
}
