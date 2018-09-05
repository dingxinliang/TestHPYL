#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.IServices.IDXL
* 项目描述 ：
* 类 名 称 ：INOrderService
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.IServices.IDXL
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/9/3 15:49:08
* 更新时间 ：2018/9/3 15:49:08
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
using Himall.Model;
using static Himall.Model.OrderInfo;

namespace Himall.IServices.IDXL
{
    public interface INOrderService : IService
    {
        List<OrderInfo.NewOrdermodel> GetNewmodel(long orderId);
    }
}
