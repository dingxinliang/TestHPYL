#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.Service.DXL
* 项目描述 ：
* 类 名 称 ：NOrderService
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.Service.DXL
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/9/3 15:54:53
* 更新时间 ：2018/9/3 15:54:53
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using Himall.IServices.IDXL;
using Himall.Model;
using Maticsoft.DBUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service.DXL
{
    public class NOrderService : INOrderService
    {
        public void Dispose()
        {
           
        }

        /// <summary>
        /// 获取新加字段模型
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<OrderInfo.NewOrdermodel> GetNewmodel(long orderId)
        {
            string sqlQuery = "SELECT b.RealName doctorName, RemindType,ReceiveDate,date_format(ReceiveStartTime, '%H:%i') ReceiveStartTime, " +
                " date_format(ReceiveEndTime, '%H:%i') ReceiveEndTime FROM himall_orders a " +
                " LEFT JOIN himall_members b on a.ShareUserId = b.Id WHERE a.Id = "+orderId+"";
            return new Repository<OrderInfo.NewOrdermodel>().FindList(sqlQuery).ToList();
        }
    }
}
