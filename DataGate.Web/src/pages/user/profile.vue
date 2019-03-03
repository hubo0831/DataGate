<template>
  <el-row>
    <el-col :xs="24" :sm="24" :md="18" :lg="12" :xl="8">
      <el-form ref="form" :model="form" :rules="rules" size="mini" label-width="80px">
        <el-form-item label="账号">
          <el-input v-model="form.account" disabled></el-input>
        </el-form-item>
        <el-form-item prop="name" label="姓名">
          <el-input v-model="form.name" required placeholder="请输入真实姓名"></el-input>
        </el-form-item>
        <el-form-item prop="tel" label="电话">
          <el-input v-model="form.tel" required placeholder="请输入手机或电话"></el-input>
        </el-form-item>
        <el-form-item prop="email" label="邮箱">
          <el-input v-model="form.email" required placeholder="请输入电子邮件地址"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="doSave">提交</el-button>
        </el-form-item>
      </el-form>
    </el-col>
  </el-row>
</template>

<script>
import * as API from "../../api";

export default {
  data() {
    return {
      form: {},
      rules: {
        name: [{ required: true, message: "请输入姓名", trigger: "blur" }],
        tel: [{ required: true, message: "请输入手机或电话", trigger: "blur" }],
        email: [
          { required: true, message: "请输入邮箱", trigger: "blur" },
          {
            type: "email",
            message: "请输入正确的邮箱地址",
            trigger: "blur,change"
          }
        ]
      }
    };
  },
  created() {
    this.form = $.extend({}, this.userState.currentUser);
  },
  methods: {
    doSave() {
      let that = this;
      that.$refs.form.validate(valid => {
        if (valid) {
          let args = {
            name: that.form.name,
            tel: that.form.tel,
            email: that.form.email
          };
          API.POST("/api/check/ChangeProfile", args).done(r => {
            $.extend(this.userState.currentUser, args);
            this.$message.success("用户信息保存成功！");
          });
        }
      });
    }
  }
};
</script>
