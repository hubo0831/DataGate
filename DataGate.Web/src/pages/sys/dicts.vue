<template>
<!-- 系统字典管理 -->
  <el-row :gutter="10">
    <el-col :span="10">
      <div class="dg-toolbar">
        <el-button-group>
          <el-button type="primary" icon="fa fa-file-text-o" v-on:click='doGroupCmd("doAdd")'>新增组</el-button>
          <el-button type="primary" icon="fa fa-edit" v-on:click='doGroupCmd("doEdit")'>编辑组</el-button>
          <el-button type="primary" icon="fa fa-trash-o" v-on:click='doGroupCmd("doDel")'>删除组</el-button>
        </el-button-group>
        <el-button-group>
          <el-button type="primary" icon="fa fa-long-arrow-up" v-on:click='doGroupCmd("doUp")'>上移</el-button>
          <el-button type="primary" icon="fa fa-long-arrow-down" v-on:click='doGroupCmd("doDown")'>下移</el-button>
        </el-button-group>
      </div>
      <edit-grid :task="task" show-index id="groupEdit" :height="fitHeight('#groupEdit')" ref="groupEdit" :metadata="groupMeta" :data="groupFilter"
       @current-change="doGroupCurrentChange"></edit-grid>
    </el-col>
    <el-col :span="14">
      <div class="dg-toolbar">
        <el-button-group>
          <el-button :disabled="!currentGroup" type="primary" icon="fa fa-file-text-o"
           v-on:click='doItemCmd("doAdd")'>新增字典项</el-button>
          <el-button :disabled="!currentGroup" type="primary" icon="fa fa-file-text-o"
           v-on:click='showBatchAdd=true'>批量新增</el-button>
          <el-button type="primary" icon="fa fa-edit" v-on:click='doItemCmd("doEdit")'>编辑</el-button>
          <el-button type="primary" icon="fa fa-trash-o" v-on:click='doItemCmd("doDel")'>删除</el-button>
        </el-button-group>
        <el-button-group>
          <el-button type="primary" icon="fa fa-long-arrow-up" v-on:click='doItemCmd("doUp")'>上移</el-button>
          <el-button type="primary" icon="fa fa-long-arrow-down" v-on:click='doItemCmd("doDown")'>下移</el-button>
        </el-button-group>
         <el-button-group>
          <el-button :disabled="!this.task.changed" type="primary" icon="fa fa-save" v-on:click='doSave()'>保存</el-button>
        </el-button-group>
     </div>
      <edit-grid :task="task" show-index id="itemEdit" :height="fitHeight('#itemEdit')" multi-select ref="itemEdit"
       :metadata="itemMeta" :data="itemFilter"
       @new-row="doNewItemRow"></edit-grid>
  </el-col>
  <el-dialog title="批量新增字典项" :visible.sync="showBatchAdd" width="36%">
    <div style="margin-bottom:10px">请在下面的框内输入编码和名称，同一行用空格隔开编码和名称，每行一个字典项。<br />
    提示：您可以将Excel表格中对应编码和名称的两列复制粘贴进来。</div>
<el-input
  type="textarea"
  :rows="10"
  placeholder="编码 名称 说明(选填)<换行>编码 名称 说明(选填)"
  v-model="batchAddText">
</el-input>
      <div slot="footer" class="dialog-footer">
     <el-button type="primary" @click="batchAdd">确 定</el-button>
   <el-button @click="showBatchAdd = false">取 消</el-button>
  </div>
  </el-dialog>
  </el-row>
</template>
<script>
// import "jquery-slimscroll";
import taskmixin from "../taskmixin.js";
import * as API from "../../api";
import util from "../../common/util";

let groupMeta = [
  {
    name: "code",
    title: "组编码",
    width: 70,
    required: true,
    order: 1
  },
  {
    name: "name",
    title: "组名称",
    required: true,
    order: 2
  },
  {
    name: "isEnabled",
    title: "启用",
    uitype: "CheckBox",
    width: 40,
    order: 3
  }
];

export default {
  mixins: [taskmixin],
  mounted: function() {
    // $("#taskChildenDiv").slimScroll({});
  },
  data: function() {
    return {
      currentGroup: null,
      currentItem: null,
      groupMeta: null,
      itemMeta: null,
      showBatchAdd: false,
      batchAddText: ""
    };
  },
  created() {
    API.META("getsysdict").done(meta => {
      this.task.setMetadata(meta);
      this.groupMeta = this.task.reDefineMetadata(groupMeta);
      this.itemMeta = meta;
    });
    this.loadData();
  },
  computed: {
    groupFilter() {
      return util.sort(this.task.products.filter(p => !p.parentCode), a => a.ord);
    },

    itemFilter() {
      if (!this.currentGroup) return [];
      return util.sort(
        this.task.products.filter(p => p.parentCode == this.currentGroup.code),
        a => a.ord
      );
    }
  },
  methods: {
    loadData() {
      this.task.clearData();
      API.QUERY("getsysdict").done(dict => {
        this.task.products = dict;
      });
    },
    doGroupCmd(cmd) {
      this.$refs.groupEdit[cmd]();
    },
    doItemCmd(cmd) {
      this.$refs.itemEdit[cmd]();
    },
    doGroupCurrentChange(group) {
      this.currentGroup = group;
    },
    //以下是item操作
    doNewItemRow(rowData) {
      rowData.parentId = this.currentGroup.id;
    },
    //批量导入
    batchAdd() {
      this.showBatchAdd = false;
      var codeNames = this.batchAddText.split("\n");
      var maxOrder = this.$refs.itemEdit.getMaxOrder() + 1;
      codeNames.forEach(cn => {
        var cs = cn
          .replace(/\s+/, " ")
          .split(" ")
          .map(c => c.trim());
        if (cs.length < 2) return;
        if (cs.length < 3) {
          cs.push("");
        }
        var item = $.extend(this.task.createProduct(), {
          code: cs[0],
          name: cs[1],
          remark: cs[2],
          parentCode: this.currentGroup.code,
          ord: maxOrder++
        });
        this.task.changeStatus(item, "added");
      });
      this.batchAddText = "";
    },
    doSave() {
      this.$refs.groupEdit
        .submitRow()
        .then(() => this.$refs.itemEdit.submitRow())
        .then(this.save);
    },
    save() {
      API.SUBMIT("savesysdict", this.task.createSaveData())
        .done(() => this.$message.success("保存成功！"))
        .done(this.loadData);
    }
  }
};
</script>
<style scoped lang="scss">
</style>
