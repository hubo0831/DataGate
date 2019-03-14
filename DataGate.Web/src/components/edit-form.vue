<template>
  <!-- 用于表单编辑 -->
  <div :id="id" :style="height?{height:height + 'px'}:{}">
    <el-form
      size="mini"
      class="meta-form"
      :model="task.editBuffer"
      v-if="task.editBuffer"
      :label-width="labelWidth + 'px'"
      ref="editForm"
      :readonly="readonly"
      :disabled="disabled || task.selection.length==0"
      :rules="rules"
      status-icon
      inline-message
    >
      <el-form-item
        v-for="item in metadataFilter"
        :key="item.name"
        :prop="item.name"
        :class="{multiValue:item.multiValue}"
      >
        <!-- 控件起始区内容插槽 -->
        <slot :meta="item" :obj="task.editBuffer" name="item-header"></slot>
        <label slot="label" :meta="item">
          <slot name="item-label" :meta="item">{{item.title}}</slot>
        </label>
        <!-- 不能编辑 -->
        <template v-if="readonly || item.readonly">
          <display-item :meta="item" v-model="task.editBuffer[item.name]"></display-item>
        </template>
        <!-- 普通文本 -->
        <el-input
          v-else-if="item.uitype=='TextBox'"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        ></el-input>
        <!-- 多行文本 -->
        <el-input
          v-else-if="item.uitype=='TextArea'"
          type="textarea"
          :rows="3"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        ></el-input>
        <!-- 单项选择 -->
        <el-select
          v-else-if="item.uitype=='DropdownList'"
          v-model="task.editBuffer[item.name]"
          filterable
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        >
          <el-option
            v-for="sel in item.options"
            :key="sel.value"
            :label="sel.text"
            :value="sel.value"
          ></el-option>
        </el-select>
        <!-- 多项选择 ,此项涉及到一对多表关系-->
        <el-select
          v-else-if="item.uitype=='List'"
          v-model="task.editBuffer[item.name]"
          multiple
          filterable
          :value-key="item.valuekey"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        >
          <el-option
            v-for="sel in item.options"
            :key="sel[item.valuekey]"
            :label="sel.text"
            :value="sel.value"
          ></el-option>
        </el-select>
        <!-- 多选框组选择 ,此项涉及到一对多表关系-->
        <el-checkbox-group
          v-else-if="item.uitype=='CheckBoxList'"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        >
          <el-checkbox v-for="sel in item.options" :key="sel.value" :label="sel.text"></el-checkbox>
        </el-checkbox-group>
        <!-- 开关 -->
        <el-switch
          v-else-if="item.uitype=='Switch'"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          active-value="1"
          inactive-value="0"
        ></el-switch>
        <el-checkbox
          v-else-if="item.uitype=='CheckBox'"
          v-model="task.editBuffer[item.name]"
          :true-label="'1'"
          :false-label="'0'"
          v-on:change="handleChange(item)"
        ></el-checkbox>
        <!-- 日期选择 -->
        <el-date-picker
          v-else-if="item.uitype=='Date'"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          type="date"
          :placeholder="getPlaceholder(item)"
        ></el-date-picker>
        <!-- 日期和时间选择 -->
        <el-date-picker
          v-else-if="item.uitype=='DateTime'"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          type="datetime"
          :placeholder="getPlaceholder(item)"
        ></el-date-picker>
        <!-- 普通文本 -->
        <el-input
          v-else-if="item.uitype=='TextBox' || !item.uitype"
          v-model="task.editBuffer[item.name]"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        ></el-input>
        <!-- 自定义输入组件 -->
        <component
          v-else
          :is="item.uitype"
          v-model="task.editBuffer[item.name]"
          :meta="item"
          :in-edit="true"
          :obj="task.editBuffer"
          :task="task"
          v-on:change="handleChange(item)"
          :placeholder="getPlaceholder(item)"
          style="width:100%"
        ></component>
        <!-- 控件结束区内容插槽 -->
        <slot :meta="item" :obj="task.editBuffer" name="item-footer"></slot>
      </el-form-item>
    </el-form>
  </div>
</template>
<script>
import util from "../common/util";
export default {
  props: {
    //editTask对象，要用到其中的
    //metadata(元数据定义)
    // selection(勾选的对象列表)
    // editBuffer(编辑缓冲对象)
    task: {
      type: Object
    },
    //禁用，组件还是输入组件但不能输入
    disabled: {
      type: Boolean,
      default: false
    },
    //不能修改只能看，组件全是display状态
    readonly: {
      type: Boolean,
      default: false
    },
    height: {
      type: Number,
      default: 0
    },
    labelWidth: {
      type: Number,
      default: 120
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
  mounted() {
    $("#" + this.id).slimScroll();
  },
  watch: {
    "task.editBuffer": function() {
      this.$refs.editForm.clearValidate();
    }
  },
  computed: {
    rules() {
      return this.task.rules;
    },
    metadataFilter() {
      //从传入的元数据集合筛选中能在表单中处理的子集
      return util.sort(
        this.task.metadata.filter(m => m.formorder >= 0),
        m => m.formorder
      ); // && (item.uitype!='File' || task.selection.length<=1));
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
      var that = this;
      for (var i in that.task.metadata) {
        var targetItem = that.task.metadata[i];
        //处理联动
        if (targetItem.linkto == item.name) {
          this.task.updateOptions(targetItem);
        }
      }
      this.$emit("change", item);
    },

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
