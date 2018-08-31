﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.Service.DXL
* 项目描述 ：
* 类 名 称 ：FollowtService
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.Service.DXL
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 18:34:25
* 更新时间 ：2018/8/26 18:34:25
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using Himall.CommonModel;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Maticsoft.DBUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XL.Util.WebControl;

namespace Himall.Service.DXL
{
    public class FollowService : IFollowtService
    {
        public void BatchOn(IEnumerable<long> ids, long shopId)
        {
            string sql = "update HPYL_FollowTemPlate set HTP_State=1 where HTP_ID in(" + string.Join(",", ids) + ")";
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void BatchStop(IEnumerable<long> ids, long shopId)
        {
            string sql = "update HPYL_FollowTemPlate set HTP_State=0 where HTP_ID in(" + string.Join(",", ids) + ")";
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void Creatdoctor(string cid,string title, string pic, string remark, string ids, long shopId, long id)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                sql = "update HPYL_AdviceArticle a,himall_shopcategories b set HAA_Title='" + title + "',HFT_ID=" + cid + ",HAA_PicUrl='"+pic+ "',HAA_Content='"+remark+"'  where a.HFT_ID=b.Id  and  HAA_ID=" + ids + " and ShopId=" + shopId + "";
            }
            else
            {
                sql = "INSERT INTO HPYL_AdviceArticle  ( `HFT_ID`, `HAA_Title`, `HAA_PicUrl`, `HAA_Content`, `HAA_State`) VALUES (" + cid + ", '" + title + "', '"+pic+"', '"+remark+"', 1);";
            }
            DbHelperMySQL.ExecuteSql(sql);
        }

        /// <summary>
        /// 删除随访内容
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="shopId"></param>
        public void DeleteContent(IEnumerable<long> ids, long shopId)
        {
            string sql = "delete from HPYL_FollowPlanContent where HFC_ID in(" + string.Join(",", ids) + ");";
            DbHelperMySQL.ExecuteSql(sql);
        }

