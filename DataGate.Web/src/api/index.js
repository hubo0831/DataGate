import appConfig from '../appConfig.js'
import bus from "../bus"
import util from "../common/util.js"
import userState from "../userState.js"

function formatUrl(url) {
  if (url.startsWith("http://") || url.startsWith("https://")) {
    return url;
  }
  if (!url.startsWith('/')) {
    url = '/' + url;
  }
  return appConfig.apiUrl + url;
};

let context = null; //调用此模块的vue对象，主要是获取它的弹出信息框

function showError(xhr, err, e) {
  var ex = xhr.responseJSON;
  if (ex && appConfig.debug) {
    this.$notify.error({
      title: ex.exceptionType,
      message: ex.message + "\n点击查看详情",
      onClick: () => bus.$emit("server-exception", ex)
    })
  } else {
    this.$message.error('请求出错:' + (e || err));
  }
}

function closeAll() {
  this.loading = false;
  this.saving = false;
}

export const setContext = function (vue) {
  context = vue;
}

//post 键-值对传参
export const POST = (url, params) => {
  context.loading = true;
  url = formatUrl(url);
  return $.ajax({
      headers: {
        token: userState.token
      },
      cache:false,
      data: params,
      url: url,
      crossDomain: true,
      context,
      type: 'post'
    }).fail(showError)
    .always(closeAll);
}

//get方法url传参
export const GET = (url, params) => {
  context.loading = true;
  url = formatUrl(url);
  return $.ajax({
      headers: {
        token: userState.token
      },
      cache:false,
      data: params,
      url: url,
      crossDomain: true,
      context,
      type: 'get'
    }).fail(showError)
    .always(closeAll);
}
//解决JSON.stringify序列化日期时转成了ISO时间的问㓳
//注意中间要加T，否则Oracle会报错
Date.prototype.toJSON = function () {
  return util.formatDate(this, "yyyy-MM-ddThh:mm:ss");
}
//json传值提交
export const JPOST = (url, params) => {
  context.loading = true;
  url = formatUrl(url);
  return $.ajax({
      headers: {
        token: userState.token
      },
      cache:false,
      data: JSON.stringify(params),
      url: url,
      crossDomain: true,
      contentType: 'application/json',
      context,
      type: 'post'
    }).fail(showError)
    .always(closeAll);
}

function filterResult(result) {
  if (!result.$code ) return;
  if (result.$code == 1010) {
    bus.$emit("session-timeout", result);
  } else if (result.$message) {
    this.$message.error(result.$message);
  }
}

//通用查询
export const QUERY = (key, params) => {
  return GET('/api/dg/' + key, params).done(filterResult);
}

//通用批量更新
export const SUBMIT = (key, params) => {
  context.saving = true;
  return JPOST('/api/dg/s/' + key, params).done(filterResult);
}

//获取元数据
export const META = key => {
  return GET('/api/dg/m/' + key).done(filterResult);
}
