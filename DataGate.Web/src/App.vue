<template>
  <div id="app">
    <el-row class="container" v-if="user">
      <!--头部-->
      <el-col :span="24" class="topbar-wrap">
        <site-title></site-title>
        <div class="topbar-menu">
          <el-menu mode="horizontal" @select="handleMainMenu" :default-active="defaultMainIndex" background-color="#2668ad"
            text-color="#fff" active-text-color="#ffd04b">
            <el-menu-item :index="item.id" v-for="item in mainMenu" :key="item.id">
              <i :class="item.iconCls"></i><span>{{item.name}}</span>
            </el-menu-item>
          </el-menu>
        </div>
        <div class="topbar-account topbar-btn">
          <el-dropdown trigger="click">
            <span class="el-dropdown-link userinfo-inner">
            <i class="fa fa-user-circle-o" aria-hidden="true" style="font-size:20px"></i> {{user.name}}
              <i class="el-icon-arrow-down el-icon--right"></i></span>
            <el-dropdown-menu slot="dropdown">
              <el-dropdown-item v-for="item in selfMenu" :key="item.id">
                <router-link :to="item.url"><i :class="item.iconCls"></i> {{item.name}}</router-link>
              </el-dropdown-item>
              <el-dropdown-item divided @click.native="logout"><i class="fa fa-power-off"></i> 退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </el-dropdown>
        </div>
      </el-col>

      <!--中间-->
      <el-col :span="24" class="main">
        <!--左侧导航-->
        <aside :class="{showSidebar:!collapsed}">
          <!--展开折叠开关-->
          <div class="menu-toggle" @click.prevent="collapse">
            <i class="fa fa-arrow-left" v-show="!collapsed"></i>
            <i class="fa fa-arrow-right" v-show="collapsed"></i>
          </div>
          <!--导航菜单-->
          <el-menu :default-active="defaultChildIndex" :collapse="collapsed" @select="handleSelect">
            <template v-for="item in currentMenu.children">
              <el-submenu v-if="item.children && item.children.length" index="item.id" :key="item.id">
                <template slot="title">
                   <i :class="item.iconCls || 'fa fa-file-o'"></i>
                    <span slot="title">{{item.name}}</span>
                </template>
                <el-menu-item v-for="term in item.children" :key="term.id" :index="term.id">
                               <i :class="term.iconCls || 'fa fa-file-o'"></i>
               <span slot="title">{{term.name}}</span>
                </el-menu-item>
              </el-submenu>
              <el-menu-item v-else :index="item.id" :key="item.id">
                <i :class="item.iconCls || 'fa fa-file-o'"></i>
                 <span slot="title">{{item.name}}</span>
              </el-menu-item>
            </template>
          </el-menu>
        </aside>

        <!--右侧内容区-->
        <section class="content-container">
          <div class="grid-content bg-purple-light">
            <el-col :span="24" class="warp-breadcrum">
              <!-- 面包屑导航 -->
              <el-breadcrumb separator-class="el-icon-arrow-right" replace>
                <el-breadcrumb-item v-for="(item, index) in breadCrumbMenu" v-bind:key="index">
                  <router-link v-if="item.url" v-bind:to="item.url">
                    {{item.name}}
                  </router-link>
                  <span v-else>{{item.name}}</span>
                </el-breadcrumb-item>
              </el-breadcrumb>
            </el-col>
            <el-col :span="24" class="content-wrapper" v-if="logined">
              <transition name="fade" mode="out-in">
                <router-view></router-view>
              </transition>
            </el-col>
          </div>
        </section>
      </el-col>

    </el-row>
    <!-- 在这里不想再套一层Vue-Router -->
     <login v-else />

    <!-- 在服务端报错时，显示服务端错误详情 -->
    <el-dialog title="错误详情" :visible.sync="errorBoxVisible" width="50%">
      <error-page :ex="ex"></error-page>
      <span slot="footer" class="dialog-footer">
        <el-button type="primary" @click="errorBoxVisible = false">确定</el-button>
      </span>
    </el-dialog>
  </div>
</template>

<script>
import util from "./common/util.js";
import userState from "./userState.js";
import UserAPI from "./api/api_user.js";
import routerObj from "./router.js";
import * as API from "./api/index.js";
import ErrorPage from "./pages/errorpage";
import bus from "./bus";
import "./assets/styles/app.scss";
import appConfig from "./appConfig"

