import Vue from 'vue'
import Router from 'vue-router'
import bus from "./bus"
import UserChangePwd from './pages/user/changepwd'
import UserProfile from './pages/user/profile'
import util from './common/util'
import NotFound from "./pages/notfound"
import UnAuth from "./pages/unauth"
Vue.use(Router)
import userState from "./userState"
let sysComps = {};
let preDefinedRoutes = []; //预定义的完整路由信息
let router = new Router({
 // mode: 'history', //history模式需要服务端支持

  // 浏览器前进后退时，保留上次滚动到的位置
  // 貌似因为router封装在一个局部滚动的盒子里面，而没有全局滚动，所以没效果。
  // scrollBehavior(to, from, savedPosition) {
  //   return new Promise((resolve, reject) => {
  //     setTimeout(() => {
  //       resolve(savedPosition||{x:0,y:0})
  //     }, 500)
  //   })
  // }


});


//供项目注入自己的页面,只能在路由生成之前调用
function addPage(url, pathFunc) {
  if (typeof url == "object") {
    preDefinedRoutes.push(url);
  } else {
    url = getComponentPath(url);
    sysComps[url] = pathFunc;
  }
}

//使用此语法可以分块打包以动态加载模块
// sysComps['/index'] = resolve=> require(['./pages/index'], resolve);

//首页
import Home from './pages/index'

addPage('/index', Home);
addPage('/user/changepwd', UserChangePwd);
addPage('/user/profile', UserProfile);


import SysDepts from './pages/sys/depts'
import SysDicts from './pages/sys/dicts'
import SysRoles from './pages/sys/roles'
import SysUers from './pages/sys/users'
import SysLog from './pages/sys/log'
import SysMenus from './pages/sys/menus'

//系统管理
addPage('/sys/depts', SysDepts)
addPage('/sys/dicts', SysDicts)
addPage('/sys/roles', SysRoles)
addPage('/sys/users', SysUers)
addPage('/sys/log', SysLog)
addPage('/sys/menus', SysMenus)

//获取去掉参数后组件的加载路径
function getComponentPath(url) {
  if (!url.startsWith('/')) {
    url = '/' + url;
  }
  url = url.toLowerCase().split('/:')[0];
  url = url.split('/?')[0];
  url = url.split('?')[0];

  if (url == '/') url = '/index'; //默认首页是 ./pages/index.vue
  return url;
}

// function getShortPath(cpath) {
//   cpath = cpath + "/";
//   for (var path in sysComps) {
//     if (cpath.startsWith(path + '/'))
//       return path;
//   }
//   return null;
// }

let routeCreated = false;
let userRoutes = [];
let authedIds = [];
//根据用户菜单配置生成路由信息
//因为Vue-router不支持动态清空路由，所以这里可能会报路由重得添加的错
function createRoutes(menus) {
  authedIds = menus.map(m => m.id);
  if (!routeCreated) {
    router.addRoutes(preDefinedRoutes);
  }

  menus.forEach(menu => {
    menu.url = menu.url || '';
    //如果是绝对路径，则忽略
    if (menu.url.indexOf('//') >= 0) {
      return;
    }

    var cpath = getComponentPath(menu.route || menu.url);

    //解决某些带参数的路由，后台配置成具体的参数而形成的路由
    //比如 /sys/users/123
    if (!sysComps[cpath]) {
      // cpath = getShortPath(cpath);
      // if (!cpath)
      return;
    }

    //已经添加过的路由, 就不再添加，免得报错。
    //因为vue-router不支持清空路由重新添加
    var exists = userRoutes.find(ur => ur.meta.id == menu.id);
    if (exists) {
      return;
    }
    var rtr = {
      path: menu.route || menu.url,
      component: sysComps[cpath],
      name: menu.id,
      props: true, //能通过URL传参给组件的props
      meta: {
        id: menu.id,
        cpath //组件的唯一标识，用于判断路由切换时是否是同一组件同一页面，因为路由规则不同
      }
    }
    userRoutes.push(rtr);
    router.addRoutes([rtr]);
  });
  if (routeCreated) return;
  routeCreated = true;
  router.addRoutes([{
      path: '/unauth/:path?',
      name: '没有权限',
      props: true, //能通过URL传参给组件的props
      component: UnAuth
    },
    //notfound应该加在最后面，否则个个页面都会notfound
    {
      path: '*',
      name: '没有找到资源',
      component: NotFound
    }
  ]);
}

router.beforeEach((to, from, next) => {
  if (!to.meta.id || !userState.token || authedIds.includes(to.meta.id)) {
    from.meta.scrollTop = $('.content-container').scrollTop(); //记录滚动位置
    next();
  } else {
    next('/unauth/' + encodeURIComponent(to.fullPath));
  }
});

export default {
  router,
  addPage,
  createRoutes
}
