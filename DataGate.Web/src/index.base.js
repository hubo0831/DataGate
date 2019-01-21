//此文件作为不带Element-UI和Router的公共库的入口文件

// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import appConfig from './appConfig.js'

Vue.config.productionTip = !appConfig.debug;

import "./formatmixin"
import userState from './userState'
import util from "./common/util"
import editTask from "./common/editTask"
import bus from "./bus"
import * as api from "./api"

import Login from "./Login.vue"
//组件注册

import pubmixin from "./pages/pubmixin.js";
import taskmixin from "./pages/taskmixin"

//提供外部注册组件的事件
bus.$on("register", (name, component) => {
  if (typeof name == "function" || typeof name == "object") {
    Vue.use(name);
  } else {
    Vue.component(name, component);
  }
});

//导出常用状态和工具类
export const UserState = userState;
export const AppConfig = appConfig;
export const Util = util;
export const EditTask = editTask;
export const API = api;
export const Bus = bus;
export const PubMixin = pubmixin;
export const TaskMixin = taskmixin;