export default {
  name: "app",
  data() {
    return {
      errorBoxVisible: false, //错误框
      ex: {}, //异常信息
      defaultMainIndex: "0",
      defaultChildIndex: "不存在",
      user: null, //用户信息，用于显示在左上角的用户姓名,在logount时会置空
      collapsed: false,
      mainMenu: [], //横向的主菜单
      userPages: [], //用户能访问的所有页面的数组
      breadCrumbMenu: [], //面包屑导航
      selfMenu: [], //用户设置菜单
      currentMenu: {}, //当前的子菜单
      logined :false //是否已完成登录全过程
    };
  },
  components: {
    ErrorPage
  },
  created() {
    API.setContext(this);
    var reLogin = result => {
      if (!result.$code) return;
      this.$message.error(result.$message + " 正在重新登录...");
      setTimeout(() => {
        UserAPI.logout();
      }, 2000);
    };
    var getUser = () => {
      this.user = {}; //加此句使得登录框不会刷新页面时闪过
      this.logined = false;
      UserAPI.getUserInfo()
        .then(user => {
          this.user = user;
          this.userPages = user.menus;
          routerObj.createRoutes(user.menus);
          var menuTree = util.buildNestData(user.menus.filter(m => m.showType));
          this.mainMenu = menuTree;
          var home = this.userPages.find(m => m.url == "/");
          this.selfMenu = this.userPages.filter(m => m.parentId == home.id);
          this.restoreMenu();
          this.logined = true;
        })
        .catch(reLogin);
    };

    //导航进入时生成面包屑
    routerObj.router.afterEach((to, from) => {
      var found = false;
      for (var i in this.userPages) {
        var menu = this.userPages[i];
        if (to.matched.some(record => record.meta.id == menu.id)) {
          this.createBreadCrumb({
            id: menu.id,
            name: menu.name,
            parentId: menu.parentId,
            url: to.fullPath
          });
          found = true;
          break;
        }
      }
      if (!found) {
        this.breadCrumbMenu = [];
      }
    });

    bus.$on("server-exception", ex => {
      this.ex = ex;
      this.errorBoxVisible = true;
    });
    bus.$on("update-title", this.updateTitle);
    bus.$on("session-timeout", reLogin);
    bus.$on("login", getUser);
    bus.$on("logout", () => {
      this.user = null;
   //   this.$router.replace("/");
      //  location.reload(); //没办法
    });

    bus.$emit("app-start");
    var token = userState.token;
    if (token) {
      getUser();
    }
  },
  methods: {
    //判断某个菜单是不是主菜单的下级
    isSubMenu(subMenu, mainMenu) {
      if (subMenu.id == mainMenu.id) return true;
      var parentId = subMenu.parentId;
      while (parentId) {
        var parent = this.userPages.find(p => p.id == parentId);
        if (parent.id == mainMenu.id) return true;
        parentId = parent.parentId;
      }
      return false;
    },
    //刷新页面时能恢复上次的菜单选择
    restoreMenu() {
      //从已生成面包屑的导航的url从右往左找在当前菜单中的位置
      for (var ii = this.breadCrumbMenu.length - 1; ii >= 0; ii--) {
        var bid = this.breadCrumbMenu[ii].id;
        for (var i in this.userPages) {
          var child = this.userPages[i];
          if (child.id == bid) {
            this.defaultChildIndex = child.id;
            for (var j in this.mainMenu) {
              if (this.isSubMenu(child, this.mainMenu[j])) {
                this.defaultMainIndex = this.mainMenu[j].id;
                this.currentMenu = this.mainMenu[j];
                return;
              }
            }
            return;
          }
        }
      }
    },
    handleMainMenu(index) {
      var main = this.mainMenu.find(m => m.id == index);
      this.currentMenu = main;
      if (main.url) {
        this.$router.push(main.url);
      }
    },
    handleSelect(index) {
      this.defaultChildIndex = index;
      var subMenu = this.userPages.find(m => m.id == index);
      if (subMenu && subMenu.url) this.$router.push(subMenu.url);
    },
    //折叠导航栏
    collapse() {
      this.collapsed = !this.collapsed;
    },
    //生成面包屑导航
    createBreadCrumb(menu) {
      var breadcrumb = [];
      var i = 0; //防止死循环
      while (menu && i < 8) {
        var cmenuOld = this.breadCrumbMenu.find(m => m.id == menu.id);
        var cmenu;
        if (i > 0 && cmenuOld) {
          cmenu = cmenuOld;
        } else {
          cmenu = {
            id: menu.id,
            name: menu.name,
            url: menu.url
          };
        }
        breadcrumb.unshift(cmenu);
        menu = this.userPages.find(m => m.id == menu.parentId);
        i++;
      }

      this.breadCrumbMenu = breadcrumb;
    },
    //修改面包屑最后一层和窗口标题
    updateTitle(displayName) {
      this.breadCrumbMenu[this.breadCrumbMenu.length - 1].name = displayName;
      window.document.title = displayName + " - " + appConfig.systemName;
    },
    logout() {
      UserAPI.logout();
    }
  },
  mounted() {
    //  console.log("MOUNTED-HOME");
  }
};
</script>
