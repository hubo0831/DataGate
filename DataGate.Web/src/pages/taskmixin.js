import * as API from '../api'
import util from "../common/util"
import editTask from "../common/editTask";

import pubmixin from "../pages/pubmixin"
//公共数据操作类混入对象,用于数据增删改类的顶层页面
export default {
  mixins: [pubmixin],
  created: function () {
    for (var i in this.$route.query) {
      this.urlQuery[i] = this.$route.query[i];
    }
  },
  provide() {
    return {
      urlQuery: this.urlQuery
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
    '$route': "loadData"
  },
  methods: {
    loadData() {
      //由子类实现
    },
    //根据url的参数进行查询
    apiUrlPageQuery(key) {
      return API.QUERY(key, this.urlQuery).done(result => {
        this.task.clearData();
        this.total = result.total;
        this.task.products = result.data;
      });
    },
    apiSubmit(saveKey, successTips) {
      //this.$message.success(tips);
      var saveData = this.task.createSaveData();
      return API.SUBMIT(saveKey, saveData)
        .done(() => this.$message.success(successTips))
        .done(this.loadData);
    }
  }
}
