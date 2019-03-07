<template>
  <div v-on:keyup.enter="search">
    <!-- @*筛选条件组件*@ -->
    <el-form :inline="true">
      <el-form-item v-for="meta in metaFilter" :key="meta.name" :label="meta.title">
        <div class="search-input">
        <template v-if="meta.uitype=='List'">
          <el-select
            v-model="meta.value"
            multiple
            filterable
            allow-create
            default-first-option
            style="width:100%"
            :placeholder="meta.title"
          >
            <el-option
              v-for="sel in meta.options"
              :key="sel.value"
              :label="sel.text"
              :value="sel.value"
            ></el-option>
          </el-select>
        </template>
        <template v-else-if="meta.uitype=='DropdownList'">
          <el-select
            v-model="meta.value"
            clearable
            filterable
            allow-create
            default-first-option
            :placeholder="meta.title"
          >
            <el-option
              v-for="sel in meta.options"
              :key="sel.value"
              :label="sel.text"
              :value="sel.value"
            ></el-option>
          </el-select>
        </template>
        <el-checkbox
          v-else-if="meta.uitype=='CheckBox'"
          v-model="meta.value"
          :true-label="'1'"
          :indeterminate="meta.value !=1 && meta.value!=0"
          :false-label="'0'"
        ></el-checkbox>
        <template v-else-if="meta.uitype=='Date'|| meta.uitype=='DateTime'">
          <div v-if="meta.operator=='bt'">
            <span>
              <el-date-picker v-model="meta.value" type="date" clearable placeholder="起始日期"></el-date-picker>
            </span> ~
            <span>
              <el-date-picker v-model="meta.value1" type="date" clearable placeholder="结束日期"></el-date-picker>
            </span>
          </div>
          <span v-else>
            <el-date-picker v-model="meta.value" type="date" clearable :placeholder="meta.title"></el-date-picker>
          </span>
        </template>
        <template v-else-if="meta.uitype=='TextBox'">
          <el-input v-model="meta.value" clearable :placeholder="meta.title"></el-input>
        </template>
        <template v-else-if="meta.uitype=='TextArea'">
          <el-input v-model="meta.value" clearable :placeholder="meta.title"></el-input>
        </template>
        <!-- Custom自定义组件暂时用文本框 -->
        <template v-else-if="meta.uitype=='Custom'">
          <el-input v-model="meta.value" clearable :placeholder="meta.title"></el-input>
        </template>
        <!-- 自定义输入组件 -->
        <component
          v-else-if="meta.uitype"
          :is="meta.uitype"
          v-model="meta.value"
          :meta="meta"
          :obj="meta"
          :in-edit="true"
          :placeholder="meta.title"
        ></component>
        <!-- 没有明确定义的组件 -->
        <template v-else>
          <el-input v-model="meta.value" clearable :placeholder="meta.title"></el-input>
        </template>
        </div>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" v-on:click.native.prevent="search">查询</el-button>
        <el-button type="primary" v-on:click="reset">重置</el-button>
        <slot></slot>
      </el-form-item>
    </el-form>
  </div>
</template>
<script>
import util from "../common/util.js";
import editTask from "../common/editTask.js";
var task = new editTask();
export default {
  props: {
    //传入的待搜索的元数据定义
    metadata: {
      type: Array,
      default: function() {
        return [];
      }
    }
  },
  inject: ["urlQuery"],
  computed: {
    metaFilter() {
      //按照原order顺序排序，如果order是负数则取绝对值
      //因为原不显示出来的字段可能也要参加搜索
      return util.sort(this.metadata, m => Math.abs(m.order));
    }
  },
  watch: {
    metadata(val) {
      val.forEach(meta => {
        if (!meta.operator) {
          meta.operator = this.getOperators(meta)[0].value;
        }
        if ((meta.uitype || "").indexOf("List") >= 0) task.updateOptions(meta);
        this.$set(meta, "value1", null);
      });
     this.restoreFormValue();
    }
  },

  methods: {
    restoreFormValue() {
      var query = this.urlQuery._filter;
      if (query) query = JSON.parse(query);
      if (!query) return;
      for (var i in query) {
        var meta = this.metaFilter.find(f => f.name == query[i].n);
        if (!meta) continue;
        meta.value = query[i].v;
        if (query[i].v1) meta.value1 = query[i].v1;
      }
    },
    //计算比较表达式运算符
    getOperators: function(meta) {
      switch (meta.uitype) {
        case "Date":
        case "DateTime":
          return [
            {
              text: "介于",
              value: "bt"
            },
            {
              text: "等于",
              value: "de"
            },
            {
              text: "早于",
              value: "lte"
            },
            {
              text: "晚于",
              value: "gte"
            }
          ];
        case "List":
          return [
            {
              text: "包含",
              value: "in"
            },
            {
              text: "不包含",
              value: "nin"
            }
          ];
        default:
          return [
            {
              text: "包含",
              value: "i"
            },
            {
              text: "不包含",
              value: "ni"
            },
            {
              text: "等于",
              value: "e"
            },
            {
              text: "为空",
              value: "n"
            },
            {
              text: "不为空",
              value: "nn"
            }
          ];
      }
    },
    //TODO：数据检验
    validate: function(callback) {
      return callback(true);
    },
    search: function() {
      var filter = [];
      this.metadata.forEach(meta => {
        if (!(meta.value || meta.operator == "n" || meta.operator == "nn")) {
          return;
        }
        var wp = {
          o: meta.operator,
          n: meta.name,
          v: meta.value
        };
        if (meta.value1) wp.v1 = meta.value1;
        filter.push(wp);
      });
      // filter = filter
      //   .map(f => {
      //     var str = `${f.n}|${f.o}|${f.v}`;
      //     if (f.v1) str += "|" + f.v1;
      //     return str;
      //   })
      //   .join("`");
      this.validate(valid => {
        if (valid && this.$emitPass("search", filter).passed) {
          if (filter.length) {
            var f = JSON.stringify(filter);
            if (f.length >= 2000) {
              this.$message.error("您输入的查询条件过长，请适当减少一些条件。");
              return;
            }
            this.urlQuery._filter = f;
          } else {
            this.$delete(this.urlQuery, "_filter");
          }
          this.$router.replace({
            path: this.$route.path,
            query: this.urlQuery
          });
        }
      });
    },
    reset: function() {
      this.metadata.forEach(meta => {
        meta.value = null;
        meta.value1 = null;
      });
      this.search();
    }
  }
};
</script>
<style scoped lang="scss">
.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 140px;
}
.el-form-item {
  margin-bottom: 10px;
}
.search-input {
  max-width: 140px;
}
</style>