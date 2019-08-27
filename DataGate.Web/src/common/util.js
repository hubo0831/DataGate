/**
 * Created by jerry on 2017/4/14.
 */
const SIGN_REGEXP = /([yMdhsmf])(\1*)/g
const DEFAULT_PATTERN = 'yyyy-MM-dd'

function padding(s, len) {
    let l = len - (s + '').length
    for (var i = 0; i < l; i++) {
        s = '0' + s
    }
    return s
};

let mineTypes = {
    //{后缀名，MIME类型}   
    "3gp": "video/3gpp",
    "apk": "application/vnd.android.package-archive",
    "asf": "video/x-ms-asf",
    "avi": "video/x-msvideo",
    "bin": "application/octet-stream",
    "bmp": "image/bmp",
    "c": "text/plain",
    "class": "application/octet-stream",
    "conf": "text/plain",
    "cpp": "text/plain",
    "doc": "application/msword",
    "docx": "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    "xls": "application/vnd.ms-excel",
    "xlsx": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "exe": "application/octet-stream",
    "gif": "image/gif",
    "gtar": "application/x-gtar",
    "gz": "application/x-gzip",
    "h": "text/plain",
    "htm": "text/html",
    "html": "text/html",
    "jar": "application/java-archive",
    "java": "text/plain",
    "jpeg": "image/jpeg",
    "jpg": "image/jpeg",
    "js": "application/x-javascript",
    "log": "text/plain",
    "m3u": "audio/x-mpegurl",
    "m4a": "audio/mp4a-latm",
    "m4b": "audio/mp4a-latm",
    "m4p": "audio/mp4a-latm",
    "m4u": "video/vnd.mpegurl",
    "m4v": "video/x-m4v",
    "mov": "video/quicktime",
    "mp2": "audio/x-mpeg",
    "mp3": "audio/x-mpeg",
    "mp4": "video/mp4",
    "mpc": "application/vnd.mpohun.certificate",
    "mpe": "video/mpeg",
    "mpeg": "video/mpeg",
    "mpg": "video/mpeg",
    "mpg4": "video/mp4",
    "mpga": "audio/mpeg",
    "msg": "application/vnd.ms-outlook",
    "ogg": "audio/ogg",
    "pdf": "application/pdf",
    "png": "image/png",
    "pps": "application/vnd.ms-powerpoint",
    "ppt": "application/vnd.ms-powerpoint",
    "pptx": "application/vnd.openxmlformats-officedocument.presentationml.presentation",
    "prop": "text/plain",
    "rc": "text/plain",
    "rmvb": "audio/x-pn-realaudio",
    "rtf": "application/rtf",
    "sh": "text/plain",
    "tar": "application/x-tar",
    "tgz": "application/x-compressed",
    "txt": "text/plain",
    "wav": "audio/x-wav",
    "wma": "audio/x-ms-wma",
    "wmv": "audio/x-ms-wmv",
    "wps": "application/vnd.ms-works",
    "xml": "text/plain",
    "z": "application/x-compress",
    "zip": "application/x-zip-compressed",
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
     * maxDeepth 最大深度
     */
    buildNestData(dataList, id, parentId, maxDeepth) {
        maxDeepth || (maxDeepth = 10);
        if (!id) id = "id";
        if (!parentId) parentId = "parentId";
        if (typeof(parentId) == "undefined") parentId = "parentId";
        //递归设置节点的子节点对象
        var getTreeChildrenNodes = function(nodeItem, list) {
            deepth++;
            // console.log( deepth + "process:" + nodeItem.corpno + "-" + nodeItem.corpname)
            nodeItem.children = []; //先赋值清空 ，否则下次出现双份
            if (deepth < maxDeepth) {
                list.forEach(function(item, index) {
                    if ((nodeItem[id] == item[parentId]) && item[parentId]) {
                        nodeItem.children.push(item);
                        getTreeChildrenNodes(item, list);
                    }
                });
            } else {
                console.log("too deep:");
                console.table(nodeItem);
            }
            deepth--;
        }

        //获取所有根节点
        let dataRes = [];
        let deepth = 0;

        //
        dataList.forEach(function(item, index) {
            //item父结点ID为空或根据父结点ID在dataList中找不到父节点
            if (!(item[parentId] && dataList.find(d => d[id] == item[parentId]))) {
                dataRes.push(item)
                    //设置根节点的子节点对象
                getTreeChildrenNodes(item, dataList)
            }
        });
        return dataRes;
    },
    getQueryStringByName: function(name) {
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
        return pattern.replace(SIGN_REGEXP, function($0) {
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
                case 'f':
                    return padding(date.getMilliseconds(), $0.length)
            }
        })
    },
    parseDate: function(dateString, pattern) {
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

    //日期相减得天数
    dateDiff(dstart, dend) {
        var days = new Date(dend).getTime() - new Date(dstart).getTime();
        var day = parseInt(days / (1000 * 60 * 60 * 24));
        return day;
    },

    //判断对象各属性是否相等，只判断两层
    isEqual: function(a, b) {
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
        if (typeof(ordFunc) == "string") {
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
            return -1;
        });
    },
    //使用一个方法返回的值排降序
    sortDesc(arr, ordFunc) {
        var ord = null;
        if (typeof(ordFunc) == "string") {
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
            return -1;
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
        cname += window.location.port; //防止同一端口不同网站cookie相同
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
        var uuid = 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x7 | 0x8)).toString(16);
        });
        return uuid.toUpperCase();
    },

    //判断一个对象是空值或空数组
    // 0不包含在内
    isEmpty(obj) {
        //是空值或未定义或空字符串
        if (obj == null || typeof obj == "undefined" || obj == '') return true;

        //是空数组
        return Array.isArray(obj) && !obj.length;
    },
    //从指定url下载文件 name-期望的文件名
    download({ url, name }) {
        var a = document.createElement("a");
        a.href = url;
        name && (a.download = name);
        a.style.display = "none";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    },
    //https://segmentfault.com/q/1010000014406245
    encodeParams(obj) {
        var params = [];
        Object.keys(obj).forEach(function(key) {
            var value = obj[key];
            // 如果值为undefined我们将其置空
            if (typeof value === "undefined") {
                value = "";
            }
            // 对于需要编码的文本（比如说中文）我们要进行编码
            params.push([key, encodeURIComponent(value)].join("="));
        });
        return params.join("&");
    },

    base64toBlob(base64, type) {
        // 将base64转为Unicode规则编码
        let bstr = atob(base64, type),
            n = bstr.length,
            u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n); // 转换编码后才可以使用charCodeAt 找到Unicode编码
        }
        return new Blob([u8arr], {
            type
        });
    },

    //完整的base64 URL转Blob
    dataURLtoBlob(dataurl) {
        var arr = dataurl.split(','),
            mime = arr[0].match(/:(.*?);/)[1],
            bstr = atob(arr[1]),
            n = bstr.length,
            u8arr = new Uint8Array(n);
        while (n--) {
            u8arr[n] = bstr.charCodeAt(n);
        }
        return new Blob([u8arr], { type: mime });
    },

    jsonToBlob(obj) {
        let json = obj;
        if (typeof obj != "string") {
            json = JSON.stringify(obj);
        }
        return new Blob([json], {
            type: "text/json"
        });
    },
    //构造一个语法糖以方便地使用Promise
    //例：
    /*
    function test() {
      let dfd = deferred();
      var a = new Date().getSeconds();
      console.log("source=" + a);
      if (a % 3 == 0) {
        dfd.resolve(a);
      } else {
        dfd.reject(a);
      }
      return dfd.promise;
    }*/
    deferred() {
        let dfd = {};
        dfd.promise = new Promise((resolve, reject) => {
            dfd.resolve = resolve;
            dfd.reject = reject;
        });
        return dfd;
    },
    getMineType(fileURL) {
        var ext = fileURL.substr(fileURL.lastIndexOf('.') + 1);
        ext = ext.toLowerCase();
        return mineTypes[ext];
    },
    isPhoneNumber(mobile) {
        var tel = /^0\d{2,3}-?\d{7,8}$/;
        var phone = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
        if (mobile.length == 11) { //手机号码
            if (phone.test(mobile)) {
                console.log(mobile);
                return true;
            }
        } else if (mobile.length == 13 && mobile.indexOf("-") != -1) { //电话号码
            if (tel.test(mobile)) {
                console.log(mobile);
                return true;
            }
        }
        return false;
    },
    isEmail(email) {
        var mailReg = /^(\w-*\.*)+@(\w-?)+(\.\w{2,})+$/;
        return mailReg.test(email);
    }

}