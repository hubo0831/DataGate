<template>
 <!-- 用作单纯显示一个字段值的模板 -->
   <div>
        <span v-if="meta.uitype=='CheckBox' || meta.uitype=='Switch'">
            <i class="fa fa-check" aria-hidden="true" v-if="value"></i>
        </span>
        <span v-else-if="meta.datatype=='Date'">
        {{value | formatDate}}
        </span>
        <span v-else-if="meta.datatype=='DateTime'">
        {{value | formatDateTime}}
        </span>
        <!-- 数组类型 -->
        <div v-else-if="(meta.datatype||'').startsWith('[')">
            <slot :meta="meta" :value="value"></slot>
        </div>
        <span v-else>{{ value }}</span>
    </div>
</template>
<script>
export default {
  props:["meta", "value"],
  methods:{
  }
};
</script>