/**
 * Created by jerry on 2017/4/14.
 */
var SIGN_REGEXP = /([yMdhsm])(\1*)/g
var DEFAULT_PATTERN = 'yyyy-MM-dd'

function padding(s, len) {
  let l = len - (s + '').length
  for (var i = 0; i < l; i++) {
    s = '0' + s
  }
  return s
};

export default {
  /*
   * 根据平行数据处理成树型结构对象,会新增一个children存放子节点对象集合
   * [
   *  属性原有字段...
   *  children:[{},{}]
   * ]
   * dataList = 数组集合
   * id=数据主键id
   * parentId=父节点id
   */
  buildNestData(dataList, id, parentId) {
    if (!id) id = "id";
    if (!parentId) parentId = "parentId";
    if (typeof (parentId) == "undefined") parentId = "parentId";
    //递归设置节点的子节点对象
    var getTreeChildrenNodes = function (nodeItem, list) {
      nodeItem.children = []; //先赋值清空 ，否则下次出现双份
      list.forEach(function (item, index) {
        if ((nodeItem[id] == item[parentId]) && item[parentId]) {
          nodeItem.children.push(item);
          getTreeChildrenNodes(item, list);
        }
      });
    }

    //获取所有根节点
    var dataRes = [];
    //
    dataList.forEach(function (item, index) {
      //item父结点ID为空或根据父结点ID在dataList中找不到父节点
      if (!(item[parentId] && dataList.find(d => d[id] == item[parentId]))) {
        dataRes.push(item)
        //设置根节点的子节点对象
        getTreeChildrenNodes(item, dataList)
      }
    });
    return dataRes;
  },
  getQueryStringByName: function (name) {
    var reg = new RegExp('(^|&)' + name + '=([^&]*)(&|$)', 'i')
    var r = window.location.search.substr(1).match(reg)
    var context = ''
    if (r != null) {
      context = r[2]
    }
    reg = null
    r = null
    return context === null || context === '' || context === 'undefined' ? '' : context
  },
  //万和亿的数字显示
  formatWanYi(num) {
    if (num >= 100000000) {
      return num / 100000000 + "亿";
    }
    if (num >= 10000) {
      return num / 10000 + "万";
    }
    return num;
  },
  //万和亿的数字显示
  formatWan(num) {
    if (num >= 10000) {
      return num / 10000 + "万";
    }
    return num;
  },
  formatDate(date, pattern) {
    if (!date) return '';
    date = new Date(date);
    pattern = pattern || DEFAULT_PATTERN
    return pattern.replace(SIGN_REGEXP, function ($0) {
      switch ($0.charAt(0)) {
        case 'y':
          return padding(date.getFullYear(), $0.length)
        case 'M':
          return padding(date.getMonth() + 1, $0.length)
        case 'd':
          return padding(date.getDate(), $0.length)
        case 'w':
          return date.getDay() + 1
        case 'h':
          return padding(date.getHours(), $0.length)
        case 'm':
          return padding(date.getMinutes(), $0.length)
        case 's':
          return padding(date.getSeconds(), $0.length)
      }
    })
  },
  parseDate: function (dateString, pattern) {
    var matchs1 = pattern.match(SIGN_REGEXP)
    var matchs2 = dateString.match(/(\d)+/g)
    if (matchs1.length === matchs2.length) {
      var _date = new Date(1970, 0, 1)
      for (var i = 0; i < matchs1.length; i++) {
        var _int = parseInt(matchs2[i])
        var sign = matchs1[i]
        switch (sign.charAt(0)) {
          case 'y':
            _date.setFullYear(_int);
            break
          case 'M':
            _date.setMonth(_int - 1);
            break
          case 'd':
            _date.setDate(_int);
            break
          case 'h':
            _date.setHours(_int);
            break
          case 'm':
            _date.setMinutes(_int);
            break
          case 's':
            _date.setSeconds(_int);
            break
        }
      }
      return _date
    }
    return null
  },
  //判断对象各属性是否相等，只判断两层
  isEqual: function (a, b) {
    if (a == b) {
      return true;
    }
    if (!a || !b) return false;
    if (typeof a == "array" && typeof b == "array" && a.length == b.length) {
      a = a.slice(0).sort();
      b = b.slice(0).sort();
      for (var i in a) {
        if (a[i] != b[i]) {
          return false;
        }
      }
      return true;
    }
    if (typeof a == "object" && typeof b == "object") {
      var c = {};
      for (var i in a) {
        c[i] = 1;
      }
      for (var i in b) {
        c[i] = 1;
      }
      for (var i in c) {
        if (a[i] != b[i]) {
          return false;
        }
      }
      return a.toString() == b.toString();
    }
    return false;
  },
  //使用一个方法返回的值排升序
  sort(arr, ordFunc) {
    var ord = null;
    if (typeof (ordFunc) == "string") {
      ord = ordFunc;
    }
    return arr.sort((a, b) => {
      if (ord) {
        a = a[ord];
        b = b[ord];
      } else {
        a = ordFunc(a);
        b = ordFunc(b);
      }
      if (a > b) return 1;
      if (a == b) return 0;
      if (a < b) return -1;
    });
  },
  //使用一个方法返回的值排降序
  sortDesc(arr, ordFunc) {
    var ord = null;
    if (typeof (ordFunc) == "string") {
      ord = ordFunc;
    }
    return arr.sort((a, b) => {
      if (ord) {
        a = a[ord];
        b = b[ord];
      } else {
        a = ordFunc(a);
        b = ordFunc(b);
      }
      if (a < b) return 1;
      if (a == b) return 0;
      if (a > b) return -1;
    });
  },
  //数组去重, 指定主键的属性名或求返回主键值的函烽
  //遇重复时，以后面的元素为准
  distinct(arr, prop) {
    var r = [];
    var t = {};
    if (typeof prop == "function") {
      arr.forEach(a => t[prop(a)] = a);
    } else {
      arr.forEach(a => t[a[prop]] = a);
    }
    for (var j in t) {
      r.push(t[j]);
    }
    return r;
  },
  //设置cookie
  setCookie(cname, cvalue, exminutes) {
    cname  += window.location.port; //防止同一端口不同网站cookie相同
    if (typeof exminutes == 'number') {
      var d = new Date();
      d.setTime(d.getTime() + (exminutes * 60 * 1000));
      var expires = "expires=" + d.toUTCString();
      document.cookie = cname + "=" + cvalue + "; " + expires;
    } else {
      document.cookie = cname + "=" + cvalue;
    }
  },
  //移除cookie
  removeCookie(cname) {
    this.setCookie(cname, '', -1000);
  },
  //获取cookie
  getCookie(cname) {
    var name = cname + window.location.port + "="; //防止同一端口不同网站cookie相同
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
      var c = ca[i];
      while (c.charAt(0) == ' ') c = c.substring(1);
      if (c.indexOf(name) != -1) return c.substring(name.length, c.length);
    }
    return "";
  },
  // https://www.cnblogs.com/wuyuchang/p/3956656.html
  //  从网上看到这个方法，兼顾了时间，楼主看看：
  guid() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = (d + Math.random() * 16) % 16 | 0;
      d = Math.floor(d / 16);
      return (c == 'x' ? r : (r & 0x7 | 0x8)).toString(16);
    });
    return uuid.toUpperCase();
  },

  //返回一个无条件pass的promise
  emptyPromise(obj) {
    return new Promise((resolve) => {
      resolve(obj)
    });
  },
  //判断一个对象是空值或空数组
  // 0不包含在内
  isEmpty(obj) {
    //是空值或未定义或空字符串
    if (obj == null || typeof obj == "undefined" || obj == '') return true;

    //是空数组
    return Array.isArray(obj) && !obj.length;
  }
}
