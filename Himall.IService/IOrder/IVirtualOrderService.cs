using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IVirtualOrderService : IService
    {
        /// <summary>
        /// 根据虚拟预约单实体创建虚拟预约单
        /// </summary>
        /// <param name="model">虚拟预约单实体</param>
        /// <returns></returns>
        bool CreateVirtualOrder(VirtualOrderInfo model);
        /// <summary>
        /// 根据虚拟预约单号更新虚拟预约单支付状态
        /// </summary>
        /// <param name="payNum">虚拟预约单号</param>
        /// <returns></returns>
        bool UpdateMoneyFlagByPayNum(string payNum);
        /// <summary>
        /// 根据虚拟预约单号更新诊所结算
        /// </summary>
        /// <param name="payNum">虚拟预约单号</param>
        /// <returns></returns>
        bool UpdateShopAccountByPayNum(string payNum);
        /// <summary>
        /// 根据虚拟预约单号更新平台结算
        /// </summary>
        /// <param name="payNum">虚拟预约单号</param>
        /// <returns></returns>
        bool UpdatePlatAccountByPayNum(string payNum);
    }
}
