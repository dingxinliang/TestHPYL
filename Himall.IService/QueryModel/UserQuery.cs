#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.IServices.QueryModel
* 项目描述 ：
* 类 名 称 ：UserQuery
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.IServices.QueryModel
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/9/9 14:11:07
* 更新时间 ：2018/9/9 14:11:07
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    /// <summary>
    /// 查询项
    /// </summary>
   public class UserQuery: QueryBase
    {
        public string UserName { get; set; }
        public string UserRealName { get; set; }
        public long auditStatus { get; set; }
        public string UserCell { get; set; }
        public long shopId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 返回用户数据模型
    /// </summary>
    public class UserModel {

        public long Id { get; set; }
        
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public System.DateTime CreateDate { get; set; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string RealName { get; set; }
        public string CellPhone { get; set; }
        public string QQ { get; set; }
        public string Address { get; set; }
        public bool Disabled { get; set; }
        public System.DateTime LastLoginDate { get; set; }
        public int OrderNumber { get; set; }
        public decimal Expenditure { get; set; }
        public int Points { get; set; }
        public string Nick { get; set; }
        internal string photo { get; set; }
        public long ParentSellerId { get; set; }
        public string Remark { get; set; }
        public string PayPwd { get; set; }
        public string PayPwdSalt { get; set; }
        public Nullable<long> InviteUserId { get; set; }
        public int Sex { get; set; }
        public Nullable<long> ShareUserId { get; set; }
        public decimal TotalAmount { get; set; }
        public Nullable<System.DateTime> BirthDay { get; set; }
        public string Occupation { get; set; }
        public decimal NetAmount { get; set; }
        public Nullable<System.DateTime> LastConsumptionTime { get; set; }
        public string CreateDateStr { get; set; }
    }
}
