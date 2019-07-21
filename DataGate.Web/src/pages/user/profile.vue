<template>
  <el-row>
    <el-col :xs="24" :sm="24" :md="18" :lg="12" :xl="8">
      <el-form ref="profileForm" :model="form" :rules="rules" inline-message size="mini" label-width="80px">
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
    //设置用户账号手机邮箱自定义检验规则，validateFunc的签名是(rule, value, callback)
    //验证不通过时是callback(new Error('错误信息'));通过时空的callback()
    const validateAccountTelEmail = (rule, value, callback) => {
      value = value || "";
      API.QUERY("CheckUser", {
        key: value.toLowerCase(),
        id: this.userState.currentUser.id
      }).then(r => {
        if (r.cnt > 0) {
          callback(new Error(`${rule.title}已注册，请换一个名称`));
        } else {
          callback();
        }
      });
    };

    return {
      form: {},
      rules: {
        name: [{ required: true, message: "请输入姓名", trigger: "blur" }],
        tel: [
          { required: true, message: "请输入手机或电话", trigger: "blur" },
          {
            validator: validateAccountTelEmail,
            trigger: "blur",
            title: "手机"
          }
        ],
        email: [
          { required: true, message: "请输入邮箱", trigger: "blur" },
          {
            type: "email",
            message: "请输入正确的邮箱地址",
            trigger: "blur,change"
          },
          {
            validator: validateAccountTelEmail,
            trigger: "blur",
            title: "邮箱"
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
      let { form } = this;
      this.$refs.profileForm.validate(valid => {
        if (valid) {
          let args = {
            name: form.name,
            tel: form.tel,
            email: form.email
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
