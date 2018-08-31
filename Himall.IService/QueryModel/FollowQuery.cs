#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.IServices.QueryModel
* 项目描述 ：
* 类 名 称 ：FollowQuery
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.IServices.QueryModel
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 12:48:03
* 更新时间 ：2018/8/26 12:48:03
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using Himall.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public partial class FollowQuery : QueryBase
    {


        /// <summary>
        /// 主键
        /// </summary>
        public long HTP_ID { get; set; }
        /// <summary>
        /// 类别ID
        /// </summary>
        public long HFT_ID { get; set; }
        /// <summary>
        /// 随访名称
        /// </summary>
        public string HTP_Name { get; set; }
        /// <summary>
        /// 启用状态
        /// </summary>
        public int HTP_State { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string  Time { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
   
 
            public string State { get; set; }
        public long HTP_UserId { get; set; }

        public string Name { get; set; }
        public string keyWords { get; set; }
        public string AuditStatus { get; set; }
        public int OrderKey { get; set; }
        public bool OrderType { get; set; }
        public long shopId { get; set; }
        /// <summary>
        /// 要过滤的门店ID
        /// </summary>
       
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long ShopCategoryId { get; set; }
    }

    public partial class FollowContentQuery : QueryBase
    {


        /// <summary>
        /// 主键
        /// </summary>
        public long HFC_ID { get; set; }
        /// <summary>
        /// 模板类别
        /// </summary>
        public long HTP_ID { get; set; }
        /// <summary>
        /// 随访间隔时间
        /// </summary>
        public int HFC_Days { get; set; }
        public string Days { get; set; }
        /// <summary>
        /// 随访内容
        /// </summary>
        public string HFC_Content { get; set; }
        /// <summary>
        /// 模板
        /// </summary>
        public string FollowName { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string CategoryName { get; set;}
        public long shopId { get; set; }
        /// <summary>
        /// 要过滤的门店ID
        /// </summary>
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    
    }

    /// <summary>
    /// 医嘱模板
    /// </summary>
    public partial class FollowDoctorQuery : QueryBase
    {


        /// <summary>
        /// 主键
        /// </summary>
        public long HAA_ID { get; set; }
        /// <summary>
        /// 模板类别
        /// </summary>
        public long HFT_ID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string  HAA_Title { get; set; }
       
        public string HAA_PicUrl { get; set; }

        public int HAA_State { get; set; }
        /// <summary>
        /// 随访内容
        /// </summary>
        public string HAA_Content { get; set; }
    
        /// <summary>
        /// 类别
        /// </summary>
        public string CategoryName { get; set; }

        public string State { get; set; }

        public long shopId { get; set; }
        /// <summary>
        /// 要过滤的门店ID
        /// </summary>
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

    }
    public class FollowSearch
    {
        public string keyWords { get; set; }
        
        public int OrderKey { get; set; }
        public bool OrderType { get; set; }
        public string  AuditStatus { get; set; }
        public long shopId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public long ShopCategoryId { get; set; }
    }

    
}
