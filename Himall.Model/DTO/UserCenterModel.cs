using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class UserCenterModel
    {

        public UserCenterModel()
        {
            FollwProducts = new List<FollowProduct>();
            FollowShops = new List<FollowShop>();
            FollowShopCarts = new List<FollowShopCart>();
        }
        /// <summary>
        /// 待支付预约单
        /// </summary>
        public long WaitPayOrders { set; get; }

        /// <summary>
        /// 待结算预约单
        /// </summary>
        public long WaitReceivingOrders { set; get; }


        /// <summary>
        /// 待发货预约单
        /// </summary>
        public long WaitDeliveryOrders { set; get; }


        /// <summary>
        /// 待评价预约单
        /// </summary>
        public long WaitEvaluationOrders { set; get; }

        /// <summary>
        /// 共消费金额
        /// </summary>
        public decimal Expenditure { set; get; }

        /// <summary>
        /// 关注诊疗项目
        /// </summary>
        public List<FollowProduct> FollwProducts { set; get; }

        /// <summary>
        /// 关注诊疗项目数
        /// </summary>
        public int FollowProductCount { set; get; }


        /// <summary>
        /// 用户积分数
        /// </summary>
        public int Intergral { set; get; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public string GradeName { set; get; }

        //退换货
        public int RefundCount { set; get; }

        /// <summary>
        /// 用户优惠券
        /// </summary>
        public int UserCoupon { set; get; }

        /// <summary>
        /// 关注店铺
        /// </summary>
        public List<FollowShop> FollowShops { set; get; }

        /// <summary>
        /// 关注店铺数
        /// </summary>
        public int FollowShopsCount { set; get; }

        /// <summary>
        /// 购物车
        /// </summary>
        public List<FollowShopCart> FollowShopCarts { set; get; }
        /// <summary>
        /// 购物车数
        /// </summary>
        public int FollowShopCartsCount { get; set; }

        public MemberAccountSafety memberAccountSafety { get; set; }
        //预约单信息
        public IQueryable<OrderInfo> Orders { set; get; }
    }


    public class FollowProduct
    {

        private string _imagePath;
        /// <summary>
        /// 诊疗项目价格
        /// </summary>
        public decimal Price { set; get; }

        /// <summary>
        /// 图片
        /// </summary>
        public string ImagePath
        {
            set { _imagePath = value; }
            get { return _imagePath; }
        }

        /// <summary>
        /// 诊疗项目ID
        /// </summary>
        public long ProductId { set; get; }

        /// <summary>
        /// 诊疗项目名称
        /// </summary>
        public string ProductName { set; get; }
    }

    public class FollowShop
    {
        public string Logo { set; get; }

        public string ShopName { set; get; }

        public long ShopID { set; get; }
        /// <summary>
        /// 关注人数
        /// </summary>
        public long ConcernedCount { get; set; }

    }

    public class FollowShopCart
    {
        public string ImagePath { set; get; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
    }

}
