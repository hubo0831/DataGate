using DataGate.Com.DB;
using DataGate.Com;
using System.Diagnostics;
using Xunit;
using Newtonsoft.Json;
using System.IO;

namespace DataGate.Tests
{
    /// <summary>
    /// 基础测试类
    /// </summary>
    public class BaseTest
    {
        /// <summary>
        /// 测试数据库表名字段名和前端属性名之间的转换
        /// </summary>
        /// <param name="upper">oracle表的大写表名或字段名</param>
        /// <param name="pascal">原pascal或camal形式的属性名</param>
        [InlineData("PATH_CODE", "PathCode")]
        [InlineData("PARENT_ID", "ParentId")]
        [InlineData("TEST_ORM_FIELD", "TestORMField")]
        [InlineData("ENV_CODE_PAGE", "ENV_CODE_PAGE")]
        [InlineData("W_PROGRESS", "WProgress")]
        [InlineData("PARENT_CODE", "parentCode")]
        [InlineData("PARENT_CODE_DEF", "parent_CodeDef")]
        [InlineData("EMPLOYEE2_ID", "employee2Id")]
        [InlineData("W_TASK.EMPLOYEE2_ID", "WTask.employee2Id")]
        [InlineData("TST_DATA_URL.C_NAME", "TstDataUrl.CName")]
        [InlineData("TST_DATA_URL.C_NAME", "TstDataUrl.cName")]
        [InlineData("ID", "Id")]
        [InlineData("ID", "id")]
        [InlineData("DELEGATE_FILEID", "delegate_fileid")]
        [Theory]
        public void ConvertFieldName(string upper, string pascal)
        {
            UpperNameConverter conv = new UpperNameConverter();
            string f = conv.ToDBName(pascal);
            Assert.Equal(upper, f);
        }

        [InlineData("TEST_ORM_FIELD", "testOrmField")]
        [InlineData("EMPLOYEE2_ID", "employee2Id")]
        [InlineData("W_TASK.EMPLOYEE2_ID", "wTask.employee2Id")]
        [InlineData("W_TASK", "wTask")]
        [InlineData("DELEGATE_FILEID", "delegateFileid")]
        [InlineData("ID", "id")]
        [InlineData("TST_DATA_URL.C_NAME", "tstDataUrl.cName")]
        [Theory]
        public void ConvertDBNameTOPropName(string dbName, string camel)
        {
            UpperNameConverter tran = new UpperNameConverter();
           string camelResult = tran.ToPropName(dbName);
            Assert.Equal(camel, camelResult);
        }

        [Fact]
        public void ConvertJson()
        {
            var obj = new
            {
                PARENT_CODE_DEF = "C001",
                TEST_ORM_FIELD = "F002"
            };

            string result = JsonConvert.SerializeObject(obj);
        }

        [Fact]
        public void TestDir()
        {
            string dir = "..\\..\\TestDir";
            DirectoryInfo di = new DirectoryInfo(dir);
            Debug.WriteLine("DIR RESOLVED=" + di.FullName);
        }

        /// <summary>
        /// 测试Md5值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("123456" + "D7F6B0E19D604EAEB63914A520344EBB")]
        public string MD5Test(string source)
        {
            var str = Encryption.MD5(source);
            Debug.WriteLine("MD5=" + str);
            return str;
        }
    }
}
