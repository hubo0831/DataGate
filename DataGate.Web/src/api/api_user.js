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
    if (userState.currentUser && userState.currentUser.name) {
      resolve(userState.currentUser);
      return;
    }
    API.GET('/api/Check/GetUser').then(user => {
      if (!user.$code) {
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
  return loginUrl('/api/Check/Login', account);
}

//使用指定的url地址登录
function loginUrl(url, param) {
  return new Promise((resolve, reject) => {
    API.POST(url, param)
      .then(result => {
        if (!result.$code) {
          loginSuccess(result);
          resolve(result);
        } else {
          reject(result);
        }
      });
  });
}

function loginSuccess(result) {
  userState.token = result.token;
  util.setCookie("remember", result.remember, 14 * 24 * 60); //保存密码两周
  bus.$emit("login");
}

//勾了“记住我”后下次的登录
function rememberLogin() {
  var remember = util.getCookie("remember");
  if ((remember || '').length < 10) {
    return Promise.reject(0);
  } else return loginUrl('/api/Check/Login', {
    remember
  });
}

//退出登录，主动退出或超时退出都会进入此方法，则需要保留是否记住我的勾选状态同时清空记住我的内容
function logout() {
  function releaseUser() {
    userState.token = null;
    userState.currentUser = {};
    var remember = util.getCookie("remember");
    if (remember) {
      util.setCookie("remember", 1);
    } else {
      util.removeCookie("remember");
    }
    bus.$emit('logout');
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
  rememberLogin,
  //登出
  logout,
  loginUrl,
}
