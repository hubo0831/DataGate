<template>
  <!-- 用户管理 -->
  <el-row>
    <el-col :span="24">
      <div class="dg-toolbar">
        <el-button-group>
          <el-button type="primary" icon="fa fa-user-plus" v-on:click="doCmd('doAdd')">新增</el-button>
          <el-button type="primary" icon="fa fa-edit" v-on:click="doCmd('doEdit')">修改</el-button>
          <!-- <el-button type="primary" icon="fa fa-edit" v-on:click='disable()'>禁用</el-button> -->
          <el-button type="primary" icon="fa fa-trash-o" v-on:click="doCmd('doDel')">删除</el-button>
        </el-button-group>
      </div>
      <search-form :metadata="searchMeta"></search-form>
      <edit-grid
        :task="task"
        v-loading="loading"
        id="editGrid"
        :height="fitHeight('#editGrid')-33"
        ref="editGrid"
        edit-mode="popup"
        multi-select
        @before-del-rows="confirmDel"
        @after-del-rows="submitDel"
        @save-task="doSave"
      >
        <span slot="editer-title" slot-scope="userIs">
          <template v-if="userIs.newItem">
            <i class="fa fa-plus"></i>
            <i class="fa fa-user"></i> 新增用户
          </template>
          <template v-else>
            <i class="fa fa-user"></i> 修改用户
          </template>
        </span>
      </edit-grid>
      <url-pager :total="task.total"></url-pager>
    </el-col>
  </el-row>
</template>
<script>
// import "jquery-slimscroll";
import TaskMixin from "../taskmixin.js";
import * as API from "../../api";
import Util from "../../common/util";
export default {
  mixins: [TaskMixin],
  data: function() {
    return {
      allRoles: [],
      searchMeta: []
    };
  },
  created() {
    //元数据和角色数据准备
    // a1 and a2 are arguments resolved for the page1 and page2 ajax requests, respectively.
    // Each argument is an array with the following structure: [ data, statusText, jqXHR ]
    $.when(API.META("getallusers"), API.QUERY("getallroles"))
      .done((meta, roles) => {
        this.allRoles = roles[0];
        this.task.setMetadata(meta[0]);
        this.searchMeta = this.task.getSearchMeta();
        var rolemeta = this.task.getMeta("roles");
        rolemeta.options = this.allRoles.map(r => ({
          value: {
            roleId: r.id
          },
          text: r.name
        }));
      })
      .done(this.loadData);
  },
  computed: {},
  methods: {
    loadData() {
      this.apiUrlPageQuery("getallusers").done(result => {
        this.task.createDetails("roles", "SaveUserRole");
      });
    },
    doCmd(cmd) {
      this.$refs.editGrid[cmd]();
    },

    doSave() {
      this.apiSubmit("saveuser", "保存成功！");
    },

    confirmDel(args) {
      args.passed = confirm("确认删除所选用户？");
    },
    submitDel() {
      this.apiSubmit("saveuser", "删除成功");
    }
  }
};
</script>
<style scoped lang="scss">
</style>