        /// <summary>
        /// 删除医嘱模板
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopId"></param>
        public void Deletedoctor(IEnumerable<long> ids, long shopId)
        {
            string sql = "delete from HPYL_AdviceArticle where HAA_ID in(" + string.Join(",", ids) + ");";
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void DeleteProduct(IEnumerable<long> ids, long shopId)
        {
            string sql = "delete from HPYL_FollowTemPlate where HTP_ID in(" + string.Join(",", ids) + ");delete from HPYL_FollowPlanContent where HTP_ID in(" + string.Join(",", ids) + ")";
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void Dispose()
        {
            
        }

        public void doctorOn(IEnumerable<long> ids, long shopId)
        {
            string sql = "update HPYL_AdviceArticle set HAA_State=1 where HAA_ID in(" + string.Join(",", ids) + ")";
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void doctorStop(IEnumerable<long> ids, long shopId)
        {
            string sql = "update HPYL_AdviceArticle set HAA_State=0 where HAA_ID in(" + string.Join(",", ids) + ")";
            DbHelperMySQL.ExecuteSql(sql);
        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public QueryPageModel<FollowContentQuery> GetFollowContent(FollowSearch queryModel, long id)
        {
            string sql = "SELECT HFC_ID,a.HTP_ID, CONCAT(HFC_Days, '天')   Days,HFC_Days, HFC_Content,b.HTP_Name FollowName,  c.NAME CategoryName FROM HPYL_FollowPlanContent a " +
                " inner join HPYL_FollowTemPlate b on a.HTP_ID = b.HTP_ID " +
                " INNER JOIN himall_shopcategories c ON b.HFT_ID = c.Id " +
                " WHERE  ShopId ="+queryModel.shopId+"  and a.HTP_ID ="+ id + "";
            var total = new Repository<FollowContentQuery>().FindList(sql);
            Pagination pagination = new Pagination();
            pagination.page = queryModel.PageNumber;
            pagination.rows = queryModel.PageSize;
            pagination.sidx = "HFC_Days";
            pagination.sord = "asc";

            var result = new Repository<FollowContentQuery>().FindList(sql, pagination).ToList();
            QueryPageModel<FollowContentQuery> pageModel = new QueryPageModel<FollowContentQuery>()
            {
                Total = (total == null ? 0 : total.Count()),
                Models = result
            };
            return pageModel;
        }

        public QueryPageModel<FollowDoctorQuery> GetFollowDoctor(FollowSearch search)
        {
            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.Append("SELECT HAA_ID, HFT_ID, HAA_Title,HAA_PicUrl,HAA_Content, b.NAME CategoryName, HAA_State," +
                "( CASE HAA_State WHEN 0 THEN '未启用' ELSE '已启用' END) State FROM HPYL_AdviceArticle a " +
                " INNER JOIN himall_shopcategories b ON a.HFT_ID = b.Id WHERE ShopId ="+search.shopId+"");

            if (search.ShopCategoryId > 0)
            {
                sqlQuery.Append(" and HFT_ID=" + search.ShopCategoryId + "");
            }
            if (!string.IsNullOrEmpty(search.keyWords))
            {
                sqlQuery.Append(" and HAA_Title like '%" + search.keyWords + "%'");
            }
            if (!string.IsNullOrEmpty(search.AuditStatus.ToString()))
            {
                sqlQuery.Append(" and HAA_State=" + search.AuditStatus + "");
            }

            var total = new Repository<FollowDoctorQuery>().FindList(sqlQuery.ToString());
            Pagination pagination = new Pagination();
            pagination.page = search.PageNumber;
            pagination.rows = search.PageSize;
            pagination.sidx = "HAA_ID";
            pagination.sord = "desc";

            var result = new Repository<FollowDoctorQuery>().FindList(sqlQuery.ToString(), pagination).ToList();
            QueryPageModel<FollowDoctorQuery> pageModel = new QueryPageModel<FollowDoctorQuery>()
            {
                Total = (total == null ? 0 : total.Count()),
                Models = result
            };
            return pageModel;
        }

        public void SaveFollow(string name, string cId, string ids, long shopId,long uid)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                sql = "update HPYL_FollowTemPlate a,himall_shopcategories b set HTP_Name='" + name + "',HFT_ID=" + cId + "  where a.HFT_ID=b.Id  and  HTP_ID="+ ids + " and ShopId=" + shopId + "";
             }
            else {
                sql = "INSERT INTO hpyl_followtemplate  ( `HFT_ID`, `HTP_Name`, `HTP_State`, `HTP_CreateTime`, `HTP_UserId`) VALUES ("+cId+", '"+name+"', '1', now(), "+uid+");";
            }
            DbHelperMySQL.ExecuteSql(sql);
        }

        public void SaveFollowContent(string cId, string ids, long shopId, int day, string cont)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(ids))
            {
                sql = "UPDATE hpyl_followplancontent a,hpyl_followtemplate b, himall_shopcategories c SET HFC_Days =" + day + ", HFC_Content = '"+ cont + "'" +
                     " WHERE a.HTP_ID = b.HTP_ID and b.HFT_ID = c.Id AND HFC_ID = " + ids + " AND ShopId =" + shopId + "";
            }
            else
            {
                sql = "INSERT INTO hpyl_followplancontent  ( `HTP_ID`, `HFC_Days`, `HFC_Content`) VALUES (" + cId + ", " + day + ", '"+ cont + "');";
            }
            DbHelperMySQL.ExecuteSql(sql);
        }

        public ObsoletePageModel<FollowQuery> SearchFollow(FollowSearch search)
        {
            StringBuilder sqlQuery = new StringBuilder();
            sqlQuery.Append("SELECT HTP_ID,HFT_ID,HTP_Name,b.Name, HTP_State,(CASE HTP_State when 0 then '未启用' else '已启用' end )State,date_format(HTP_CreateTime, '%Y-%m-%d %H:%i' )Time,HTP_UserId from HPYL_FollowTemPlate a inner join himall_shopcategories b on a.HFT_ID=b.Id where ShopId =" + search.shopId+"");

            if (search.ShopCategoryId > 0)
            {
                sqlQuery.Append(" and HFT_ID=" + search.ShopCategoryId + "");
            }
            if (!string.IsNullOrEmpty(search.keyWords))
            {
                sqlQuery.Append(" and HTP_Name like '%" + search.keyWords + "%'");
            }
            if (!string.IsNullOrEmpty(search.AuditStatus.ToString()))
            {
                sqlQuery.Append(" and HTP_State=" + search.AuditStatus + "");
            }

            var total = new Repository<FollowQuery>().FindList(sqlQuery.ToString());
            Pagination pagination = new Pagination();
            pagination.page = search.PageNumber;
            pagination.rows = search.PageSize;
            pagination.sidx = "HTP_ID";
            pagination.sord = "desc";

            var result = new Repository<FollowQuery>().FindList(sqlQuery.ToString(), pagination).AsQueryable();
            ObsoletePageModel<FollowQuery> pageModel = new ObsoletePageModel<FollowQuery>()
            {
                Total = (total == null ? 0 : total.Count()),
                Models = result
            };
            return pageModel;

        }
    }
}