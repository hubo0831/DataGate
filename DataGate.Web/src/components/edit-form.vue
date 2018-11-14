<template>
  <!-- 用于表单编辑 -->
  <div>
    <el-form size="mini" class="meta-form" :model="task.editBuffer" v-if="task.editBuffer" label-width="90px" ref="editForm"
      :disabled="disabled || task.selection.length==0" :rules="rules" status-icon inline-message>

      <el-form-item v-for="item in metadataFilter" :key="item.name" :label="item.title" v-if="item.uitype!='File' || task.selection.length<=1"
        :prop="item.name" :class="{multiValue:item.multiValue}">
        <!-- 不能编辑 -->
        <template v-if="item.readonly">
          <display-item :meta="item" v-model="task.editBuffer[item.name]">
          </display-item>
        </template>
        <!-- 普通文本 -->
        <el-input v-else-if="item.uitype=='TextBox'" v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)" style="width:100%">
        </el-input>
        <!-- 多行文本 -->
        <el-input v-else-if="item.uitype=='TextArea'" type="textarea" :rows="3" v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)" :placeholder="getPlaceholder(item)" style="width:100%">
        </el-input>
        <!-- 单项选择 -->
        <el-select v-else-if="item.uitype=='DropdownList'" v-model="task.editBuffer[item.name]" filterable v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)" style="width:100%">
          <el-option v-for="sel in item.options" :key="sel.value" :label="sel.text" :value="sel.value">
          </el-option>
        </el-select>
        <!-- 多项选择 ,此项涉及到一对多表关系-->
        <el-select v-else-if="item.uitype=='List'" v-model="task.editBuffer[item.name]" multiple filterable :value-key="item.valuekey"
          v-on:change="handleChange(item)" :placeholder="getPlaceholder(item)" style="width:100%">
          <el-option v-for="sel in item.options" :key="sel[item.valuekey]" :label="sel.text" :value="sel.value">
          </el-option>
        </el-select>
        <!-- 多选框组选择 ,此项涉及到一对多表关系-->
        <el-checkbox-group v-else-if="item.uitype=='CheckBoxList'" v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)" style="width:100%">
          <el-checkbox v-for="sel in item.options" :key="sel.value" :label="sel.text">
          </el-checkbox>
        </el-checkbox-group>
        <!-- 开关 -->
        <el-switch v-else-if="item.uitype=='Switch'" v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          active-value="1" inactive-value="0">
        </el-switch>
        <el-checkbox v-else-if="item.uitype=='CheckBox'" v-model="task.editBuffer[item.name]" :true-label="'1'"
          :false-label="'0'" v-on:change="handleChange(item)">
        </el-checkbox>
        <!-- 日期选择 -->
        <el-date-picker v-else-if="item.uitype=='Date'" v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          type="date" :placeholder="getPlaceholder(item)">
        </el-date-picker>
        <!-- 日期和时间选择 -->
        <el-date-picker v-else-if="item.uitype=='DateTime'" v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          type="datetime" :placeholder="getPlaceholder(item)">
        </el-date-picker>
        <!-- 单文件上传 -->
        <el-input v-else-if="item.uitype=='File' && task.selection.length==1" v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)" readonly="true" :placeholder="getPlaceholder(item)">
          <el-button slot="append" icon="fa fa-folder-open-o" title="选择文件" v-on:click="openUploaderWindow(item)"></el-button>
        </el-input>
        <!-- 多文件上传 -->
        <div v-else-if="item.uitype=='Files' && task.selection.length==1">
          <file-upload v-on:upload-success="handleuploadSuccess" :id="'muploader' + item.name" :ref="'muploader' + item.name"></file-upload>
        </div>
        <!-- 普通文本 -->
        <el-input v-else-if='item.uitype=="TextBox" || !item.uitype' v-model="task.editBuffer[item.name]" v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)" style="width:100%">
        </el-input>
        <!-- 自定义输入组件 -->
        <component v-else :is="item.uitype" v-model="task.editBuffer[item.name]" :meta="item" :obj="task.editBuffer"
          :task="task" v-on:change="handleChange(item)" :placeholder="getPlaceholder(item)" style="width:100%"></component>
      </el-form-item>
    </el-form>
    <el-dialog title="重新上传文件" top="10px" :modal="false" :visible.sync="uploaderVisible">
      <file-upload v-on:upload-success="handleuploadSuccess" v-on:upload-error="uploaderVisible=true" id="metaUploader"
        :file-list="currentItemFiles" ref="metaUploader" :options="{fileNumLimit:1}"></file-upload>
      <div slot="footer" class="dialog-footer">
        <el-button type="primary" v-on:click="uploaderVisible = false">关闭</el-button>
      </div>
    </el-dialog>
  </div>
