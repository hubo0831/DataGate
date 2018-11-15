using DataGate.Api;
using DataGate.App;
using DataGate.App.DataService;
using DataGate.App.Models;
using DataGate.Com.DB;
using DataGate.Com;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace DataGate.Tests
{
    public class AppDataTest : BaseTest
    {
        protected TestServer _testServer;
        protected DBHelper _db;
        public AppDataTest()
        {
            Consts.IsTesting = true;
            _testServer = new TestServer(WebHost.CreateDefaultBuilder().UseStartup<Startup>());
            _db = Get<DBHelper>();

        }
        protected async Task<HttpClient> CreateClientAsync()
        {
            var client = _testServer.CreateClient();
            var result = await PostLoginTest("system", "123456");
            client.DefaultRequestHeaders.Add("token", result.Token);
            return client;
        }

        /// <summary>
        /// 根据类型返回实例的泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return (T)_testServer.Host.Services.GetService(typeof(T));
        }

        /// <summary>
        /// 根据类型返回实例的非泛型方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            return _testServer.Host.Services.GetService(type);
        }

        /// <summary>
        /// 测试获取所有菜单
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetMenus()
        {
            var menuMan = Get<MenuMan>();
            var menus = await menuMan.GetAllAsync();
            Assert.NotNull(menus);
        }

        /// <summary>
        /// 测试登录 
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        [Theory]
        [InlineData("System", "123456")]
        public async Task<LoginResult> PostLoginTest(string userName, string password)
        {
            var client = _testServer.CreateClient();
            HttpResponseMessage response = await client.PostAsync("/api/check/login",
              new FormUrlEncodedContent(new Dictionary<string, string>(){
                  { "Account", userName },
                  {"Password",password }}));
            var result = await response.Content.ReadAsAsync<LoginResult>();
            Assert.Equal(0, result.Code);
            return result;
        }

        /// <summary>
        /// 测试类型是否生成单例
        /// </summary>
        /// <param name="serviceType">类型</param>
        /// <param name="isSingleton">期望是否是单例</param>
        [Theory]
        [InlineData(typeof(SessionProvider), true)]
        [InlineData(typeof(AppUser), false)]
        public void TestSingleton(Type serviceType, bool isSingleton)
        {
            var s1 = Get(serviceType);
            var s2 = Get(serviceType);
            if (isSingleton)
                Assert.True(s1 == s2);
            else
                Assert.False(s1 == s2);
        }

        [Theory]
        [InlineData("System", "123456")]
        async Task InnerLoginTest(string userName, string password)
        {
            var sessionProivder = Get<SessionProvider>();
            var loginRequest = new LoginRequest { Account = userName, Password = password };
            var loginResult = await sessionProivder.Login(loginRequest);
            Assert.NotNull(loginResult.Token);
        }

        /// <summary>
        /// 测试sql语句
        /// </summary>
        /// <param name="sql">带参数的sql语句，参数名统一用@打头</param>
        /// <param name="ps">参数列表，数组0，2，4..项是key, 1,3,5...项是值</param>
        /// <returns></returns>
        [Theory]
        [InlineData("select * from app_user where ACCOUNT=@ACCOUNT", new object[] { "Account", "system" })]
        public async Task SqlQueryTest(string sql, object[] ps)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < ps.Length; i += 2)
            {
                dict[ps[i].ToString()] = ps[i + 1];
            }
            var parameters = _db.GetParameter(dict);
            DataTable dt = await _db.ExecDataTableAsync("select * from app_user where ACCOUNT=@ACCOUNT", parameters.ToArray());
            Assert.True(dt.Rows.Count > 0);
        }

        [Fact]
        void TestDataReader()
        {
            int i = 0;
            IDataReader reader = _db.ExecReader("select * from app_user where ACCOUNT=@ACCOUNT", _db.CreateParameter("account", "system"));
            using (reader)
            {
                while (reader.Read())
                {
                    i++;
                }
            }
            Assert.True(i == 1);
        }

        [Fact]
        public async Task TestDataReaderAsync()
        {
            int i = 0;
            IDataReader reader = await _db.ExecReaderAsync("select * from app_user where ACCOUNT=@ACCOUNT", _db.CreateParameter("ACCOUNT", "system"));
            using (reader)
            {
                while (reader.Read())
                {
                    i++;
                }
            }
            Assert.True(i == 1);
        }

        /// <summary>
        /// 将object参数序列化到url的查询字符串中
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string SerialzeToUrl(object o)
        {
            if (o == null) return null;
            return "?" + string.Join("&", o.GetType().GetProperties()
                .Select(pi => $"{pi.Name}={HttpUtility.UrlEncode(CommOp.ToStr(pi.GetValue(o, null)))}"));
        }

        /// <summary>
        /// 执行指定Key的查询
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        [InlineData("GetSysDict")]
        [InlineData("GetAllRoles")]
        [InlineData("GetAllRoleMenus")]
        [InlineData("GetAuthMenus")]
        [Theory]
        public async Task<JArray> Query(string key, object parm = null)
        {
            var client = _testServer.CreateClient();
            string sql = $"/api/dg/{key}{SerialzeToUrl(parm)}";
            HttpResponseMessage response = await client.GetAsync(sql);
            var result = await response.Content.ReadAsAsync<JArray>();
            string resultStr = result.ToString();
            Debug.WriteLine("QUERY-RESULT-STRING=" + resultStr);
            Debug.WriteLine("QUERY-RESULT-COUNT=" + result.Count);
            return result;
        }

        [Theory]
        [InlineData("00000000000000000000000000000001")]
        public async Task TestGetUserMenus(string userId)
        {
            var array = await Query("GetUserMenuIds", new { userId });
            Assert.True(array.Count > 0);
        }

        /// <summary>
        /// 执行指定Key的分页查询
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parm">传pageIndex,pageSize的参数</param>
        /// <returns></returns>
        protected async Task<JObject> PageQuery(string key, object parm = null)
        {
            var client = _testServer.CreateClient();
            string sql = $"/api/dg/{key}{SerialzeToUrl(parm)}";
            HttpResponseMessage response = await client.GetAsync(sql);
            var result = await response.Content.ReadAsAsync<JObject>();
            Debug.WriteLine("QUERY-RESULT-COUNT=" + result.Count);
            return result;
        }

        /// <summary>
        /// 查询指定的key值对应的元数据定义
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Theory]
        [InlineData("GetSysDict")]
        [InlineData("test")]
        public async Task GetMetadata(string key)
        {
            var client = _testServer.CreateClient();

            HttpResponseMessage response = await client.GetAsync($"/api/dg/m/{key}");
            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("Metadata-RESULT=" + result);
        }

        [Fact]
        void TestDataRowSerialize()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            dt.Rows.Add("obj1", "obj2");

            string result = JsonConvert.SerializeObject(dt);

            Assert.True(result != null);
        }

        [InlineData(5)]
        [Theory]
        async Task<string[]> TestInsertMany(int count)
        {
            Random rand = new Random();
            var objs = Enumerable.Range(0, count).Select(a => new
            {
                id = "",
                name = $"SUPER Man -{DateTime.Now.Ticks}-{a}",
                createDate = DateTime.Now,
                updateDate = DateTime.Now,
                isDeleted = a % 2,
                userName = "supermaster",
                num = rand.NextDouble() * 10000
            });
            var client = _testServer.CreateClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/dg/s/test", new
            {
                added = objs,
                changed = new string[0],
                removedKeys = new string[0]
            });
            var result = await response.Content.ReadAsAsync<string[]>();
            Assert.Equal(objs.Count(), result.Length);
            return result;
        }

        [Fact]
        async Task<JArray> TestInQuery()
        {
            var inserts = await TestInsertMany(3);
            var objIds = JsonConvert.SerializeObject(inserts);
            var client = _testServer.CreateClient();
            string url = "/api/dg/testin?ids=" + WebUtility.UrlEncode(objIds);
            HttpResponseMessage response = await client.GetAsync(url);
            //string resStr = await response.Content.ReadAsStringAsync(); ;
            var result = await response.Content.ReadAsAsync<JArray>();
            Assert.Equal(inserts.Count(), result.Count);
            return result;
        }

        [Fact]
        async Task TestReplaceMany()
        {
            var insertsArray = await TestInQuery();
            foreach (var t in insertsArray)
            {
                t["userName"] = "新的用户";
                t["updateDate"] = new DateTime(1999, 3, 8);
            }
            var client = _testServer.CreateClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/dg/s/test", new
            {
                changed = insertsArray
            });
            var result = await response.Content.ReadAsAsync<string[]>();
            Assert.True(0 == result.Length);
        }

        [Fact]
        async Task TestDeleteMany()
        {
            var insertsArray = await TestInQuery();

            var client = _testServer.CreateClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/dg/s/test", new
            {
                removed = insertsArray.Select(a => new
                {
                    ID = a["id"] //故意ID大写测试
                })
            });
            var result = await response.Content.ReadAsAsync<string[]>();
            Assert.True(0 == result.Length);
        }

        /// <summary>
        /// 测试返回Key对应的指定页的数据
        /// </summary>
        /// <param name="key">数据访问Key值</param>
        /// <param name="pageIndex">页号，1打头</param>
        /// <param name="pageSize">页大小</param>
        /// <returns></returns>
        [Theory]
        [InlineData("GetAllUsers", 1, 10)]
        public async Task TestGetPage(string key, int pageIndex, int pageSize)
        {
            var result = await PageQuery(key, new { pageIndex, pageSize });
            string resultStr = result.ToString();
            Console.WriteLine(resultStr);
            Assert.True(result["data"].Count() <= pageSize);
        }

        [Theory]
        [InlineData("SelectUsers", 1, 20)]
        async Task TestSelectUser(string key, int pageIndex, int pageSize)
        {
            string keyword = "%wang%";
            var result = await PageQuery(key, new { pageIndex, pageSize, keyword });
            string resultStr = result.ToString();
            Console.WriteLine(resultStr);
            Assert.True(result["data"].Count() <= pageSize);
        }

        [Theory]
        [InlineData("GetAllUsers", 0, 10)]
        async Task SearchUserTest(string key, int pageIndex, int pageSize)
        {
            var _filter = new FilterRequest[]{
                new FilterRequest{ Name="account", Operator="i", Value = "USER"},
                new FilterRequest{ Name="createDate", Operator="de", Value = new DateTime(2018,10,14)}
            };
            var result = await PageQuery(key, new
            {
                pageIndex,
                pageSize,
                _filter = JsonConvert.SerializeObject(_filter)
            });
            string resultStr = result.ToString();
            Console.WriteLine(resultStr);
            Assert.True(result["data"].Count() <= pageSize);
        }

        [Fact]
        async Task TestCUDUser()
        {
            var rand = new Random();
            string pk = CommOp.NewId();
            var roles = await Get<DBCrud<AppRole>>().GetListAsync();
            if (roles.Count < 2)
            {
                throw new Exception("要完成本次测试，角色表中至少要有两个角色。");
            }
            var userRole = new
            {
                roleId = roles[0].Id,
                userId = pk
            };

            var user = new
            {
                id = pk,
                name = "测试用户" + rand.Next(),
                account = "TEST_USER" + rand.Next(),
                email = DateTime.Now.Ticks + "@abc.com",
                tel = DateTime.Now.Ticks,
                //这个子表数据应该被忽略掉，在前端json传值时也应该不传
                roles = new object[] { userRole }
            };

            var task = new
            {
                added = new object[] { user },
                details = new object[]
                {
                    new
                    {
                        key="SaveUserRole",
                        added = new object[]{ userRole }
                    }
                }
            };

            var client = _testServer.CreateClient();
            var response = await client.PostAsJsonAsync("/api/dg/s/SaveUser", task);
            var result = await response.Content.ReadAsAsync<string[]>();
            Assert.True(1 == result.Length);

            var user1 = new
            {
                id = pk,
                name = "测试用户U" + rand.Next(),
                account = "USER_" + rand.Next(),
                email = rand.Next() + "@update.com",
                tel = new Random().Next(),
                roles = new object[] { userRole }
            };

            var userRole1 = new
            {
                roleId = roles[1].Id,
                userId = pk
            };

            var task1 = new
            {
                changed = new object[] { user1 },
                details = new object[]
                {
                    new
                    {
                        key="SaveUserRole",
                        removed = new object[]{ userRole },
                        added = new object[]{userRole1}
                    }
                }
            };

            response = await client.PostAsJsonAsync("/api/dg/s/SaveUser", task1);
            result = await response.Content.ReadAsAsync<string[]>();
            Assert.True(0 == result.Length);

            var task2 = new
            {
                removed = new object[] { user1 },
                details = new object[]
                {
                    new
                    {
                        key = "SaveUserRole",
                        removed = new object[] { userRole1 }
                    }
                }
            };

            response = await client.PostAsJsonAsync("/api/dg/s/SaveUser", task2);
            result = await response.Content.ReadAsAsync<string[]>();
            Assert.True(0 == result.Length);
        }
    }
}
