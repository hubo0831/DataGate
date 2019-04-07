import * as API from '../api'
import util from "../common/util"
import editTask from "../common/editTask";

import pubmixin from "../pages/pubmixin"
//公共数据操作类混入对象,用于数据增删改类的顶层页面
export default {
  mixins: [pubmixin],
  created() {
    this.createQuery();
  },
  provide() {
    return {
      urlQuery: this.urlQuery
    }
  },
  beforeRouteLeave: function (to, from, next) {
    var ok = !this.task.changed || window.confirm('有修改尚未保存，是否放弃修改？');
    if (ok) {
      next();
    } else {
      next(false);
    }
  },
  data: function () {
    return {
      task: new editTask(), //核心数据对象
      total: 0, //记录总数，分页时有用
      urlQuery: {} //url传递的查询参数
    };
  },
  watch: {
    //解决翻页时路由改变不刷新的问题
    // 利用watch方法检测路由变化：
    //有一个大坑：当路由组件keep-alive时，所有打开过的页面都会触发这个事件
    '$route': function (to, from) {
      this.createQuery();
      this.loadData();
    }
  },
  methods: {
    createQuery() {
      for (var i in this.$route.query) {
        this.urlQuery[i] = this.$route.query[i];
      }
    },
    loadData() {
      //由子类实现
    },
    //根据url的参数进行查询， 通常用于子类的loadData方法中加载数据
    apiUrlPageQuery(key) {
      if (!this.urlQuery.pageSize) {
        this.urlQuery.pagesize = util.getCookie("pageSize");
      }
      return API.QUERY(key, this.urlQuery)
        .then(result => this.apiDataFilter(key, result))
        .done(result => {
          this.task.clearData(result.total > result.data.length);
          this.total = result.total;
          this.task.products = result.data;
        });
    },
    //不分页的查询， 通常用于子类的loadData方法中加载数据
    apiUrlQuery(key) {
      return API.QUERY(key, this.urlQuery)
        .then(result => this.apiDataFilter(key, result))
        .done(result => {
          this.task.clearData();
          this.task.products = result;
        });
    },
    //由子类实现,获取数据后的数据清洗
    apiDataFilter(key, data) {
      return data;
    },
    apiDataSaving(saveData) {
      //保存前对保存的数据的操作 v0.3.2+
    },
    apiSubmit(saveKey, successTips) {
      //this.$message.success(tips);
      var saveData = this.task.createSaveData();
      this.apiDataSaving(saveData);
      return API.SUBMIT(saveKey, saveData)
        .done(() => this.$message.success(successTips))
        .done(this.loadData);
    }
  }
}
