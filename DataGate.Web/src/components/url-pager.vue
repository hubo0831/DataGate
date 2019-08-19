<template>
  <!-- 使用url分页,便于用户收藏指定页面-->
  <el-pagination
    @size-change="handleSizeChange"
    @current-change="handleCurrentChange"
    :current-page.sync="pageIndex"
    :page-sizes="[10, 20, 30, 50, 100, 150]"
    :page-size.sync="pageSize"
    layout="total, sizes, prev, pager, next, jumper"
    :total="total"
  ></el-pagination>
</template>
<script>
import { Util } from "../";
export default {
  props: {
    total: Number
  },
  data() {
    return {
      pageIndex: 1,
      pageSize: 20
    };
  },
  created() {
    this.pageSize =
      parseInt(this.$route.query["pagesize"]) ||
      parseInt(Util.getCookie("pageSize")) ||
      20;
  },
  watch: {
    total() {
      this.pageIndex = parseInt(this.$route.query.pageindex) || 1;
    }
  },
  methods: {
    handleSizeChange(val) {
      this.$route.query.pagesize = val;
      Util.setCookie("pageSize", val, 24 * 60 * 14);
      //如果不想url分页，则调用此事件,并args.passed=false
      if (
        this.$emitPass("page-change", {
          pageSize: this.pageSize,
          pageIndex: this.pageIndex
        }).passed
      ) {
        //直接url跳转
        this.$router.replace({
          path: this.$route.path,
          query: this.$route.query
        });
      }
    },
    handleCurrentChange(val) {
      let query = $.extend({}, this.$route.query);
      query.pageindex = val;
      //如果不想url分页，则调用此事件,并args.passed=false
      if (
        this.$emitPass("page-change", {
          pageSize: this.pageSize,
          pageIndex: this.pageIndex
        }).passed
      ) {
        //直接url跳转
        this.$router.replace({ path: this.$route.path, query });
      }
    }
  }
};
</script>
