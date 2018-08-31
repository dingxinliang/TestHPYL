/****************************************************************************
*Copyright (c) 2018 Microsoft All Rights Reserved.
*CLR版本： 4.0.30319.42000
*公司名称：Microsoft
*命名空间：DBUtility
*文件名：  MySqlDatabase
*版本号：  V1.0.0.0
*当前的用户域：QH-20160830FLFX
*创建人：丁新亮
*创建时间：2018/7/15 0:21:09

*描述：
*
*=====================================================================
*修改标记
*修改时间：2018/7/15 0:21:09
*修改人： 
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using Maticsoft.DBUtility;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XL.Util.Attributes;
using Dapper;
using System.Linq.Expressions;
using System.Data;
using DBUtility;
using SQLinq;
using SQLinq.Dapper;

namespace Maticsoft.DBUtility
{
   public class MySqlDatabase
    {

        #region 构造函数
        /// <summary>
        /// 构造方法
        /// </summary>
        public MySqlDatabase()
        {
            connectionString = DbHelperMySQL.connectionString;
        }
        #endregion
        public string connectionString { get; set; }
        protected DbConnection Connection
        {
            get
            {
                DbConnection dbconnection = new MySqlConnection(connectionString);
                dbconnection.Open();
                return dbconnection;
            }
        }
        #region 对象实体 查询
        public T FindEntity<T>(object keyValue) where T : class
        {
            using (var dbConnection = Connection)
            {
                var data = dbConnection.Query<T>("select * from {T表名} where key=@key", new { key = keyValue });
                return data.FirstOrDefault();
            }
        }
        public T FindEntity<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                var query = new SQLinq.SQLinq<T>();
               
                var data = dbConnection.Query<T>(new SQLinq<T>().Where(condition));
                return data.FirstOrDefault();

                 
            }
        }
        /// <summary>
        /// 未实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> IQueryable<T>() where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                return (IQueryable<T>)dbConnection.Query<T>(new SQLinq<T>());
            }
        }
        /// <summary>
        /// 未实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                return (IQueryable<T>)dbConnection.Query<T>(new SQLinq<T>().Where(condition));
            }
        }
        public IEnumerable<T> FindList<T>() where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                return dbConnection.Query<T>(new SQLinq<T>()).ToList();
            }
        }
        public IEnumerable<T> FindList<T>(Func<T, object> keySelector) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                return dbConnection.Query<T>(new SQLinq<T>()).OrderBy(keySelector).ToList();
            }
        }
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                return dbConnection.Query<T>(new SQLinq<T>().Where(condition)).ToList();
            }
        }
        public IEnumerable<T> FindList<T>(string strSql) where T : class
        {
            return FindList<T>(strSql, null);
        }
        public IEnumerable<T> FindList<T>(string strSql, object MySqlParameter) where T : class
        {
            using (var dbConnection = Connection)
            {
                return dbConnection.Query<T>(strSql, MySqlParameter);
            }
        }

        public IEnumerable<T> FindList<T>(string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                string[] _orderField = orderField.Split(',');
                var dataLinq = new SQLinq<T>();
                foreach (string item in _orderField)
                {
                    var parameter = Expression.Parameter(typeof(T), "t");
                    var property = typeof(T).GetProperty(item);
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    Expression<Func<T, object>> orderBy = t => propertyAccess;
                    dataLinq.OrderByExpressions.Add(new SQLinq<T>.OrderByExpression { Ascending = isAsc, Expression = orderBy });
                }
                var dataQuery = dbConnection.Query<T>(dataLinq);
                total = dataQuery.Count();
                var data = dataQuery.Skip<T>(pageSize * (pageIndex - 1)).Take<T>(pageSize).AsQueryable();
                return data.ToList();
            }
        }
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> condition, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class, new()
        {
            using (var dbConnection = Connection)
            {
                string[] _orderField = orderField.Split(',');
                var dataLinq = new SQLinq<T>().Where(condition);
                foreach (string item in _orderField)
                {
                    var parameter = Expression.Parameter(typeof(T), "t");
                    var property = typeof(T).GetProperty(item);
                    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                    Expression<Func<T, object>> orderBy = t => propertyAccess;
                    dataLinq.OrderByExpressions.Add(new SQLinq<T>.OrderByExpression { Ascending = isAsc, Expression = orderBy });
                }
                var dataQuery = dbConnection.Query<T>(dataLinq);
                total = dataQuery.Count();
                var data = dataQuery.Skip<T>(pageSize * (pageIndex - 1)).Take<T>(pageSize).AsQueryable();
                return data.ToList();
            }
        }
        public IEnumerable<T> FindList<T>(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class
        {
            return FindList<T>(strSql, null, orderField, isAsc, pageSize, pageIndex, out total);
        }
        public IEnumerable<T> FindList<T>(string strSql, MySqlParameter[] MySqlParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total) where T : class
        {
            using (var dbConnection = Connection)
            {
                StringBuilder sb = new StringBuilder();
                if (pageIndex == 0)
                {
                    pageIndex = 1;
                }
                int num = (pageIndex - 1) * pageSize;
                int num1 = (pageIndex) * pageSize;
                string OrderBy = "";

                if (!string.IsNullOrEmpty(orderField))
                {
                    if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                    {
                        OrderBy = "  Order By " + orderField;
                    }
                    else
                    {
                        OrderBy = "  Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                    }
                }
                else
                {
                    OrderBy = "  order by (select 0)";
                }
                sb.Append(strSql + OrderBy);
                sb.Append("  limit " + num + "," + pageSize + "");
                total = Convert.ToInt32(DbHelperMySQL.GetSingle(" Select Count(1) From (" + strSql + ") As t", MySqlParameter));
                var IDataReader = DbHelperMySQL.ExecuteReader(sb.ToString(), MySqlParameter);
                return ConvertExtension.IDataReaderToList<T>(IDataReader);
            }
        }
        #endregion

        #region 数据源查询
        public DataTable FindTable(string strSql)
        {
            return FindTable(strSql, null);
        }
        public DataTable FindTable(string strSql, MySqlParameter[] MySqlParameter)
        {
            using (var dbConnection = Connection)
            {
                var IDataReader = DbHelperMySQL.ExecuteReader(strSql, MySqlParameter);
                return ConvertExtension.IDataReaderToDataTable(IDataReader);
            }
        }
        public DataTable FindTable(string strSql, string orderField, bool isAsc, int pageSize, int pageIndex, out int total)
        {
            return FindTable(strSql, null, orderField, isAsc, pageSize, pageIndex, out total);
        }
        public DataTable FindTable(string strSql, MySqlParameter[] MySqlParameter, string orderField, bool isAsc, int pageSize, int pageIndex, out int total)
        {
            using (var dbConnection = Connection)
            {
                StringBuilder sb = new StringBuilder();
                if (pageIndex == 0)
                {
                    pageIndex = 1;
                }
                int num = (pageIndex - 1) * pageSize;
                int num1 = (pageIndex) * pageSize;
                string OrderBy = "";

                if (!string.IsNullOrEmpty(orderField))
                {
                    if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                    {
                        OrderBy = "  Order By " + orderField;
                    }
                    else
                    {
                        OrderBy = "  Order By " + orderField + " " + (isAsc ? "ASC" : "DESC");
                    }
                }
                else
                {
                    OrderBy = "  order by (select 0)";
                }
                sb.Append(strSql + OrderBy);
                sb.Append("  limit " + num + "," + pageSize + "");
                total = Convert.ToInt32(DbHelperMySQL.GetSingle(" Select Count(1) From (" + strSql + ") As t", MySqlParameter));
                var IDataReader = DbHelperMySQL.ExecuteReader(sb.ToString(), MySqlParameter);
                DataTable resultTable = ConvertExtension.IDataReaderToDataTable(IDataReader);
                
                return resultTable;
            }
        }
        public object FindObject(string strSql)
        {
            return FindObject(strSql, null);
        }
        public object FindObject(string strSql, MySqlParameter[] MySqlParameter)
        {
            using (var dbConnection = Connection)
            {
                return DbHelperMySQL.GetSingle(strSql, MySqlParameter);
            }
        }
        #endregion
    }
}