</template>
<script>
  export default {
    props: {
      //editTask对象，要用到其中的
      //metadata(元数据定义)
      // selection(勾选的对象列表)
      // editBuffer(编辑缓冲对象)
      task: {
        type: Object
      },
      disabled: {
        type: Boolean
      }
    },
    data() {
      return {
        id: "MD_" + new Date().getTime(), //生成唯一的DOM ID
        uploaderVisible: false,
        currentItem: null, //当前的表单项对应的元数据定义项
        currentItemFiles: [] //当前表单的上传文件集合
      };
    },
    watch: {
      "task.editBuffer": function () {
        this.$refs.editForm.clearValidate();
      }
    },
    created() {},
    mounted() {
      // if (this.height > 0) {
      //   $("#" + this.id).slimScroll({});
      // }
    },
    computed: {
      rules() {
        return this.task.rules;
      },
      metadataFilter() {
        //从传入的元数据集合筛选中能在表单中处理的子集
        return this.task.metadata.filter(m => m.formorder >= 0);
      }
    },
    methods: {
      validate(callback) {
        this.$refs.editForm.validate(valid => {
          if (valid && callback) {
            callback(valid);
          }
        });
      },
      resetFields() {
        this.$refs.editForm.resetFields();
      },
      getPlaceholder(item) {
        if (item.multiValue) return "<<多个>>";
        return item.title || item.name;
      },
      handleChange(item) {
        this.$emit("change", item);
        var that = this;
        for (var i in that.task.metadata) {
          var metaItem = that.task.metadata[i];
          //处理联动
          if (metaItem.linkto == item.name) {
            that.updateOptions(metaItem);
          }
        }
      },
      //复制文件数组，以免对原有对象的files数据改写
      getFiles() {
        var files = [];
        if (this.task.editBuffer.files) {
          for (var i in this.task.editBuffer.files) {
            files.push(this.task.editBuffer.files[i]);
          }
        }
        return files;
      },
      openUploaderWindow(item) {
        //映射到filename字段
        this.uploaderVisible = true;
        this.currentItem = item;
        this.currentItemFiles = this.getFiles();
      },
      handleuploadSuccess(file) {
        this.$message.info("'" + file.name + "'上传成功!");
        this.uploaderVisible = false;
        this.task.editBuffer[this.currentItem.name] = file.name;
        this.task.editBuffer.files = [file];
        this.$emit("change", this.task.editBuffer, this.currentItem);
      }
    }
  };

</script>
<style scoped lang="scss">
  .multiValue input::-webkit-input-placeholder,
  .multiValue textarea::-webkit-input-placeholder {
    color: #409eff;
  }

  .multiValue input:-moz-placeholder,
  .multiValue textarea:-moz-placeholder {
    color: #409eff;
  }

  .multiValue input::-moz-placeholder,
  .multiValue textarea::-moz-placeholder {
    color: #409eff;
  }

  .multiValue input:-ms-input-placeholder,
  .multiValue textarea:-ms-input-placeholder {
    color: #409eff;
  }

  .meta-form .el-form-item__label,
  .el-select-dropdown__item {
    font-size: 14px;
  }

  .meta-form .el-form-item {
    margin-bottom: 5px;
  }

  .el-table--mini {
    line-height: 100%;
  }

</style>
