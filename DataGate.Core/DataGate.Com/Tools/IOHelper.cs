using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace DataGate.Com
{
    /// <summary>
    /// IO的帮助类
    /// </summary>
    public static class IOHelper
    {
        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(DataTable dt, string fileName)
        {
            FileStream fs = new FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string data = "";

            //写出列名称
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    data += dt.Rows[i][j].ToString();
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }

            sw.Close();
        }


        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static DataTable OpenCSV(string fileName)
        {
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;

            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                aryLine = strLine.Split(',');
                if (IsFirst == true)
                {
                    IsFirst = false;
                    columnCount = aryLine.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(aryLine[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }

            sr.Close();
            return dt;
        }

        /// <summary>
        /// 将流写入文件
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        public static void WriteStreamToFile(Stream stream, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                const int bufferLen = 4096;
                byte[] buffer = new byte[bufferLen];
                int count = 0;
                while ((count = stream.Read(buffer, 0, bufferLen)) > 0)
                {
                    fs.Write(buffer, 0, count);
                }
                fs.Close();
            }
        }

        /// <summary>
        /// 过滤非法文件名字符
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CheckFileName(string fileName)
        {
            fileName = Regex.Replace(fileName, "[|\\/,;:><=\\*\\?]+", "");
            return fileName;
        }

        /// <summary>
        /// 检查有无相同的文件，如果有，则返回一个后面加了(1),(2)...的文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetUniqueFileName(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);

            for (int i = 1; ; i++)
            {
                if (File.Exists(fileName))
                {
                    fileName = String.Format("{0}\\{1}({2}){3}", fi.DirectoryName, fi.Name.Substring(0, fi.Name.LastIndexOf('.')), i, fi.Extension);
                }
                else
                {
                    break;
                }
            }
            return fileName;
        }

        /// <summary>
        /// 返回不带路径、不带扩展名的文件名
        /// </summary>
        /// <param name="fileName">原始文件名</param>
        /// <returns>不带扩展名的文件名</returns>
        public static string GetBaseFileName(string fileName)
        {
            fileName = new FileInfo(fileName).Name;
            string title = fileName.Contains('.') ? fileName.Substring(0, fileName.LastIndexOf('.')) : fileName;
            return title;
        }

        /// <summary>
        /// 复制一个文件夹下所有内容到另一个文件夹
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="copySrcDir">是否连同文件夹一起拷贝</param>
        public static void CopyDirectorysAndFiles(string dest, DirectoryInfo srcdir, bool copySrcDir)
        {
            if (dest.LastIndexOf('\\') != (dest.Length - 1))
            {
                dest += "\\";
            }

            if (copySrcDir)
            {
                dest = dest + srcdir.Name + '\\';
            }
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            FileInfo[] files = srcdir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(dest + file.Name, true);
            }
            DirectoryInfo[] dirs = srcdir.GetDirectories();
            foreach (DirectoryInfo dirInfo in dirs)
            {
                CopyDirectorysAndFiles(dest, dirInfo, true);
            }
        }


        /// <summary>
        /// 将字符串转化成内存流
        /// </summary>
        /// <param name="str">要转的字符串</param>
        /// <returns>内存流</returns>
        public static Stream StrToStream(string str)
        {
            byte[] strBytes = System.Text.Encoding.Default.GetBytes(str ?? "");
            MemoryStream ms = new MemoryStream(strBytes);
            return ms;
        }

        /// <summary>
        /// 将字符串转成文件流
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>指定编码的文件流</returns>
        public static Stream StrToStream(string str, Encoding encoding)
        {
            byte[] strBytes = encoding.GetBytes(str ?? "");
            MemoryStream ms = new MemoryStream(strBytes);
            return ms;
        }

        /// <summary>
        /// 将Stream转换成String
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static String StreamToStr(Stream stream)
        {
            // convert stream to string  
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 流转成字节数组
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static String StreamToStr(Stream stream, Encoding encoding)
        {
            // convert stream to string  
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, encoding);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 字节数组转成流
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];

            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始

            stream.Seek(0, SeekOrigin.Begin);

            return bytes;

        }

        public static Stream BytesToStream(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            return ms;
        }

        /// <summary>
        /// 获取某文件夹全部指定了扩展名的文件名称(不包括扩展名)
        /// <param name="dirName">文件夹名称</param>
        /// <param name="extName">匹配的文件名</param>
        /// </summary>
        public static String[] GetFileBaseNamesList(String dirName, String extName = null)
        {
            List<String> namesList = new List<string>();
            string[] files = extName == null ? Directory.GetFiles(dirName)
                : Directory.GetFiles(dirName, extName);

            return files.Select(f =>
            {
                var name = new FileInfo(f).Name;
                var idx = name.LastIndexOf('.');
                if (idx >= 0) return name.Remove(idx);
                return name;
            }).ToArray();
        }


        #region 临时文件和目录
        static string _tempDir;
        /// <summary>
        /// 设置本系统临时目录
        /// </summary>
        /// <returns></returns>
        public static string GetTempDir()
        {
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                return _tempDir;
            }
            string[] dirs = { @"d:\Temp", @"c:\Temp" };
            foreach (string dir in dirs)
            {
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                        _tempDir = dir;
                        return _tempDir;
                    }
                    catch
                    {

                    }
                }
                else
                {
                    _tempDir = dir;
                    return _tempDir;
                }
            }
            return null;
        }


        /// <summary>
        /// 创建一个随机文件名供Word保存临时文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string CreateTempFileName(string type = "TMP", string ext = ".docx")
        {
            return Path.Combine(GetTempDir(), type + "_" +
                 Guid.NewGuid().ToString("N").Substring(8, 8) + ext);
        }

        /// <summary>
        /// 删除所有本次操作产生的临时文件
        /// 本方法一定要在确信所有操作终止以后调用。
        /// </summary>
        public static void DeleteTempDir()
        {
            try
            {
                Directory.Delete(GetTempDir(), true);
            }
            catch (IOException)
            {
            }
        }
        #endregion

        /// <summary>
        /// 将一个流COPY到另一个流
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void CopyStream(Stream source, Stream dest)
        {
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            while ((count = source.Read(buffer, 0, bufferLen)) > 0)
            {
                dest.Write(buffer, 0, count);
            }
            source.Close();
        }

        private static Dictionary<string, string> mineTypes = new Dictionary<string, string>
        {
            //{后缀名，MIME类型}   
            {".3gp","video/3gpp"},
            {".apk","application/vnd.android.package-archive"},
            {".asf","video/x-ms-asf"},
            {".avi","video/x-msvideo"},
            {".bin","application/octet-stream"},
            {".bmp","image/bmp"},
            {".c","text/plain"},
            {".class","application/octet-stream"},
            {".conf","text/plain"},
            {".cpp","text/plain"},
            {".doc","application/msword"},
            {".docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".xls","application/vnd.ms-excel"},
            {".xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".exe","application/octet-stream"},
            {".gif","image/gif"},
            {".gtar","application/x-gtar"},
            {".gz","application/x-gzip"},
            {".h","text/plain"},
            {".htm","text/html"},
            {".html","text/html"},
            {".jar","application/java-archive"},
            {".java","text/plain"},
            {".jpeg","image/jpeg"},
            {".jpg","image/jpeg"},
            {".js","application/x-javascript"},
            {".log","text/plain"},
            {".m3u","audio/x-mpegurl"},
            {".m4a","audio/mp4a-latm"},
            {".m4b","audio/mp4a-latm"},
            {".m4p","audio/mp4a-latm"},
            {".m4u","video/vnd.mpegurl"},
            {".m4v","video/x-m4v"},
            {".mov","video/quicktime"},
            {".mp2","audio/x-mpeg"},
            {".mp3","audio/x-mpeg"},
            {".mp4","video/mp4"},
            {".mpc","application/vnd.mpohun.certificate"},
            {".mpe","video/mpeg"},
            {".mpeg","video/mpeg"},
            {".mpg","video/mpeg"},
            {".mpg4","video/mp4"},
            {".mpga","audio/mpeg"},
            {".msg","application/vnd.ms-outlook"},
            {".ogg","audio/ogg"},
            {".pdf","application/pdf"},
            {".png","image/png"},
            {".pps","application/vnd.ms-powerpoint"},
            {".ppt","application/vnd.ms-powerpoint"},
            {".pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".prop","text/plain"},
            {".rc","text/plain"},
            {".rmvb","audio/x-pn-realaudio"},
            {".rtf","application/rtf"},
            {".sh","text/plain"},
            {".tar","application/x-tar"},
            {".tgz","application/x-compressed"},
            {".txt","text/plain"},
            {".wav","audio/x-wav"},
            {".wma","audio/x-ms-wma"},
            {".wmv","audio/x-ms-wmv"},
            {".wps","application/vnd.ms-works"},
            {".xml","text/plain"},
            {".z","application/x-compress"},
            {".zip","application/x-zip-compressed"},
            {"","*/*"}
        };


        /// <summary>
        /// 根据文件扩展名获取文件流contenttype类型
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>ContentTyp</returns>
        public static string GetContentType(string fileName)
        {
            var ext = new FileInfo(fileName).Extension?.ToLower();

            if (String.IsNullOrEmpty(ext)) return "application/octet-stream";
            if (mineTypes.ContainsKey(ext))
            {
                return mineTypes[ext];
            }
            return "application/octet-stream";
        }

        /// <summary>
        /// https://www.cnblogs.com/zpx1986/p/5584479.html wang加
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
