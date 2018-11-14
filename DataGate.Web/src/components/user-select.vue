<template>
  <!-- 用于用户选择的组件 -->
  <el-select v-model="userId" :multiple="multiple" filterable remote reserve-keyword placeholder="请输入账号或姓名"
    :remote-method="remoteMethod" @change="doChange"
    @visible-change="doVisibleChange"
     :loading="loading">
    <el-option v-for="item in usersList" :key="item.id" :label="item.name" :value="item.id">
      <span style="float: left">{{ item.name }}</span>
      <span style="float: right; color: #8492a6; font-size: 12px">{{ item.account }}</span> </el-option>
  </el-select>
</template>
<script>
import * as API from "../api";
import util from "../common/util";
export default {
  props: ["value", "multiple"],
  data() {
    return {
      usersList: [],
      userId: null,
      loading: false
    };
  },
  created() {
    if (this.multiple) {
      userId = [];
    }
    //初次载入不触发watch,因此在此补上
    this.restoreUsers(this.value);
  },
  watch: {
    value(val) {
      if (val !== this.userId)
      this.restoreUsers(val);
    }
  },
  methods: {
    restoreUsers(val) {
      if (!val) {
        this.userId = val;
        return;
      }
      this.queryUsers(val).done(() => (this.userId = val));
    },
    queryUsers(keyword) {
      this.loading = true;
      return API.QUERY("selectusers", {
        keyword
      }).done(result => {
        this.usersList = result.data;
        this.loading = false;
      });
    },
    remoteMethod(keyword) {
      keyword = `%${keyword}%`;
      this.queryUsers(keyword);
    },
    doChange(val) {
      this.userId = val;
      this.$emit("input", val);
      this.$emit("change", val);
    },
    doVisibleChange(visible) {
      if (!visible) return;
      var keyword = "%%";
      var user = this.usersList.find(u => u.id == this.userId);
      if (user) {
        keyword = user.name.substr(0, 1) + "%";
      }
      this.remoteMethod(keyword);
    }
  }
};
</script>
