using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Web.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Xml;
using Himall.Core;

namespace Himall.Web.Areas.Web.Controllers
{
    public class InstallerController : Controller
    {
        private IList<string> errorMsgs = null;
        private int usernameMinLength = 3;
        private int usernameMaxLength = 20;
        private string usernameRegex = "[\u4e00-\u9fa5a-zA-Z0-9]+[\u4e00-\u9fa5_a-zA-Z0-9]*";
        private int passwordMaxLength = 16;
        private int passwordMinLength = 6;


        string _dbServer,                       // 数据库服务器
         _dbPort,
         _dbName,                        // 数据库名称
         _dbLoginName,             // 数据库登录名
         _dbPwd,                          // 数据库密码
         _siteName,                     //  网站名称
         _siteAdminName,         //  管理员名称
         _sitePwd,                       //  管理员密码
         _sitePwd2,                       //  管理员密码
         _shopName,                 //  官方自营店名称
         _shopAdminName,     //  官方自营店管理员名称
         _shopPwd,                      // 官方自营店密码
        _shopPwd2;                      // 官方自营店密码

        private bool IsInstalled()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            return null == t || bool.Parse(t);
        }

        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            //string encryptedPassword = Core.Helper.SecureHelper.MD5(password);//一次MD5加密
            //string encryptedWithSaltPassword = Core.Helper.SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            //return encryptedWithSaltPassword;
            return Himall.Core.Helper.SecureHelper.MD5(Himall.Core.Helper.SecureHelper.MD5(password) + salt);
        }

        // GET: Web/Installer
        public ActionResult Agreement()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SaveConfiguration(string dbServer, string dbName, string dbLoginName, string dbPwd, string siteName, string siteAdminName, string sitePwd, string sitePwd2, string shopName, string shopAdminName, string shopPwd, string shopPwd2, int installData)
        {
            if (this.IsInstalled())
            {
                return base.Json(new { success = true, msg = "软件已经安装,不需要重新安装.", status = 0 }, JsonRequestBehavior.AllowGet);
            }
            this._dbServer = dbServer;                      // 数据库服务器
            this._dbPort = "";                              //string dbPort,                          //数据库端口
            this._dbName = dbName;
            this._dbLoginName = dbLoginName;                // 数据库名称
            this._dbPwd = dbPwd;                          // 数据库密码
            this._siteName = siteName;                     //  网站名称
            this._siteAdminName = siteAdminName;         //  管理员名称
            this._sitePwd = sitePwd;                       //  管理员密码
            this._sitePwd2 = sitePwd2;                       //  管理员密码
            this._shopName = shopName;                 //  官方自营店名称
            this._shopAdminName = shopAdminName;     //  官方自营店管理员名称
            this._shopPwd = shopPwd;                      // 官方自营店密码
            this._shopPwd2 = shopPwd2;                      // 官方自营店密码
            string msg = string.Empty;
            // 检查用户信息
            if (!this.ValidateUser(out msg))
            {
                return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
            }
            //创建数据库
            if (!this.CreateDtabase(out msg))
            {
                return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
            }
            //如果还没有运行过安装测试，则先运行安装测试
            if (!this.ExecuteTest())
            {
                return base.Json(new { success = true, errorMsg = "数据库链接信息有误" }, JsonRequestBehavior.AllowGet);
            }
            //检查权限
            if (!this.TestPermission())
            {
                return base.Json(new { success = true, errorMsg = "WEB目录读写权限不够." }, JsonRequestBehavior.AllowGet);
            }
            //执行数据脚本
            if (!this.CreateDataSchema(out msg))
            {
                return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
            }
            //创建管理员账户
            if (!this.CreateAdministrator(out msg))
            {
                return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
            }
            //添加演示数据
            if (installData == 1)
            {
                if (!this.AddDemoData(out msg))
                {
                    return base.Json(new { success = true, errorMsg = "添加演示数据失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                // 添加初始化数据
                if (!this.AddInitData(out msg))
                {
                    return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
                }
                if (!this.UpdateSliderImage(out msg))
                {
                    return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
                }
            }
            // 保存web.config文件
            if (!this.SaveConfig(out msg))
            {
                return base.Json(new { success = true, errorMsg = msg }, JsonRequestBehavior.AllowGet);
            }
            //服务配置
            this.SetServicesConfig(dbName);
            return base.Json(new { success = true, msg = "安装成功", status = 1 }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 设置服务名称
        /// </summary>
        /// <param name="ServiceSuffix"></param>
        private void SetServicesConfig(string ServiceSuffix)
        {
            try
            {
                KeyValueConfigurationElement servicePathKey = WebConfigurationManager.OpenWebConfiguration(base.Request.ApplicationPath).AppSettings.Settings["ServicePath"];
                string serviceMainPath = "系统服务";
                if ((servicePathKey != null) && (servicePathKey.Value != ""))
                {
                    serviceMainPath = servicePathKey.Value;
                }
                string siteMapPath = base.Server.MapPath(base.Request.ApplicationPath);
                serviceMainPath = Path.Combine(siteMapPath, serviceMainPath);
                //根据配置文件里定义的服务路径，设置服务的服务名
                string path = Path.Combine(siteMapPath, "ServicePathSetting.xml");
                if (System.IO.File.Exists(path))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(path);
                    XmlNodeList nodelist = document.SelectNodes("ServiceConfig/Service");
                    string configPath = string.Empty;
                    //循环所有服务
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        string innerText;
                        XmlNode serviceConfigFileNode = nodelist.Item(i);
                        XmlNodeList configfileNodes = serviceConfigFileNode.SelectNodes("configfile");
                        if (configfileNodes.Count > 0)
                        {
                            innerText = configfileNodes.Item(0).InnerText;
                            configPath = Path.Combine(serviceMainPath, innerText);
                            //设置服务名后辍
                            SaveServiceName(configPath, ServiceSuffix);
                        }
                        XmlNodeList connconfigfileNodes = serviceConfigFileNode.SelectNodes("connconfigfile");
                        if (connconfigfileNodes.Count > 0)
                        {
                            innerText = connconfigfileNodes.Item(0).InnerText;
                            configPath = Path.Combine(serviceMainPath, innerText);
                            //设置服务数据库连接
                            SaveServiceMySqlConnection(configPath);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error("设置服务名时出错(SetServicesConfig):" + exception.Message);
            }
        }
        /// <summary>
        /// 保存服务
        /// </summary>
        /// <param name="configPath">配置路径</param>
        /// <param name="serviceSuffix">后缀</param>
        private void SaveServiceName(string configPath, string serviceSuffix)
        {
            string str = configPath;
            if (!string.IsNullOrWhiteSpace(str))
            {
                XmlDocument document = new XmlDocument();
                if (System.IO.File.Exists(str))
                {
                    document.Load(str);
                    XmlNode node = document.SelectSingleNode("Settings/ServiceName");
                    if (node != null)
                    {
                        node.InnerText = serviceSuffix;
                    }
                    node = document.SelectSingleNode("Settings/DisplayName");
                    if (node != null)
                    {
                        node.InnerText = serviceSuffix;
                    }
                    node = document.SelectSingleNode("Settings/Description");
                    if (node != null)
                    {
                        node.InnerText = serviceSuffix;
                    }
                    document.Save(str);
                }
            }

        }
        /// <summary>
        /// 保存mysql数据库链接
        /// </summary>
        /// <param name="configPath"></param>
        private void SaveServiceMySqlConnection(string configPath)
        {
            try
            {
                if (System.IO.File.Exists(configPath))
                {
                    System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(configPath);
                    // 写入数据库连接信息
                    if (configuration.ConnectionStrings.ConnectionStrings["Entities"] != null)
                    {
                        configuration.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = this.GetEFConnectionString();
                    }
                    if (configuration.ConnectionStrings.ConnectionStrings["mysql"] != null)
                    {
                        configuration.ConnectionStrings.ConnectionStrings["mysql"].ConnectionString = this.GetSimpleConnectionString();
                    }
                    if (configuration.AppSettings.Settings["ConnString"] != null)
                    {
                        configuration.AppSettings.Settings["ConnString"].Value = this.GetSimpleConnectionString();
                    }
                    configuration.Save();
                }
            }
            catch (Exception exception)
            {
                Log.Error("设置服务数据库链接异常：" + exception.Message);
            }
        }
        public ActionResult Configuration()
        {
            ViewBag.IsDebug = GetSolutionDebugState();
            return View();
        }

        private bool GetSolutionDebugState()
        {
            #if !DEBUG
                        return false;
            #elif DEBUG
                        return true;
            #endif
        }

        /// <summary>
        /// 执行测试连接
        /// </summary>
        /// <returns></returns>
        #region ExecuteTest
        private bool ExecuteTest()
        {
            this.errorMsgs = new List<string>();
            DbTransaction transaction = null;
            DbConnection connection = null;
            try
            {
                string errorMsg;
                if (this.ValidateConnectionStrings(out errorMsg))
                {
                    using (connection = new MySqlConnection(this.GetConnectionString()))
                    {
                        connection.Open();
                        DbCommand command = connection.CreateCommand();
                        transaction = connection.BeginTransaction();
                        command.Connection = connection;
                        command.Transaction = transaction;
                        // 创建测试表
                        command.CommandText = "CREATE TABLE installTest(Test bit NULL)";
                        command.ExecuteNonQuery();
                        // 删除测试表
                        command.CommandText = "DROP TABLE installTest";
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        connection.Close();
                    }
                }
                else
                {
                    this.errorMsgs.Add(errorMsg);
                }
            }
            catch (Exception exception)
            {
                this.errorMsgs.Add(exception.Message);
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exception2)
                    {
                        this.errorMsgs.Add(exception2.Message);
                        return (this.errorMsgs.Count == 0);
                    }
                }
                if ((connection != null) && (connection.State != ConnectionState.Closed))
                {
                    connection.Close();
                    connection.Dispose();
                }
                return (this.errorMsgs.Count == 0);
            }
            return (this.errorMsgs.Count == 0);
        }
        /// <summary>
        /// 测试目录读写权限
        /// </summary>
        /// <returns></returns>
        private bool TestPermission()
        {
            string testError;
            errorMsgs = new List<string>();
            // 检查config目录的权限
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            //testPath = Request.MapPath( Request.ApplicationPath + "/config/test.txt" );
            //if( !TestFolder( testPath , out errorMsg ) )
            //{
            //    errorMsgs.Add( errorMsg );
            //}

            // 检查web.config文件的修改权限
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(base.Request.ApplicationPath);
                if (configuration.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString == "none")
                {
                    configuration.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = "required";
                }
                else
                {
                    configuration.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = "none";
                }
                configuration.Save();
            }
            catch (Exception exception)
            {
                this.errorMsgs.Add(exception.Message);
            }
            if (!TestFolder(Request.MapPath(base.Request.ApplicationPath + "/storage/test.txt"), out testError))
            {
                this.errorMsgs.Add(testError);
            }
            return (this.errorMsgs.Count == 0);

        }
        #endregion
        private bool ValidateUser(out string msg)
        {
            msg = null;
            //检查帐号信息完整度
            if ((string.IsNullOrEmpty(this._siteAdminName) || string.IsNullOrEmpty(this._sitePwd)) || string.IsNullOrEmpty(this._sitePwd2))
            {
                msg = "管理员账号信息不完整";
                return false;
            }
            if ((string.IsNullOrEmpty(this._shopAdminName) || string.IsNullOrEmpty(this._shopPwd)) || string.IsNullOrEmpty(this._shopPwd2))
            {
                msg = "店铺管理员账号信息不完整";
                return false;
            }
            // 检查用户名长度
            if ((this._siteAdminName.Length > this.usernameMaxLength) || (this._siteAdminName.Length < this.usernameMinLength))
            {
                msg = string.Format("管理员用户名的长度只能在{0}和{1}个字符之间", this.usernameMinLength, this.usernameMaxLength);
                return false;
            }
            // 检查用户名长度
            if ((this._shopAdminName.Length > this.usernameMaxLength) || (this._shopAdminName.Length < this.usernameMinLength))
            {
                msg = string.Format("店铺管理员用户名的长度只能在{0}和{1}个字符之间", this.usernameMinLength, this.usernameMaxLength);
                return false;
            }
            // 检查是否和匿名用户名重复
            if (string.Compare(this._siteAdminName, "anonymous", true) == 0)
            {
                msg = "不能使用anonymous作为管理员用户名";
                return false;
            }
            // 检查是否和匿名用户名重复
            if (string.Compare(this._shopAdminName, "anonymous", true) == 0)
            {
                msg = "不能使用anonymous作为店铺管理员用户名";
                return false;
            }
            // 检查用户名格式
            if (!Regex.IsMatch(this._siteAdminName, this.usernameRegex))
            {
                msg = "管理员用户名的格式不符合要求，用户名一般由字母、数字、下划线和汉字组成，且必须以汉字或字母开头";
                return false;
            }
            // 检查用户名格式
            if (!Regex.IsMatch(this._shopAdminName, this.usernameRegex))
            {
                msg = "店铺管理员用户名的格式不符合要求，用户名一般由字母、数字、下划线和汉字组成，且必须以汉字或字母开头";
                return false;
            }
            // 比较两次密码输入
            if (this._sitePwd != this._sitePwd2)
            {
                msg = "管理员登录密码两次输入不一致";
                return false;
            }
            // 比较店铺两次密码输入
            if (this._shopPwd != this._shopPwd2)
            {
                msg = "店铺管理员登录密码两次输入不一致";
                return false;
            }
            // 检查密码长度
            if ((this._sitePwd.Length < this.passwordMinLength) || (this._sitePwd.Length > this.passwordMaxLength))
            {
                msg = string.Format("管理员登录密码的长度只能在{0}和{1}个字符之间", this.passwordMinLength, this.passwordMaxLength);
                return false;
            }
            // 检查密码长度
            if ((this._shopPwd.Length < this.passwordMinLength) || (this._shopPwd.Length > this.passwordMaxLength))
            {
                msg = string.Format("店铺管理员登录密码的长度只能在{0}和{1}个字符之间", this.passwordMinLength, this.passwordMaxLength);
                return false;
            }
            return true;

        }
        /// <summary>
        /// 检查数据库连接字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ValidateConnectionStrings(out string msg)
        {
            msg = null;

            // 不校验数据库登录密码
            if (
                string.IsNullOrEmpty(_dbServer) ||
                string.IsNullOrEmpty(_dbName) ||
                string.IsNullOrEmpty(_dbLoginName)
                )
            {
                // 数据库地址，数据库名称和数据库用户名是必填项
                msg = "数据库连接信息不完整";
                return false;
            }

            return true;
        }
        /// <summary>
        /// 检查目录权限
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private static bool TestFolder(string folderPath, out string errorMsg)
        {
            try
            {
                // 创建测试文件
                System.IO.File.WriteAllText(folderPath, "Hi");
                // 修改测试文件
                System.IO.File.AppendAllText(folderPath, ",This is a test file.");
                // 删除文件
                System.IO.File.Delete(folderPath);
                errorMsg = null;
                return true;
            }
            catch (Exception exception)
            {
                errorMsg = exception.Message;
                return false;
            }

        }
        /// <summary>
        /// 执行完成
        /// </summary>
        /// <returns></returns>
        public ActionResult Completed()
        {
            return View();
        }
        #region helper
        /// <summary>
        /// 返回数据库连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            int result = 0;
            if (int.TryParse(this._dbPort, out result))
            {
            }
            //return String.Format(
            //    "provider=Sql.Data.SqlClient;server={0};database={1};user id={2};password={3};persistsecurityinfo=True;" ,
            //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );
            return string.Format("server={0};database={1};user id={2};password={3};persistsecurityinfo=True;", new object[] { this._dbServer, this._dbName, this._dbLoginName, this._dbPwd });
            //return String.Format(
            //    "Data Source = {0};Initial Catalog = {1};User Id = {2};Password = {3};" ,
            //    _dbServer , _dbName , _dbLoginName , _dbPwd );

        }
        /// <summary>
        /// 返回EF数据库连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetEFConnectionString()
        {
            int result = 0;
            if (int.TryParse(this._dbPort, out result))
            {
            }
            //return String.Format(
            //    "metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=System.Data.SqlClient;provider connection string=\"data source={0}{1};initial catalog={2};persist security info=True;uid={3};Password={4};MultipleActiveResultSets=True;App=EntityFramework\";" ,
            //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );
            return string.Format("metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=MySql.Data.MySqlClient;provider connection string=\" server={0}{1};user id={2};password={3};persistsecurityinfo=True;database={4}\";", new object[] { this._dbServer, (result == 0) ? "" : ("," + this._dbPort), this._dbLoginName, this._dbPwd, this._dbName });
        }
        /// <summary>
        /// 返回简单的连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetSimpleConnectionString()
        {
             int result = 0;
        if (int.TryParse(this._dbPort, out result))
        {
        }
        //return String.Format(
        //    "metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=System.Data.SqlClient;provider connection string=\"data source={0}{1};initial catalog={2};persist security info=True;uid={3};Password={4};MultipleActiveResultSets=True;App=EntityFramework\";" ,
        //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );
        return string.Format("server={0}{1};user id={2};password={3};persistsecurityinfo=True;database={4};Charset=utf8", new object[] { this._dbServer, (result == 0) ? "" : ("," + this._dbPort), this._dbLoginName, this._dbPwd, this._dbName });

        }

        #endregion
        /// <summary>
        /// 执行创建数据库
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool CreateDtabase(out string msg)
        {
            using (DbConnection connection = new MySqlConnection(string.Format("server={0};user id={1};password={2};persistsecurityinfo=True;", this._dbServer, this._dbLoginName, this._dbPwd)))
            {
                msg = "";
                DbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "CREATE DATABASE " + this._dbName;
                if (this._dbName.IndexOf('.') >= 0)
                {
                    msg = "数据库名不能含有字符\".\"";
                    return false;
                }
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    Log.Error("数据库创建失败:" + exception.Message);
                    msg = "数据库创建失败";
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
            return true;

        }

        #region 创建数据库架构
        /// <summary>
        /// 创建数据库架构
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool CreateDataSchema(out string errorMsg)
        {
            string path = base.Request.MapPath("/SqlScripts/Schema.sql");
            if (!System.IO.File.Exists(path))
            {
                errorMsg = "没有找到数据库架构文件-Schema.sql";
                return false;
            }
            try
            {
                return this.ExecuteScriptFile(path, out errorMsg);
            }
            catch
            {
                errorMsg = "数据架构创建错误";
                return false;
            }
        }
        #endregion

        #region script helper
        /// <summary>
        /// 执行数据库脚本语句
        /// </summary>
        /// <param name="pathToScriptFile"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool ExecuteScriptFile(string pathToScriptFile, out string errorMsg)
        {
            StreamReader reader = null;
            DbConnection connection = null;
            string applicationPath = base.Request.ApplicationPath;
            using (reader = new StreamReader(pathToScriptFile))
            {
                using (connection = new MySqlConnection(this.GetConnectionString()))
                {
                    DbCommand command = connection.CreateCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 360;
                    // 考虑到安装脚本可能比较大，将命令超时时间设为6分钟
                    connection.Open();
                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            string str = NextSqlFromStream(reader);
                            if (!string.IsNullOrEmpty(str))
                            {
                                command.CommandText = str.Replace("$VirsualPath$", applicationPath);
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new Exception(exception.Message);
                        }
                    }
                    connection.Close();
                }
                reader.Close();
            }
            errorMsg = null;
            return true;

        }

        private static string NextSqlFromStream(StreamReader reader)
        {
            StringBuilder builder = new StringBuilder();
            string strA = reader.ReadLine().Trim();
            while (!reader.EndOfStream && (string.Compare(strA, "GO", true, CultureInfo.InvariantCulture) != 0))
            {
                builder.Append(strA + Environment.NewLine);
                strA = reader.ReadLine();
            }
            // 如果最后一句不是GO,添加最后一句
            if (string.Compare(strA, "GO", true, CultureInfo.InvariantCulture) != 0)
            {
                builder.Append(strA + Environment.NewLine);
            }
            return builder.ToString();

        }
        #endregion

        #region 添加演示数据
        /// <summary>
        /// 添加演示数据
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool AddDemoData(out string errorMsg)
        {
            string path = base.Request.MapPath("/SqlScripts/SiteDemo.zh-CN.sql");
            if (!System.IO.File.Exists(path))
            {
                errorMsg = "没有找到演示数据文件-SiteDemo.Sql";
                return false;
            }
            try
            {
                return this.ExecuteScriptFile(path, out errorMsg);
            }
            catch
            {
                errorMsg = "演示数据创建错误";
                return false;
            }

        }
        #endregion

        #region 创建超级管理员
        /// <summary>
        /// 创建超级管理员
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool CreateAdministrator(out string errorMsg)
        {
            DbConnection connection = null;
            DbTransaction transaction = null;
            try
            {
                using (connection = new MySqlConnection(this.GetConnectionString()))
                {
                    connection.Open();
                    DbCommand dbCmd = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    dbCmd.Connection = connection;
                    dbCmd.Transaction = transaction;
                    dbCmd.CommandType = CommandType.Text;
                    string sitePwdSalt = Guid.NewGuid().ToString();
                    string sitePwd = this.GetPasswrodWithTwiceEncode(this._sitePwd, sitePwdSalt);
                    string shopPwdSalt = Guid.NewGuid().ToString();
                    string shopPwd = this.GetPasswrodWithTwiceEncode(this._shopPwd, shopPwdSalt);
                    // 自营店id
                    long shopId = 1;
                    //dbCmd.CommandText =
                    //    "SELECT top 1 Id FROM Himall_Shops";
                    dbCmd.CommandText = "SELECT Id FROM Himall_Shops limit 1";
                    object shopIdObj = dbCmd.ExecuteScalar();
                    if (shopIdObj != null)
                    {
                        shopId = (long)shopIdObj;
                    }

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText = "INSERT INTO Himall_Managers  (shopId, RoleId, UserName, Password, PasswordSalt, CreateDate)VALUES (@shopId, 0, @userName, @Password, @PasswordSalt,@CreateDate )";
                    dbCmd.Parameters.Add(new MySqlParameter("@shopId", shopId));
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", this._shopAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", shopPwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", shopPwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText = "INSERT INTO Himall_Managers  (shopId, RoleId, UserName, Password, PasswordSalt, CreateDate)VALUES (0, 0, @userName, @Password, @PasswordSalt,@CreateDate )";
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", this._siteAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", sitePwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", sitePwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText = "SET FOREIGN_KEY_CHECKS=0;INSERT INTO Himall_Members  (id,UserName, Password, PasswordSalt,TopRegionId,RegionId,OrderNumber,Disabled,Points,Expenditure,CreateDate,LastLoginDate)VALUES (569,@userName, @Password, @PasswordSalt,0,0,0,0,0,0.00,@CreateDate,@LastLoginDate );set foreign_key_checks=1;";
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", this._shopAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", shopPwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", shopPwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.Parameters.Add(new MySqlParameter("@LastLoginDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText = "update Himall_SiteSettings set Value=@SiteName WHERE  `Key`='SiteName'";
                    dbCmd.Parameters.Add(new MySqlParameter("@SiteName", this._siteName));
                    dbCmd.ExecuteNonQuery();
                    transaction.Commit();
                    connection.Close();
                }
                errorMsg = null;
                return true;
            }
            catch (SqlException exception)
            {
                errorMsg = exception.Message;
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exception2)
                    {
                        errorMsg = exception2.Message;
                    }
                }
                if ((connection != null) && (connection.State != ConnectionState.Closed))
                {
                    connection.Close();
                    connection.Dispose();
                }
                return false;
            }

        }
        #endregion

        #region 修改图片
        /// <summary>
        /// 修改图片
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool UpdateSliderImage(out string errorMsg)
        {
            DbConnection connection = null;
            DbTransaction transaction = null;
            try
            {
                using (connection = new MySqlConnection(this.GetConnectionString()))
                {
                    connection.Open();
                    DbCommand command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    string salt = Guid.NewGuid().ToString();
                    string passwrodWithTwiceEncode = this.GetPasswrodWithTwiceEncode(this._sitePwd, salt);
                    string str3 = Guid.NewGuid().ToString();
                    string str4 = this.GetPasswrodWithTwiceEncode(this._shopPwd, str3);
                    command.Parameters.Clear();
                    command.CommandText = "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`>1 and `Id`<8";
                    command.Parameters.Add(new MySqlParameter("@ImageUrl", "../Images/226x288.png"));
                    command.ExecuteScalar();
                    command.Parameters.Clear();
                    command.CommandText = "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=8";
                    command.Parameters.Add(new MySqlParameter("@ImageUrl", "../Images/464x288.png"));
                    command.ExecuteScalar();
                    command.Parameters.Clear();
                    command.CommandText = "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`>8 and `Id`<13";
                    command.Parameters.Add(new MySqlParameter("@ImageUrl", "../Images/226x288.png"));
                    command.ExecuteScalar();
                    command.Parameters.Clear();
                    command.CommandText = "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=1";
                    command.Parameters.Add(new MySqlParameter("@ImageUrl", "../Images/464x288.png"));
                    command.ExecuteScalar();
                    command.Parameters.Clear();
                    command.CommandText = "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=13";
                    command.Parameters.Add(new MySqlParameter("@ImageUrl", "../Images/310x165.png"));
                    command.ExecuteScalar();
                    transaction.Commit();
                    connection.Close();
                }
                errorMsg = null;
                return true;
            }
            catch (SqlException exception)
            {
                errorMsg = exception.Message;
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception exception2)
                    {
                        errorMsg = exception2.Message;
                    }
                }
                if ((connection != null) && (connection.State != ConnectionState.Closed))
                {
                    connection.Close();
                    connection.Dispose();
                }
                return false;
            }

        }
        #endregion

        #region 添加初始化数据
        /// <summary>
        /// 添加初始化数据
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>

        private bool AddInitData(out string errorMsg)
        {
            string sourceDirName = base.Request.MapPath("/SqlScripts/Storage");
            string destDirName = base.Request.MapPath("/Storage");
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch
            {
            }
            string path = base.Request.MapPath("/SqlScripts/SiteInitData.zh-CN.Sql");
            if (!System.IO.File.Exists(path))
            {
                errorMsg = "没有找到初始化数据文件-SiteInitData.Sql";
                return false;
            }
            try
            {
                return this.ExecuteScriptFile(path, out errorMsg);
            }
            catch
            {
                errorMsg = "初始化数据创建错误";
                return false;
            }

        }
        #endregion

        #region 保存web.config
        /// <summary>
        /// 保存web.config
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool SaveConfig(out string errorMsg)
        {
            try
            {
                System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(base.Request.ApplicationPath);
                // 删除安装标识配置节
                configuration.AppSettings.Settings["IsInstalled"].Value = "true";
                //写入当前网址
                System.Web.HttpContext current = System.Web.HttpContext.Current;
                configuration.AppSettings.Settings["CurDomainUrl"].Value = Himall.Web.Framework.CurrentUrlHelper.CurrentUrl();
                // 写入数据库连接信息
                configuration.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = this.GetEFConnectionString();
                configuration.ConnectionStrings.ConnectionStrings["mysql"].ConnectionString = this.GetSimpleConnectionString();
                configuration.Save();
                errorMsg = null;
                return true;
            }
            catch (Exception exception)
            {
                errorMsg = exception.Message;
                return false;
            }
        }

        #endregion
    }
}