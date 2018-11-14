/**
 * 用户相关api
 */
import * as API from './'
import util from "../common/util.js"
import appConfig from "../appConfig.js"
import userState from "../userState.js"
import bus from "../bus"
//获取用户完整信息，包括有权限的菜单
function getUserInfo() {
  return new Promise((resolve, reject) => {
    if (userState.currentUser) {
      resolve(userState.currentUser);
      return;
    }
    API.GET('/api/Check/GetUser').then(user => {
      if (user.$code == 0) {
        userState.currentUser = user;
        resolve(user);
      } else {
        reject(user);
      }
    });
  });
}

//用户登录，获取唯一的token存于cookie
function login(account) {
  return new Promise((resolve, reject) => {
    API.POST('/api/Check/Login', account)
      .then(result => {
        if (result.$code == 0) {
          userState.token = result.token;
          var returnUrl = util.getQueryStringByName("returnUrl");
          bus.$emit("login", returnUrl);
          resolve();
        } else {
          reject(result);
        }
      });
  });
}

//退出登录，用试图进入的页面作为登录后的返回页
function logout() {
  function releaseUser() {
    userState.token = null;
    userState.currentUser = null;
    var returnUrl = window.location.hash.substring(2);
    if (returnUrl) {
      returnUrl = "?returnUrl=" + returnUrl;
    } else {
      returnUrl = '';
    }
    bus.$emit('logout', returnUrl);
  }
  if (!userState.token) {
    releaseUser();
  } else {
    API.POST('/api/Check/Logout').always(releaseUser);
  }
}

export default {
  getUserInfo,
  //登录
  login,

  //登出
  logout,
}
