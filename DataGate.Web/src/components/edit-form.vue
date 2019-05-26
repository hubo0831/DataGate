<template>
  <!-- 用于表单编辑 -->
  <div :id="id" :style="getStyle()">
    <el-form
      size="mini"
      class="meta-form"
      :model="task.editBuffer"
      v-if="task.editBuffer"
      :label-width="labelWidth + 'px'"
      ref="editForm"
      :readonly="readonly"
      :disabled="disabled || task.selection.length==0"
      :rules="task.rules"
      inline-message
    >
      <el-col v-bind="item.col" v-for="item in metadataFilter" :key="item.name">
        <el-form-item :class="{multiValue:item.multiValue}" :prop="item.name">
          <!-- 控件起始区内容插槽 -->
          <slot :meta="item" :obj="task.editBuffer" name="item-header"></slot>
          <label slot="label" :meta="item">
            <slot name="item-label" :meta="item">{{item.title}}</slot>
          </label>
          <!-- Custom类型，用插槽定义输入组件，如果没有定义，则当成普通文本输入框 -->
          <slot
            :name="item.name"
            v-if="item.uitype=='Custom'"
            :task="task"
            :in-edit="readonly || item.readonly"
            :meta="item"
            :obj="task.editBuffer"
            v-bind="item.attr"
          >
            <span v-if="readonly || item.readonly">{{task.editBuffer[item.name]}}</span>
            <el-input
              v-else
              v-model="task.editBuffer[item.name]"
              @change="handleChange(item)"
              :placeholder="getPlaceholder(item)"
              v-bind="item.attr"
            ></el-input>
          </slot>
          <!-- 不能编辑 -->
          <display-item
            v-else-if="readonly || item.readonly"
            :meta="item"
            v-model="task.editBuffer[item.name]"
            v-bind="item.attr"
            :in-form="true"
          ></display-item>
          <!-- 数字输入框 -->
          <el-input
            v-else-if="item.uitype=='TextBox' && item.datatype=='Number'"
            type="number"
            :min="0"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
          ></el-input>
          <!-- 普通文本 -->
          <el-input
            v-else-if="item.uitype=='TextBox'"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            :maxlength="item.maxlength"
            v-bind="item.attr"
          ></el-input>
          <!-- 多行文本 -->
          <el-input
            v-else-if="item.uitype=='TextArea'"
            type="textarea"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            :maxlength="item.maxlength"
            v-bind="item.attr"
          ></el-input>
          <!-- 单项选择 -->
          <el-select
            v-else-if="item.uitype=='DropdownList'"
            v-model="task.editBuffer[item.name]"
            filterable
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
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
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            style="width:100%"
            v-bind="item.attr"
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
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
          >
            <el-checkbox v-for="sel in item.options" :key="sel.value" :label="sel.text"></el-checkbox>
          </el-checkbox-group>
          <!-- 开关 -->
          <el-switch
            v-else-if="item.uitype=='Switch'"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            :active-value="1"
            :inactive-value="0"
            v-bind="item.attr"
          ></el-switch>
          <input
            type="checkbox"
            v-else-if="item.uitype=='CheckBox'"
            v-model="task.editBuffer[item.name]"
            true-value="1"
            false-value="0"
            @change="handleChange(item)"
            v-bind="item.attr"
          >
          <!-- 日期选择 -->
          <el-date-picker
            v-else-if="item.uitype=='Date'"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            type="date"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
          ></el-date-picker>
          <!-- 日期和时间选择 -->
          <el-date-picker
            v-else-if="item.uitype=='DateTime'"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            type="datetime"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
          ></el-date-picker>
          <!-- 普通文本 -->
          <el-input
            v-else-if="item.uitype=='TextBox' || !item.uitype"
            v-model="task.editBuffer[item.name]"
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
            v-bind="item.attr"
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
            :in-form="true"
            @change="handleChange(item)"
            :placeholder="getPlaceholder(item)"
             v-on="$listeners"
            v-bind="item.attr"
          ></component>
          <!-- 控件结束区内容插槽 -->
          <slot :meta="item" :obj="task.editBuffer" name="item-footer" v-bind="item.attr"></slot>
        </el-form-item>
      </el-col>
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
    inline: {
      type: Boolean,
      default: false
    },
    height: {
      type: Number,
      default: 0
    },
    width: {
      type: Number,
      default: 0
    },
    //label显示宽度
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
    //if (this.needScroll()) 
    $("#" + this.id).slimScroll();
  },
  watch: {
    "task.editBuffer": function() {
      this.$refs.editForm.clearValidate();
    }
  },
  computed: {
    metadataFilter() {
      //从传入的元数据集合筛选中能在表单中处理的子集
      return util.sort(
        this.task.metadata.filter(m => m.formorder >= 0),
        m => m.formorder
      ); // && (item.uitype!='File' || task.selection.length<=1));
    }
  },
  methods: {
    getStyle() {
      var style = this.styles || {};
      if (this.height) {
        style.maxHeight = this.height + "px";
      }
      if (this.width) {
        style.width = this.width + "px";
      }
      return style;
    },
    validate(callback) {
      //参数是回调函数时，验证通过直接调回调函数
      if (callback) {
        this.$refs.editForm.validate(valid => {
          if (valid) {
            callback(valid);
          }
        });
      } else {
        //没有回调函数时，返回一个promise
        var dfd = util.deferred();
        this.$refs.editForm.validate(valid => {
          if (valid) {
            dfd.resolve();
          }
        });
        return dfd.promise;
      }
    },
    resetFields() {
      this.$refs.editForm.resetFields();
    },
    getPlaceholder(item) {
      if (item.multiValue) return "<<多个>>";
      if (item.attr && item.attr.placeholder) return item.attr.placeholder;
      return item.title || item.name;
    },
    handleChange(item) {
      var val = this.task.editBuffer[item.name];
      if (item.maxlength && val.length > item.maxlength) {
        this.task.editBuffer[item.name] = val.slice(0, item.maxlength);
      }
      for (var i in this.task.metadata) {
        var targetItem = this.task.metadata[i];
        //处理联动
        if (targetItem.linkto == item.name) {
          this.task.updateOptions(targetItem);
        }
      }
      this.$emit("change", item);
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
  label {
    font-weight: bold;
  }
}

.el-table--mini {
  line-height: 100%;
}
</style>
