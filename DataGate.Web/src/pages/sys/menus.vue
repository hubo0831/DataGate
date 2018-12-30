<template>
 <!-- 菜单管理主界面 -->
 <el-row :gutter="5">
    <div class="dg-toolbar">
      <el-button-group>
        <el-button type="primary" icon="fa fa-plus" v-on:click='doCmd("addBrother")'>新增平级</el-button>
        <el-button type="primary" icon="fa fa-plus" v-on:click='doCmd("addChild")'>新增子级</el-button>
        <el-button type="primary" icon="fa fa-plus" v-on:click='doCmd("clone")'>克隆</el-button>
        <el-button type="primary" icon="fa fa-long-arrow-up" v-on:click='doCmd("doUp")'>上移</el-button>
        <el-button type="primary" icon="fa fa-long-arrow-down" v-on:click='doCmd("doDown")'>下移</el-button>
        <el-button type="primary" icon="fa fa-trash-o" v-on:click='doCmd("doDel")'>删除</el-button>
        <el-button type="primary" icon="fa fa-save" :loading="saving" v-on:click='doSave()'>保存</el-button>
      </el-button-group>
    </div>
    <el-col :span="12">
      <div class="card">
        <div class="card-header"><i class="fa fa-check-square-o" aria-hidden="true"></i> 功能列表</div>
        <div class="card-content dg-fit dt-scr" id="menusDiv">
          <!-- <el-input placeholder="输入名称过滤" v-model="filterText">
          </el-input> -->
          <plain-tree :task="task" ref="plainTree">
            <template slot-scope="scope">
              <div class="menu-text">
                <i :class="scope.data.iconCls||'fa fa-file-o'"></i>
                <span>{{ scope.node.label }}</span>
                <div class="menu-status">
                  <i class="fa fa-navicon" v-if="scope.data.showType=='UserMenu'" title="导航菜单" @click.stop="switchShowType(scope.data)"></i>
                  <i class="fa fa-minus" v-else title="功能页" @click.stop="switchShowType(scope.data)"></i>
                  <i class="fa fa-lock" v-if="scope.data.authType=='Auth'" title="权限控制" @click.stop="switchAuthType(scope.data)"></i>
                  <i class="fa fa-unlock" v-else-if="scope.data.authType=='AllUsers'" title="所有用户" @click.stop="switchAuthType(scope.data)"></i>
                  <i class="fa fa-ban" v-else title="暂时停用" @click.stop="switchAuthType(scope.data)"></i>
                </div>
              </div>
            </template>
          </plain-tree>
        </div>
      </div>
    </el-col>
    <el-col :span="12">
      <div class="card">
        <div class="card-header"><i class="fa fa-check-square-o" aria-hidden="true"></i> 功能属性</div>
        <div class="card-content">
          <edit-form ref="editorForm" :task="task"></edit-form>
          <p style="text-align:center"><a href="https://www.thinkcmf.com/font/font_awesome/icons.html" target="_blank">点此处打开FontAwesome网页选择合适的图标。</a>
          </p>
        </div>
      </div>
    </el-col>
  </el-row>
</template>
<script>
import taskmixin from "../taskmixin.js";
import * as API from "../../api";
import util from "../../common/util";
export default {
  mixins: [taskmixin],
  data() {
    return {
      menus: []
    };
  },
  mounted() {
    $("#menusDiv").slimScroll({});
  },
  created() {
    //元数据准备
    API.META("GetAllMenus").done(metadata => {
      this.task.setMetadata(metadata);
      this.task.productName = "功能";
    });
    this.loadData();
  },
  methods: {
    loadData() {
      API.QUERY("GetAllMenus").done(menus => {
        this.task.clearData();
        this.task.products = menus;
      });
    },
    doCmd(cmd) {
      this.$refs.plainTree[cmd]();
    },
    switchShowType(menu) {
      if (menu.showType == "UserMenu") {
        menu.showType = "";
      } else {
        menu.showType = "UserMenu";
      }
      if (menu.id == this.task.editBuffer.id) {
        this.task.editBuffer.showType = menu.showType;
      }
      this.task.changeStatus(menu, "changed");
    },
    switchAuthType(menu) {
      if (menu.authType == "Auth") {
        menu.authType = "AllUsers";
      } else if (menu.authType == "AllUsers") {
        menu.authType = "Forbidden";
      } else {
        menu.authType = "Auth";
      }
      if (menu.id == this.task.editBuffer.id) {
        this.task.editBuffer.authType = menu.authType;
      }
      this.task.changeStatus(menu, "changed");
    },
    doSave() {
      this.task.acceptChanges();
      if(!this.task.changed){
        this.$message.info("没有修改无需保存");
      }
      this.$refs.plainTree
        .validate()
        .then(() => {
          return API.SUBMIT("savemenu", this.task.createSaveData());
        })
        .then(() => {
          this.$message.success("保存成功！");
          this.loadData(); //不重新加载就会报主键冲突
        });
    }
  }
};
</script>
<style lang="scss">
.menu-text {
  position: relative;

  .menu-status {
    position: absolute;
    top: 3px;
    right: 5px;
    color: lightslategray;

    i {
      width: 20px;
    }
  }
}
</style>
