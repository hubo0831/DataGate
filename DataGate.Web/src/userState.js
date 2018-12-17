import appConfig from './appConfig.js'
import util from "./common/util.js"
const keyName = appConfig.appSecret + "_Token";
let currentUser = {}, _token = util.getCookie(keyName);

export default {
  get token() {
    return _token;
  },
  set token(val) {
    _token = val;
    if (!val) {
      util.removeCookie(keyName);
    } else {
      util.setCookie(keyName, val);
    }

  },
  currentUser
}
