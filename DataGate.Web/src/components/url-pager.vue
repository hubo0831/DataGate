<template>
<!-- 使用url分页,便于用户收藏指定页面-->
    <el-pagination
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
      :current-page="pageIndex"
      :page-sizes="[10, 20, 30, 50, 100]"
      :page-size="pageSize"
      layout="total, sizes, prev, pager, next, jumper"
      :total="total">
    </el-pagination>
</template>
<script>
export default {
  props: {
    total: Number
  },
  inject: ["urlQuery"],
  data() {
    return {
      pageIndex: 1,
      pageSize: 20
    };
  },
  created() {
    this.pageSize = parseInt(this.urlQuery["pagesize"]) || 20;
  },
  watch: {
    total() {
      this.pageIndex = parseInt(this.urlQuery["pageindex"]) || 1;
    }
  },
  methods: {
    handleSizeChange(val) {
      this.urlQuery["pagesize"] = val;
      //如果不想url分页，则调用此事件,并args.passed=false
      if (this.$emitPass("page-change").passed) {
        //直接url跳转
        this.$router.replace({ path: this.$route.path, query: this.urlQuery });
      }
    },
    handleCurrentChange(val) {
      this.urlQuery["pageindex"] = val;
      //如果不想url分页，则调用此事件,并args.passed=false
      if (this.$emitPass("page-change").passed) {
        //直接url跳转
        this.$router.replace({ path: this.$route.path, query: this.urlQuery });
      }
    }
  }
};
</script>
