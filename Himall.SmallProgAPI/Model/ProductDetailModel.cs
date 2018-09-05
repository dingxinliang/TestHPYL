﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.SmallProgAPI.Model
{
    public class ProductDetailModelForMobie
    {
        public ProductInfoModel Product { get; set; }
        public ShopInfoModel Shop { get; set; }
        public CollectionSKU Color { get; set; }
        public CollectionSKU Size { get; set; }
        public CollectionSKU Version { get; set; }
        public int MaxSaleCount { get; set; }
        public string Title { get; set; }
        public CashDepositsObligation CashDepositsServer { get; set; }
        public string ProductAddress { get; set; }
        public FreightTemplateInfo FreightTemplate { get; set; }
        public string VShopLog { get; set; }

    }

    public class ProductInfoModel
    {
        /// <summary>
        /// 诊疗项目ID
        /// </summary>
        public long ProductId { get; set; }
        /// <summary>
        /// 诊疗项目状态
        /// </summary>
        public ProductInfo.ProductSaleStatus ProductSaleStatus { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public ProductInfo.ProductAuditStatus AuditStatus { get; set; }
        /// <summary>
        /// 诊疗项目图片地址
        /// </summary>
        public List<string> ImagePath { get; set; }
        /// <summary>
        /// 诊疗项目名
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 诊疗项目市场价
        /// </summary>
        public decimal MarketPrice { get; set; }
        /// <summary>
        /// 诊疗项目的简单描述
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// 诊疗项目描述
        /// </summary>
        public string ProductDescription { get; set; }
        /// <summary>
        /// 诊疗项目最低价
        /// </summary>
        public decimal MinSalePrice { get; set; }
        /// <summary>
        /// 是否收藏
        /// </summary>
        public bool IsFavorite { get; set; }
        /// <summary>
        /// 咨询数
        /// </summary>
        public int Consultations { get; set; }
        /// <summary>
        /// 诊疗项目评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        public int NicePercent { get; set; }

        /// <summary>
        /// 是否真正参与限时购
        /// </summary>
        public bool IsOnLimitBuy { get; set; }

    }
    public class CollectionSKU : List<ProductSKU>
    {
        public override string ToString()
        {
            string str = "";
            foreach (var item in this)
            {
                str +=item.Value+",";
            }
            return str.TrimEnd(',');
        }
    }

    public class ProductSKU
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public long SkuId { get; set; }
        public string EnabledClass { get; set; }
        public string SelectedClass { get; set; }
        public string Img { get; set; }
    }

    public class HotProductInfo
    {
        public string ImgPath { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int SaleCount { get; set; }
        public long Id { get; set; }

    }

    public class ShopInfoModel
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public decimal ProductMark { get; set; }
        public decimal PackMark { get; set; }
        public decimal ServiceMark { get; set; }
        public decimal ComprehensiveMark { get; set ; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public decimal FreeFreight { get; set; }

        /// <summary>
        /// 店铺在售诊疗项目数
        /// </summary>
        public long ProductNum { get; set; }

        /// <summary>
        /// 店铺优惠券数
        /// </summary>
        public long CouponCount { get; set; }

        /// <summary>
        /// 微店Id
        /// </summary>
        public long VShopId { get; set; }

        /// <summary>
        /// 宝贝描述得分
        /// </summary>
        public decimal ProductAndDescription { get; set; }
        /// <summary>
        /// 诊所服务得分
        /// </summary>
        public decimal SellerServiceAttitude { get; set; }
        /// <summary>
        /// 发货物流得分
        /// </summary>
        public decimal SellerDeliverySpeed { get; set; }

        public long FavoriteShopCount { get; set; }
    }

}