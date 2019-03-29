<template>
  <!-- 在表格内编辑时，根据元数据定义渲染编辑控件 -->
  <div>
    <el-input
      v-if="meta.uitype=='TextBox'"
      :maxlength="meta.maxlength"
      v-model="obj[meta.name]"
      size="mini"
      :placeholder="meta.title"
      v-bind="meta.attr"
    ></el-input>
    <el-checkbox
      v-model="obj[meta.name]"
      v-else-if="meta.uitype=='CheckBox'"
      :true-label="'1'"
      :false-label="'0'"
      v-bind="meta.attr"
    ></el-checkbox>
    <el-switch
      v-model="obj[meta.name]"
      v-else-if="meta.uitype=='Switch'"
      active-value="1"
      inactive-value="0"
      v-bind="meta.attr"
    ></el-switch>
    <el-date-picker v-model="obj[meta.name]" v-else-if="meta.uitype=='Date'" v-bind="meta.attr"></el-date-picker>
    <el-date-picker
      v-model="obj[meta.name]"
      type="datetime"
      v-else-if="meta.uitype=='DateTime'"
      v-bind="meta.attr"
    ></el-date-picker>
    <el-select
      v-model="obj[meta.name]"
      v-else-if="meta.uitype=='DropdownList'"
      filterable
      allow-create
      default-first-option
      :placeholder="meta.title"
      v-bind="meta.attr"
    >
      <el-option v-for="sel in meta.items" :key="sel.value" :label="sel.text" :value="sel.value"></el-option>
    </el-select>
    <el-select
      v-model="obj[meta.name]"
      v-else-if="meta.uitype=='List'"
      filterable
      multiple
      allow-create
      default-first-option
      :placeholder="meta.title"
      v-bind="meta.attr"
    >
      <el-option v-for="sel in meta.items" :key="sel.value" :label="sel.text" :value="sel.value"></el-option>
    </el-select>
    <!-- 自定义输入组件 -->
    <component
      v-else
      :is="meta.uitype"
      v-model="obj[meta.name]"
      :meta="meta"
      :obj="obj"
      :in-edit="true"
        v-bind="meta.attr"
  ></component>
  </div>
</template>
<script>
export default {
  props: {
    obj: Object,
    meta: Object
  }
};
</script>
