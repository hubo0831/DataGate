namespace DataGate.App.Files
{
    /// <summary>文件下载结果</summary>
    public class UploadFolder
    {
        /// <summary>ID</summary>
        public int Id { get; set; }
        /// <summary>父文件夹ID</summary>
        public int ParentId { get; set; }
        /// <summary>级别</summary>
        public int Level { get; set; }
        /// <summary>文件夹名称</summary>
        public string Name { get; set; }
        /// <summary>文件夹全名称</summary>
        public string FullName { get; set; }
    }
}