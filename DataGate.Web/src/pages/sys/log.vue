<template>
  <!-- 查看系统日志 -->
  <el-row>
    <el-col :span="24">
      <div class="dg-toolbar">
        <el-button-group>
          <!-- <el-button type="primary" icon="fa fa-edit" @click="details">查看详情</el-button> -->
          <el-button type="primary" icon="fa fa-edit" @click="clear()">清空</el-button>
          <el-button type="primary" icon="fa fa-trash-o" @click="doCmd('doDel')">删除</el-button>
        </el-button-group>
      </div>
      <div class="search-form">
        <search-form :metadata="searchMeta"></search-form>
      </div>
    </el-col>
    <el-col :span="24">
      <edit-grid
        :task="task"
        id="dataGrid"
        :height="fitHeight('#dataGrid')-33"
        ref="dataGrid"
        :show-index="false"
        multi-select
        @row-dblclick="details"
        @before-del-rows="confirmDel"
        @after-del-rows="submitDel"
        edit-mode="none"
      ></edit-grid>
      <url-pager :total="total"></url-pager>
    </el-col>
  </el-row>
</template>
<script>
import taskmixin from "../taskmixin.js";
import * as API from "../../api";
export default {
  mixins: [taskmixin],
  data: function() {
    return {
      searchMeta: [] //查询条件
    };
  },
  created() {
    API.META("GetSysLogs")
      .done(meta => {
        this.task.setMetadata(meta); //不能直接赋值
        this.task.productName = "日志";
        this.searchMeta = this.task.reDefineMetadata(
          "message,loglevel,optime"
        );
      })
      .done(this.loadData);
  },
  methods: {
    loadData() {
      this.apiUrlPageQuery("GetSysLogs");
    },
    doCmd(cmd) {
      this.$refs.dataGrid[cmd]();
    },
    confirmDel(args) {
      args.passed = confirm("确认删除所选日志？");
    },
    submitDel() {
      this.apiSubmit("delLog", "删除成功");
    },
    clear() {
      if (confirm("是否要清除全部日志？"))
        API.NONQUERY("clearLog").done(this.loadData);
    },
    details({ log }) {}
  }
};
</script>
