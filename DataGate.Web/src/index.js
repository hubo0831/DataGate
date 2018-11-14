//此文件作为公共库的入口文件

// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import 'font-awesome/css/font-awesome.css'
import Vue from 'vue'
import ElementUI from 'element-ui'
import 'element-ui/lib/theme-chalk/index.css'

import appConfig from './appConfig.js'

Vue.config.productionTip = false;
Vue.use(ElementUI);

import "./formatmixin"
import App from './App'
import routerObj from "./router.js";
import userState from './userState'
import util from "./common/util"
import editTask from "./common/editTask"
import bus from "./bus"
import * as api from "./api"

import Login from "./Login.vue"
//组件注册
import UrlPager from './components/url-pager'
import UserSelect from './components/user-select'
import Fileupload from "./components/file-upload";
import DisplayItem from "./components/display-item";
import EditItem from "./components/edit-item";
import EditGrid from "./components/edit-grid";
import SearchForm from "./components/search-form";
import EditForm from "./components/edit-form";
import PlainTree from "./components/plain-tree";
import TreeSelect from "./components/tree-select";
import pubmixin from "./pages/pubmixin.js";
import taskmixin from "./pages/taskmixin"

//首页标题栏注册，可以被改写
import SiteTitle from "./components/site-title"
import LoginBottom from "./components/login-bottom"

Vue.component('UrlPager', UrlPager)
Vue.component("UserSelect", UserSelect)
Vue.component("Fileupload", Fileupload)
Vue.component("DisplayItem", DisplayItem)
Vue.component("EditItem", EditItem)
Vue.component("EditGrid", EditGrid)
Vue.component("SearchForm", SearchForm)
Vue.component("EditForm", EditForm)
Vue.component("PlainTree", PlainTree)
Vue.component("TreeSelect", TreeSelect)

Vue.component("SiteTitle", SiteTitle)
Vue.component("LoginBottom", LoginBottom)

Vue.component("Login", Login)

//导出常用状态和工具类
export const RouterObj = routerObj; //主要用到RouterObj.addPages(path, pageComponent)
export const UserState = userState;
export const AppConfig = appConfig;
export const Util = util;
export const EditTask = editTask;
export const API = api;
export const Bus = bus;
export const PubMixin = pubmixin; 
export const TaskMixin = taskmixin;

//eslint-disable no-new
//导出Vue根元素
export const Run = () => new Vue({
  el: '#app',
  router: routerObj.router,
  template: '<App/>',
  components: {
    App
  }
});
