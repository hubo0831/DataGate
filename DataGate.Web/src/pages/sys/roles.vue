<template>
  <!-- 角色管理 -->
  <el-row>
    <el-col :span="24">
      <div class="dg-toolbar">
        <el-button-group>
          <el-button type="primary" icon="fa fa-plus" v-on:click="doCmd('doAdd')">新增角色</el-button>
          <el-button type="primary" icon="fa fa-edit" v-on:click="doCmd('doEdit')">修改</el-button>
          <el-button type="primary" icon="fa fa-trash-o" v-on:click="doCmd('doDel')">删除</el-button>
          <el-button type="primary" icon="fa fa-save" :loading="saving" v-on:click="doSave()">保存</el-button>
        </el-button-group>
      </div>
      <el-row :gutter="5">
        <el-col :span="12" v-loading="loading">
          <edit-grid
            :task="task"
            id="dataGrid"
            :height="fitHeight('#dataGrid')"
            ref="dataGrid"
            edit-mode="inline"
            show-index
            @current-change="doCurrentChange"
          ></edit-grid>
        </el-col>
        <el-col :span="12">
          <div class="card">
            <div class="card-header">
              <i class="fa fa-check-square-o" aria-hidden="true"></i>
              功能列表 - {{current? current.name :'请先选择一个角色'}}
            </div>
            <div class="card-content dg-fit dg-scr" id="menuDiv">
              <el-tree
                :data="menus"
                show-checkbox
                default-expand-all
                node-key="id"
                ref="tree"
                highlight-current
                :props="defaultProps"
              ></el-tree>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-col>
  </el-row>
</template>
<script>
import taskmixin from "../taskmixin.js";
import * as API from "../../api";
import util from "../../common/util";
export default {
  mixins: [taskmixin],
  data: function() {
    return {
      menus: [], //树形结构的菜单全集
      plainMenus:[], //平面结构的菜单全集
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
        this.task.productName = "角色";
        this.plainMenus = menus[0];
        this.menus = util.buildNestData(menus[0]);
      }
    );
    this.loadData();
  },
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

      if (!item) return;

      //自动勾选所有角色有的菜单
      var checkedMenuIds = item.menus.map(m => m.menuId);
      this.$refs.tree.setCheckedKeys(checkedMenuIds);

      //由于存在级联勾选，所以再自动去掉角色没有权限的勾选
      var unchkedMenuIds = this.plainMenus
        .map(m => m.id)
        .filter(id => !checkedMenuIds.includes(id));
      unchkedMenuIds.forEach(id => this.$refs.tree.setChecked(id, false));
    },
    doSave() {
      this.$refs.dataGrid.submitRow().then(() => {
        this.setRoleMenus();
        this.save();
      });
    },
    save() {
      // this.task.acceptChanges();
      if (!this.task.testChanged()) {
        //why this.task.changed 不行？
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
