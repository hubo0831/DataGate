<template>
<!-- 角色管理 -->
  <el-row>
    <el-col :span="24">
      <div class="dg-toolbar">
        <el-button-group>
          <el-button size="mini" type="primary" icon="fa fa-plus" v-on:click='doCmd("doAdd")'>新增角色</el-button>
          <el-button size="mini" type="primary" icon="fa fa-edit" v-on:click='doCmd("doEdit")'>修改</el-button>
          <el-button size="mini" type="primary" icon="fa fa-trash-o" v-on:click='doCmd("doDel")'>删除</el-button>
          <el-button size="mini" type="primary" icon="fa fa-save" :loading="saving" v-on:click='doSave()'>保存</el-button>
        </el-button-group>
      </div>
      <el-row :gutter="5">
        <el-col :span="12" v-loading="loading">
          <edit-grid :task="task" :height="pageHeight-140" ref="dataGrid" edit-mode="inline" show-index @current-change="doCurrentChange"></edit-grid>
        </el-col>
        <el-col :span="12">
          <div class="card">
            <div class="card-header"><i class="fa fa-check-square-o" aria-hidden="true"></i> 功能列表</div>
            <div class="card-content" id="menuDiv" :style="{height:(pageHeight-195) +'px'}">
              <el-tree :data="menus" show-checkbox default-expand-all node-key="id" ref="tree" highlight-current :props="defaultProps">
              </el-tree>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-col>
  </el-row>
</template>
<script>
  // import "jquery-slimscroll";
  import taskmixin from "../taskmixin.js";
  import * as API from "../../api";
  import util from "../../common/util";
  export default {
    mixins: [taskmixin],
    mounted: function () {
      $("#menuDiv").slimScroll({});
    },
    data: function () {
      return {
        menus: [],
        defaultProps: {
          children: "children",
          label: "name"
        },
        current: null
      };
    },
    created() {
      //元数据准备
      $.when(API.META("getallrolemenus"), API.QUERY("GetAuthMenus")).done(
        (meta, menus) => {
          this.task.setMetadata(meta[0]);
          this.menus = util.buildNestData(menus[0]);
        }
      );
      this.loadData();
    },
    computed: {},
    methods: {
      loadData() {
        API.QUERY("getallrolemenus").done(result => {
          this.task.clearData();
          this.task.products = result;
          this.task.createDetails("menus", "saveRoleMenu");
        });
      },
      doCmd(cmd) {
        this.$refs.dataGrid[cmd]();
      },
      //勾选右侧功能树结点后给角色授权
      setRoleMenus() {
        if (this.current) {
          this.task.editBuffer.menus = this.$refs.tree
            .getCheckedKeys()
            .map(id => ({
              roleId: this.current.id,
              menuId: id
            }));
          this.task.acceptChanges();
        }
      },
      doCurrentChange(item) {
        this.setRoleMenus();
        this.current = item;
        if (item) this.$refs.tree.setCheckedKeys(item.menus.map(m => m.menuId));
      },
      doSave() {
        this.$refs.dataGrid.submitRow().then(() => {
          this.setRoleMenus();
          this.save();
        });
      },
      save() {
       // this.task.acceptChanges();
        if (!this.task.testChanged()){ //why this.task.changed 不行？
          this.$message.info("没有修改需要保存");
          return;
        }
        API.SUBMIT("saverole", this.task.createSaveData())
          .done(() => this.$message.success("保存成功！"))
          .done(this.loadData);
      }
    }
  };

</script>
<style scoped lang="scss">
</style>
