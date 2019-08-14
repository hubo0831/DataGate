<template>
  <!-- 文件上传组件 -->
  <div>
    <!-- 文件上传组件(简单展示) -->
    <div :id="id" class="uploader" v-if="options.simple">
      <div class="queueList-Simple">
        <el-row :id="id + 'dndArea'" class="placeholder" style="border:0;margin-top:0;padding:0">
          <el-col :span="24">
            <div v-for="file in fileList" :key="file.id">
              <a href="javascript:;" @click="download(file)">
                <i :class="getThumbnail(file)"></i>
                {{file.name}}
              </a>
              <el-tooltip v-bind:content="getStateIco(file).content" placement="top" effect="light">
                <i v-bind:class="getStateIco(file).ico"></i>
              </el-tooltip>
              <i class="el-icon-info" title="有重复的文件" v-if="file.dup"></i>
              <i class="fa fa-trash-o" style="cursor:pointer" title="删除" @click="removeFile(file)"></i>
              <div
                style="margin:2px 0; background:#409eff; height:2px"
                v-if="file.percentage>0 && file.percentage<1"
                :style="{width:file.percentage * 100 + '%'}"
              ></div>
            </div>
            <span :id="id +'_filePicker'"></span>
            <el-button
              type="primary"
              size="small"
              v-if="!options.auto && getCount('waiting') > 0 && !isInProgress"
              v-on:click="startUpload"
            >开始上传</el-button>
            <el-button
              style="position:relative; height:40px; top:-16px"
              type="primary"
              size="small"
              v-show="getCount('error') > 0 && !isInProgress"
              v-on:click="retry"
            >重试</el-button>
          </el-col>
        </el-row>
      </div>
    </div>
    <!-- 文件上传组件 (表格展示）-->
    <div :id="id" class="uploader" v-else :style="height?{ height:height+'px' }:{}">
      <div class="queueList">
        <div class="allFiles">
          <el-table
            ref="dataGrid"
            v-bind:data="fileList"
            highlight-current-row
            v-on:row-click="handleRowClick"
            v-on:selection-change="handleSelection"
            v-on:cell-mouse-enter="handleCellHover"
            v-on:cell-mouse-leave="handleCellLeave"
            :row-class-name="getRowClass"
            border
            :height="height? height-49:null"
            :show-header="true"
            tooltip-effect="light"
            style="width: 100%;"
          >
            <el-table-column type="selection" width="30px"></el-table-column>
            <el-table-column prop="name" label="文件名称" show-overflow-tooltip>
              <template slot-scope="scope">
                <div
                  v-bind:draggable="draggable"
                  v-on:dragstart="$emit('drag-start', scope.row,$event)"
                  v-on:dragend="$emit('drag-end', scope.row, $event)"
                >
                  <a href="#" v-if="scope.row.oldexists" title="有重复的成果">
                    <i class="el-icon-info"></i>
                  </a>
                  <i v-bind:class="getThumbnail(scope.row)"></i>
                  {{scope.row.name}}
                </div>
                <div style="position:relative;margin-top:2px">
                  <div
                    v-if="scope.row.percentage>0 && scope.row.percentage< 1"
                    :style="{background:'#409eff',width:scope.row.percentage * 100 + '%',height:'2px',position:'absolute',left:0,bottom:0}"
                  ></div>
                </div>
              </template>
            </el-table-column>
            <el-table-column
              prop="size"
              label="大小"
              width="74px"
              align="right"
              :formatter="formatSize"
            ></el-table-column>
            <el-table-column prop="status" width="74px" align="center" label="操作">
              <template slot-scope="scope" style="text-align:center;">
                <el-tooltip
                  v-bind:content="getStateIco(scope.row).content"
                  placement="top"
                  effect="light"
                >
                  <i v-bind:class="getStateIco(scope.row).ico"></i>
                </el-tooltip>
                <a href="javascript:;" @click="download(file)">
                  <i class="el-icon-download" title="下载文件" />
                </a>
                <a href="javascript:;" v-on:click="removeFile(scope.row)">
                  <i class="el-icon-delete" title="删除" />
                </a>
              </template>
            </el-table-column>
          </el-table>
        </div>
        <div style="width:100%;height:3px;background:#ebeef5">
          <div
            v-if="totalPercentage>0 && totalPercentage< 1"
            :style="{background:'#409eff',width:totalPercentage * 100 + '%',height:'100%'}"
          ></div>
        </div>
        <el-row :id="id + 'dndArea'" class="placeholder">
          <el-col :span="11">
            <span :id="id +'_filePicker'"></span>&nbsp;
            <span v-if="fileList.length==0">或拖放到虚框内</span>
            <span v-else>共{{fileList.length}}个文件</span>&nbsp;
          </el-col>
          <el-col :span="13">
            <el-button
              type="primary"
              size="small"
              v-show="selection.length > 0"
              v-on:click="removeFiles"
            >删除</el-button>
            <el-button
              type="primary"
              size="small"
              v-show="getCount('waiting') > 0 && !isInProgress"
              v-on:click="startUpload"
            >开始上传</el-button>
            <el-button
              type="primary"
              size="small"
              v-show="isInProgress"
              v-on:click="stopUpload"
            >暂停上传</el-button>
            <el-button
              type="primary"
              size="small"
              v-show="getCount('error') > 0 && !isInProgress"
              v-on:click="retry"
            >重试</el-button>
          </el-col>
        </el-row>
      </div>
    </div>
  </div>
</template>
<script>
import WebUploader from "webuploader";
import "webuploader/css/webuploader.css";
import "../assets/styles/uploader.css";
import { Util, API } from "../";
import userState from "../userState";
export default {
  props: {
    //原始的文件列表和上传后的文件列表
    fileList: {
      type: Array,
      default() {
        return [];
      }
    },
    //是否允许按住文件名拖动的操作（不是拖动上传操作，那是默认就有的）
    draggable: {
      type: Boolean,
      default: false
    },
    //是否是文件夹上传
    allowFolder: {
      type: Boolean,
      default: false
    },
    //是否在客户端生成MD5以判断去重，如果为false则上传到服务端再去重
    //大文件在IE下这个动作可能很慢
    createMD5: {
      type: Boolean,
      default: false
    },
    height: {
      type: Number,
      default: 0
    },
    //webuploader的各个初始化选项
    //参考：http://fex.baidu.com/webuploader/doc/index.html#WebUploader_Uploader
    uploadOptions: {
      type: Object,
      default() {
        return {};
      }
    }
  },
  // events: {
  //     'files-queued'=> files
  //     'upload-start'=> file;
  //      'upload-finished'=> hasError
  // 'upload-success' => file
  // 'file-removed' =>file
  // 'start-drag' => file
  //"selection-changed" => selectedfiles
  //},
  data() {
    return {
      //为区别不同的上传控件的ID
      id: "",
      selection: [], //勾选的文件集合
      currentRow: null, //clicked row
      draging: false, //是否在拖动文件
      isInProgress: false,
      progress: [], //用以计算总进度的文件数组
      totalSize: 0,
      options: {},
      totalFinishedSize: 0,
      paused: false //在一次上传过程中暂停
    };
  },
  watch: {
    fileList(files) {
      this.checkFileList(files);
    }
  },
  computed: {
    totalPercentage() {
      return (this.totalFinishedSize * 1.0) / (this.totalSize || 1);
    }
  },
  created() {
    if (!this.id) this.id = "u" + Util.guid().substr(8, 8);
    this.checkFileList(this.fileList);
  },
  mounted() {
    // 实例化 http://fex.baidu.com/webuploader/demo.html#
    // just in case. Make sure it's not an other libaray.
    var that = this;
    var chunkSize = 3 * 1024 * 1024,
      // WebUploader实例
      uploader;

    if (!WebUploader.Uploader.support()) {
      alert(
        "Web Uploader 不支持您的浏览器！如果你使用的是IE浏览器，请尝试升级 flash 播放器"
      );
      throw new Error(
        "WebUploader does not support the browser you are using."
      );
    }

    var optionsDefault = {
      pick: {
        id: "#" + that.id + "_filePicker",
        label: that.allowFolder ? "选择文件夹" : "选择文件"
      },
      dnd: "#" + that.id + " .queueList", //指定Drag And Drop拖拽的容器，如果不指定，则不启动。
      paste: document.body,
      compress: false, // 不压缩image, 默认如果是jpeg，文件上传前会压缩一把再上传！
      formData: { guid: "guid" },

      //accept: {
      //    title: '所有文件',
      //    extensions: '*',
      //    mimeTypes: '*/*'
      //},

      // swf文件路径
      swf: "/static/Uploader.swf",

      disableGlobalDnd: true, // 是否禁掉整个页面的拖拽功能，如果不禁用，图片拖进来的时候会默认被浏览器打开。

      chunked: true,
      chunkSize: chunkSize, //分片大小（3M）,
      server: this.appConfig.apiUrl + "/api/dg/u"
      // 以下已经在geobank.js里配置
      //fileNumLimit: 300,
      //fileSizeLimit: 200 * 1024 * 1024,    // 200 M
      //fileSingleSizeLimit: 50 * 1024 * 1024    // 50 M
    };

    $.extend(
      this.options,
      optionsDefault,
      this.appConfig.uploadOptions,
      this.uploadOptions
    );

    if (this.options.fileNumLimit == 1) {
      that.options.pick.multiple = false;
    }
    // 实例化
    uploader = WebUploader.create(that.options);

    // https://segmentfault.com/q/1010000014738847
    // 支持可以上传文件夹
    if (this.allowFolder) {
      setTimeout(function() {
        $("#" + that.id + "_filePicker .webuploader-element-invisible").attr(
          "webkitdirectory",
          ""
        );
      }, 1000);
    }
    window.URL = window.URL || window.webkitURL;

    this.uploader = uploader;

    // 添加“添加文件”的按钮，
    uploader.addButton({
      id: "#" + that.id + "_filePicker2",
      label: "继续添加"
    });
    //文件被加入队列前触发，引发"before-file-queued"的vue事件。
    //当外部传回file.exists = true时，表示外部业务中已有相同文件，不加入队列
    uploader.on("beforeFileQueued", function(file) {
      //console.log('beforeFileQueued', file);
      //  uploader.reset();
      that.$emit("before-file-queued", file);
      if (file.exists) {
        return false;
      }
      //如果只允许上传一个文件，则清除原有文件
      if (that.options.fileNumLimit == 1) {
        that.fileList.splice(0);
        uploader.reset();
        that.testStatus();
      }
      return true;
    });

    // 当文件被加入队列以后触发。
    uploader.on("filesQueued", function(files) {
      if (!(files && files.length)) return; //不知为何，没有文件加入队列也会触发
      //如果只允许上传一个文件，则只加入第一个文件
      //通常是用户拖放文件，直接控制不让用户拖
      if (that.options.fileNumLimit == 1) {
        files = [files[0]];
      }
      $(files).each(function(idx, file) {
        file.percentage = 0;
        file.status = "waiting";
        file.guid = WebUploader.Base.guid(); //上传在后端生成唯一文件块的标识
        //if (!that.IsMultiple) {
        //    that.fileList = [];
        //}
        file.path = file.source.source.webkitRelativePath || "";
        var idx = file.path.lastIndexOf(file.name);
        if (idx >= 1) {
          file.path = file.path.slice(0, idx - 1);
        } else {
          file.path = "";
        }
        that.getDownloadUrl(file);
        that.fileList.push(file);
      });
      that.$emit("files-queued", files);
    });

    //单个文件开始上传前触发，一个文件只会触发一次。
    uploader.on("uploadStart", function(file) {
      //console.log('uploadStart', file);
      file.status = "uploading";
      that.$emit("upload-start", file);
    });

    //单个文件的分块在发送前触发
    uploader.on("uploadBeforeSend", function(object, data, headers) {
      object.file.status = "uploading";
      if (object.chunks == 1) {
        //单文件
        data.guid = "";
      } else {
        data.guid = object.file.guid;
      }
      headers.token = that.userState.token;
    });

    // 文件上传过程中创建进度条实时显示。
    uploader.on("uploadProgress", function(file, percentage) {
      file.percentage = percentage;
      that.testStatus();
    });

    //所有文件上传完成,不能调用webuploader自带的uploadFinished事件
    //因为大文件还有一个单独的POST请求，不在它的考虑范围
    function testFinished() {
      //console.log('uploadFinished');
      //判断是否存在未上传成功的文件 true是  false否(全部成果)
      var error = 0;
      var uploading = 0;
      var finished = 0;
      that.fileList.forEach(function(item, index) {
        if (item.status == "error") {
          error++;
        }
        if (item.status == "waiting" || item.status == "uploading") {
          uploading++;
        }
        if (item.status == "finished") {
          finished++;
        }
      });
      if (!error && that.options.fileNumLimit > 1) {
        // that.uploader.reset();
      }
      that.testStatus();
      //vue注册上传完成后的事件
      if (uploading == 0) {
        that.$emit("upload-finished", error);
      }
    }

    //单个文件上传成功 BankService/Upload
    uploader.on("uploadSuccess", function(file, response) {
      function afterSuccess(data) {
        file.id = data.id;
        file.percentage = 1;
        file.status = "finished";
        that.getDownloadUrl(file);
        that.$emit("upload-success", file);
        testFinished();
      }

      function afterFail(r) {
        file.setStatus("error");
        that.$set(file, "status", "error");
      }

      var chunksTotal = Math.ceil(file.size / chunkSize);
      if (chunksTotal > 1) {
        API.POST(that.uploader.option("server"), {
          fileName: file.name,
          filePath: file.path, //v0.2.4
          guid: file.guid,
          chunk: chunksTotal,
          chunks: chunksTotal
        })
          //分片合并后返回值
          .then(afterSuccess, afterFail);
      } else {
        //response单文件返回值
        afterSuccess(response);
      }
    });

    //单个文件上传失败
    uploader.on("uploadError", function(file, reason) {
      //console.log('uploadError', file);
      file.percentage = 0;
      file.status = "error";
      file.reason = reason;
      that.$emit("upload-error", file);
    });
    uploader.on("error", function(type) {
      if (type == "Q_TYPE_DENIED") {
        this.$message.error("请上传指定格式文件");
      } else if (type == "F_EXCEED_SIZE") {
        this.$message.error(
          "单个文件大小不能超过" +
            WebUploader.formatSize(that.options.fileSingleSizeLimit, 0)
        );
      } else if (type == "Q_EXCEED_NUM_LIMIT") {
        this.$message.error(
          "一次上传文件个数不能超过" + that.options.fileNumLimit
        );
      } else if (type == "Q_EXCEED_SIZE_LIMIT") {
        this.$message.error(
          "一次上传文件总大小不能超过" +
            WebUploader.formatSize(that.options.fileSizeLimit, 0)
        );
      } else {
        this.$message.error("上传出错！请检查后重新上传！错误代码" + type);
      }
    });
  },
  methods: {
    checkFileList(files) {
      if (!files) return;
      var that = this;
      //为初始加入的已上传过的文件附加状态信息
      $(files).each(function(idx, file) {
        if (!file.status) {
          file.percentage = 0; //上传进度百分比
          file.status = ""; //上传状态标记，用于跟踪上传状态
          that.getDownloadUrl(file);
        }
      });
    },
    getDownloadUrl(file) {
      //对于没有上传的文件，生成本地url
      if (file.status == "waiting" && window.URL) {
        //IE9没有URL
        file.url = window.URL.createObjectURL(file.source.source);
        return file;
      } else if (
        file.status == "finished" &&
        file.source &&
        file.source.source &&
        window.URL
      ) {
        //本次上传完的文件，释放生成的本地文件url
        window.URL.revokeObjectURL(file.url);
        return API.getDownloadUrl(file);
      }
      //没有在外部提前生成下载url,则生成本系统的
      else if (!file.url) {
        return API.getDownloadUrl(file);
      }
      return file;
    },

    getCount(status) {
      var cnt = 0;
      for (var i in this.fileList) {
        var file = this.fileList[i];
        if (file.status && file.status == status) {
          cnt++;
        }
      }
      return cnt;
    },
    handleCellHover(row, column, cell) {
      if (this.overRow != row) {
        this.overRow = row;
      }
    },
    handleCellLeave(row, column, cell) {
      this.overRow = null;
    },
    handleRowClick(row, event, column) {
      if (column.type != "selection") {
        this.$refs.dataGrid.clearSelection();
      }
      this.$refs.dataGrid.toggleRowSelection(row);
    },
    handleSelection(sel) {
      this.selection = sel;
      this.$emit("selection-changed", sel);
    },
    formatSize(row, column, cellValue, index) {
      return WebUploader.formatSize(cellValue);
    },
    getRowClass(row, index) {
      if (this.selection.indexOf(row.row) >= 0) {
        return "current-row";
      }
      return "";
    },
    addFiles(files) {
      this.Uploader.addFiles(files);
    },
    //单个删除上传文件
    removeFile(file) {
      var index = this.fileList.indexOf(file);
      this.fileList.splice(index, 1);
      //删除上传组件容器中的文件
      if (file.setStatus) {
        this.uploader.removeFile(file, true);
      }
      this.$emit("file-removed", file);
    },
    //批量删除上传文件
    removeFiles() {
      var tempArr = this.selection.slice(0);
      for (var i = 0; i < tempArr.length; i++) {
        var file = tempArr[i];
        this.removeFile(file);
      }
    },
    //开始计算总进度
    startProgress() {
      var files = this.uploader.getFiles("queued", "progress");
      this.progress = [];
      var ts = 0;
      for (var i in files) {
        this.progress.push(files[i]);
        ts += files[i].size;
      }
      this.totalSize = ts;
    },
    testStatus() {
      this.isInProgress = this.uploader && this.uploader.isInProgress();
      var tf = 0;
      if (this.progress.length >= 2) {
        //当文件数小于2时不显示总进度
        for (var i in this.progress) {
          tf += this.progress[i].size * this.progress[i].percentage;
        }
      }
      this.totalFinishedSize = tf;
    },
    //开始上传
    startUpload() {
      var files = this.uploader.getFiles();
      if (files.length > 0) {
        this.uploader.upload();
      }
      if (!this.paused) {
        this.startProgress();
      } else {
        this.paused = false;
      }
      this.testStatus();
    },
    //暂停上传，可以再次开始
    stopUpload() {
      this.uploader.stop(true);
      for (var i in this.fileList) {
        var file = this.fileList[i];
        if (file.status == "uploading") {
          file.status = "waiting";
        }
      }
      this.paused = true;
      this.isInProgress = false;
    },
    //重新上传fileList中的文件,不管上传成功与否
    reUpload() {
      for (var i in this.fileList) {
        var file = this.fileList[i];
        if (file.setStatus) {
          file.status = "waiting";
          file.setStatus("queued");
        }
      }
      this.uploader.upload();
      this.startProgress();
      this.testStatus();
    },
    retry() {
      for (var i in this.fileList) {
        var file = this.fileList[i];
        if (file.status == "error") {
          file.status = "waiting";
        }
      }
      this.uploader.retry();
      this.startProgress();
      this.testStatus();
    },
    //编程方式选择一个文件
    selectFile(file) {
      this.$refs.dataGrid.clearSelection();
      this.$refs.dataGrid.toggleRowSelection(file);
    },
    //编程方式选择多个文件
    selectFiles(fileIds) {
      this.$refs.dataGrid.clearSelection();
      var files = this.fileList.filter(function(file) {
        return fileIds.indexOf(file.id) >= 0;
      });
      for (var i in files) {
        this.$refs.dataGrid.toggleRowSelection(files[i]);
      }
    },
    //重置uploader控件，但不触发事件
    reset() {
      this.fileList.splice(0, this.fileList.length);
      this.uploader.reset();
      startProgress();
      testStatus();
    },
    download(file) {
      Util.download(file);
    },
    //转义上传文件类型
    getThumbnail(file) {
      if (!file.name) {
        return "fa fa-file";
      }
      var exts = file.name.split(".");
      switch (exts[exts.length - 1].toLowerCase()) {
        case "txt":
        case "text":
          return "fa fa-file-text-o";
        case "pdf":
          return "fa fa-file-pdf-o";
        case "xlsx":
        case "xls":
          return "fa fa-file-excel-o";
        case "docx":
        case "doc":
          return "fa fa-file-word-o";
        case "ppt":
        case "pptx":
          return "fa fa-file-powerpoint-o";
        case "jpg":
        case "jpeg":
        case "bmp":
        case "png":
        case "gif":
          return "fa fa-picture-o";
        case "rar":
        case "zip":
          return "fa fa-file-zip-o";
        case "html":
        case "htm":
          return "fa fa-file-code-o";
        case "avi":
        case "wmv":
        case "mpeg":
        case "mpg":
        case "mp4":
        case "mkv":
        case "flv":
        case "webm":
          return "fa fa-file-video-o";
        case "mp3":
        case "wav":
        case "mid":
          return "fa fa-file-audio-o";
        case "exe":
        case "bat":
        case "dll":
          return "fa fa-window-maximize";
        default:
          return "fa fa-file-o";
      }
    },
    //转义上传文件状态
    getStateIco(row) {
      switch (row.status) {
        default:
          return {};
        case "finished":
          return { ico: "fa fa-check-circle", content: "上传完成" };
        case "error":
          return {
            ico: "fa fa-times-circle",
            content: "上传错误:" + row.reason
          };
        case "uploading":
          return { ico: "fa fa-spinner fa-spin", content: "正在上传" };
        case "waiting":
          return { ico: "fa fa-hourglass-2", content: "等待上传" };
      }
    }
  }
};
</script>