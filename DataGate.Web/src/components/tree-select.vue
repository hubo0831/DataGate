<template>
<!-- @*树型选择组件，支持单选和多选*@ -->
<el-popover v-model="popVisible"
            placement="bottom"
            width="445"
            trigger="click">
    <div>
        <el-tree ref="optionsTree"
                 v-bind:data="treeOptions.data"
                 v-bind:props="treeOptions.defaultProps"
                 :node-key="treeOptions.key"
                 :check-strictly="true"
                 clearable
                 highlight-current
                 v-bind:expand-on-click-node="false"
                 accordion
                 :show-checkbox="multiple"
                 v-on:check-change="handleCheckChange"
                 v-bind:default-expand-all="true"
                 v-on:current-change="handleCurrentChange">
            <span slot-scope="scope">
                <i v-bind:class="scope.node.level>1?'fa fa-th':'fa fa-th-large'"></i>
                <span>{{ scope.data[treeOptions.defaultProps.label] }}</span>
            </span>
        </el-tree>
    </div>
    <el-select popper-class="hidden"
               ref="tempSelect"
               slot="reference"
               :multiple="multiple"
               :placeholder="placeholder"
               v-model="tempValue"
               v-on:change="handleSelectChange"
               style="width:100%">
        <el-option v-for="item in tempOptions"
                   :key="item.value"
                   :label="item.label"
                   :value="item.value">
        </el-option>
    </el-select>
</el-popover>
</template>
<script>
export default{
        props: ['value', //表单值，单选时是String，多选时是String[]
        'multiple', //是否多选
        'treeOptions',
        //下拉树控件的选项
        //treeOptions: {
        //    data: [], //树控件绑定的数据
        //    key: 'iiid', //主键
        //    defaultProps: {
        //        children: 'children', //子节点集合属性名
        //        label: 'name'  //树结点的显示值属性名
        //    }
        //},
        'placeholder' //占位符
    ],
    data: function () {
        return {
            tempOptions: [
            ], //select组件的选项集合
            popVisible: false, //控制树型下拉框是否显示 
            tempValue: null,  //和value联动的select组件绑定的值，因不能直接修改value
            handleEvent: true  //是否处理事件，因不确定在代码中修改select的值是否会触发事件
        }
    },
    mounted: function () {
        if (this.tempValue == this.value) {
            return;
        }
        this.tempValue = this.value;
        this.$emit('change', this.tempValue);
        this.restoreTreeNodeState();
    },
    watch: {
        value: function (val) {
            if (this.tempValue == val) {
                return;
            }
            this.tempValue = val;
            this.$emit('change', this.tempValue);
            this.restoreTreeNodeState();
        },
        "treeOptions.data": function (val) {
            this.restoreTreeNodeState();
        }
    },
    methods: {
        //同步value的值和treenode的勾选状态
        restoreTreeNodeState: function () {
            this.handleEvent = false;
            var that = this;
            //在select控件中添加option选项以显示正确的值
            if (this.multiple) {
                this.$refs.optionsTree.setCheckedKeys(this.tempValue);
                var nodes = this.$refs.optionsTree.getCheckedNodes();
                for (var i in nodes) {
                    this.addTempOption(nodes[i]);
                }
            }
            else {
                this.$refs.optionsTree.setCurrentKey(this.tempValue || '');
                //此处需要考子结点搜索，要用递归
                var foundFunc = function (nodes) {
                    if (!nodes) return null;
                    var data = nodes.find(function (d) {
                        return d[that.treeOptions.key] == that.tempValue;
                    });
                    if (!data) {
                        for (var i in nodes) {
                            data = foundFunc(nodes[i].children);
                            if (data) break;
                        }
                    }
                    return data;
                };
                var data = foundFunc(that.treeOptions.data);
                that.addTempOption(data);
            }
            this.handleEvent = true;
        },
        //在select控件中添加option选项以显示正确的值
        addTempOption: function (data) {
            if (!data) return;
            var key = data[this.treeOptions.key];
            var exists = this.tempOptions.find(function (opt) { return opt.value == key; });
            if (!exists) {
                this.tempOptions.push({
                    label: data[this.treeOptions.defaultProps.label],
                    value: key
                });
            }
            return key;
        },
        //在单选时捕捉节点单击事件以确定选中的值
        handleCurrentChange: function (data, node, e) {
            if (!this.handleEvent) return;
            if (this.multiple) return;
            var key = this.addTempOption(data);
            this.tempValue = key;
            this.$emit('input', key);
            this.$emit('change', key);
            this.popVisible = false;
        },
        //在多选时捕捉节点勾选事件以确定选中的值的集合
        handleCheckChange: function (data, checked, indeterminate) {
            if (!this.handleEvent) return;
            var nodes = this.$refs.optionsTree.getCheckedNodes();
            var keys = [];
            for (var i in nodes) {
                keys.push(this.addTempOption(nodes[i]));
            }
            this.tempValue = keys;
            this.$emit('input', keys);
            this.$emit('change', keys);
        },
        handleSelectChange: function () {
            this.$emit('input', this.tempValue);
            this.$emit('change', this.tempValue);
            this.restoreTreeNodeState();
        }
    }
}
</script>