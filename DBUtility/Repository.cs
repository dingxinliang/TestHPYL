/****************************************************************************
*Copyright (c) 2018 Microsoft All Rights Reserved.
*CLR版本： 4.0.30319.42000
*公司名称：Microsoft
*命名空间：DBUtility
*文件名：  Repository
*版本号：  V1.0.0.0
*当前的用户域：QH-20160830FLFX
*创建人：丁新亮
*创建时间：2018/7/15 0:54:01

*描述：
*
*=====================================================================
*修改标记
*修改时间：2018/7/15 0:54:01
*修改人： 
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;
using XL.Util.WebControl;
using System.Data.Common;

namespace Maticsoft.DBUtility
{
    public class Repository<T> where T : class,new()
    {
        MySqlDatabase db = new MySqlDatabase();
        #region 对象实体 查询
        public T FindEntity(object keyValue)
        {
            return db.FindEntity<T>(keyValue);
        }
        public T FindEntity(Expression<Func<T, bool>> condition)
        {
            return db.FindEntity<T>(condition);
        }
        public IQueryable<T> IQueryable()
        {
            return db.IQueryable<T>();
        }
        public IQueryable<T> IQueryable(Expression<Func<T, bool>> condition)
        {
            return db.IQueryable<T>(condition);
        }
        public IEnumerable<T> FindList(string strSql)
        {
            return db.FindList<T>(strSql);
        }
        public IEnumerable<T> FindList(string strSql, object MySqlParameter)
        {
            return db.FindList<T>(strSql, MySqlParameter);
        }
        public IEnumerable<T> FindList(Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindList<T>(pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        public IEnumerable<T> FindList(Expression<Func<T, bool>> condition, Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindList<T>(condition, pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        public IEnumerable<T> FindList(string strSql, Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindList<T>(strSql, pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        public IEnumerable<T> FindList(string strSql, MySqlParameter[] MySqlParameter, Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindList<T>(strSql, MySqlParameter, pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        #endregion

        #region 数据源 查询
        public DataTable FindTable(string strSql)
        {
            return db.FindTable(strSql);
        }
        public DataTable FindTable(string strSql, MySqlParameter[] MySqlParameter)
        {
            return db.FindTable(strSql, MySqlParameter);
        }
        public DataTable FindTable(string strSql, Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindTable(strSql, pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        public DataTable FindTable(string strSql, MySqlParameter[] MySqlParameter, Pagination pagination)
        {
            int total = pagination.records;
            var data = db.FindTable(strSql, MySqlParameter, pagination.sidx, pagination.sord.ToLower() == "asc" ? true : false, pagination.rows, pagination.page, out total);
            pagination.records = total;
            return data;
        }
        public object FindObject(string strSql)
        {
            return db.FindObject(strSql);
        }
        public object FindObject(string strSql, MySqlParameter[] MySqlParameter)
        {
            return db.FindObject(strSql, MySqlParameter);
        }
        #endregion
    }
}
