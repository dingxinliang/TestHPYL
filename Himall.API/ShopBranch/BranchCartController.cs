using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using Himall.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.API
{
    public class BranchCartController : BaseApiController
    {
        private IShopBranchService _iShopBranchService;
        private ICartService _iCartService;
        private IBranchCartService _iBranchCartService;
        private IProductService _iProductService;
        private IShopService _iShopService;
        private IVShopService _iVShopService;

        public BranchCartController()
        {
            this._iShopBranchService = (IShopBranchService)new ShopBranchService();
            this._iCartService = (ICartService)new CartService();
            this._iProductService = (IProductService)new ProductService();
            this._iBranchCartService = (IBranchCartService)new BranchCartService();
            this._iShopService = (IShopService)new ShopService();
            this._iVShopService = (IVShopService)new VShopService();
        }

        public ShoppingCartInfo GetCart(long memberId, long shopBranchId)
        {
            ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0L)
            {
                shoppingCartInfo = this._iBranchCartService.GetCart(memberId, shopBranchId);
            }
            else
            {
                shoppingCartInfo = new ShoppingCartInfo();
                string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray1 = cookie.Split(',');
                    ShoppingCartItem[] shoppingCartItemArray = new ShoppingCartItem[strArray1.Length];
                    int num = 0;
                    foreach (string str in strArray1)
                    {
                        char[] chArray = new char[1]
            {
              ':'
            };
                        string[] strArray2 = str.Split(chArray);
                        if (shopBranchId == 0L || strArray2[2] == shopBranchId.ToString())
                            shoppingCartItemArray[num++] = new ShoppingCartItem()
                            {
                                ProductId = long.Parse(strArray2[0].Split('_')[0]),
                                SkuId = strArray2[0],
                                Quantity = int.Parse(strArray2[1]),
                                ShopBranchId = new long?(long.Parse(strArray2[2]))
                            };
                    }
                    shoppingCartInfo.Items = (IEnumerable<ShoppingCartItem>)shoppingCartItemArray;
                }
            }
            return shoppingCartInfo;
        }

        public object GetUpdateCartItem(string skuId, int count, long shopBranchId)
        {
            this.CheckSkuIdIsValid(skuId, shopBranchId);
            SKUInfo sku = this._iProductService.GetSku(skuId);
            if (sku == null)
                throw new HimallException("错误的SKU");
            if ((long)count > sku.Stock)
                throw new HimallException("库存不足");
            if (this._iShopBranchService.GetShopBranchById(shopBranchId) == null)
                throw new HimallException("错误的门店id");
            List<ShopBranchSkusInfo> skusByIds = this._iShopBranchService.GetSkusByIds(shopBranchId, (IEnumerable<string>)new List<string>()
            {
            skuId
            });
            if (skusByIds == null || skusByIds.Count == 0 || skusByIds[0].Status == ShopBranchSkuStatus.InStock)
                throw new HimallException("门店没有该商品或已下架");
            if (Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)skusByIds).Stock < count)
                throw new HimallException("门店库存不足");
            long memberId = 0L;
            if (this.CurrentUser != null)
                memberId = this.CurrentUser.Id;
            if (memberId > 0L)
            {
                this._iBranchCartService.UpdateCart(skuId, count, memberId, shopBranchId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray1 = cookie.Split(',');
                    string str1 = string.Empty;
                    bool flag = false;
                    foreach (string str2 in strArray1)
                    {
                        string[] strArray2 = str2.Split(':');
                        if (strArray2[0] == skuId && strArray2[2] == shopBranchId.ToString())
                        {
                            str1 = str1 + (object)"," + skuId + ":" + (string)(object)count + ":" + (string)(object)shopBranchId;
                            flag = true;
                        }
                        else
                            str1 = str1 + "," + str2;
                    }
                    if (!flag)
                        str1 = str1 + (object)"," + skuId + ":" + (string)(object)count + ":" + (string)(object)shopBranchId;
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART-BRANCH", str1);
                }
                else
                    WebHelper.SetCookie("HIMALL-CART-BRANCH", string.Format("{0}:{1}:{2}", (object)skuId, (object)count, (object)shopBranchId));
            }
            return (object)this.Json(new
            {
                Success = true
            });
        }

        public void CheckSkuIdIsValid(string skuId, long shopBranchId)
        {
            long result = 0L;
            long.TryParse(skuId.Split('_')[0], out result);
            if (result == 0L)
                throw new InvalidPropertyException("SKUId无效");
            if (this._iProductService.GetSku(skuId) == null)
                throw new InvalidPropertyException("SKUId无效");
        }

        public object GetBranchCartProducts(long shopBranchId)
        {
            long memberId = 0L;
            decimal discount = 1.0M;
            if (base.CurrentUser != null)
            {
                memberId = base.CurrentUser.Id;
                discount = base.CurrentUser.MemberDiscount;
            }
            ShoppingCartInfo cart = this.GetCart(memberId, shopBranchId);
            ShopBranchInfo shopBranchById = this._iShopBranchService.GetShopBranchById(shopBranchId);
            IProductService productService = this._iProductService;
            IShopService shopService = this._iShopService;
            IVShopService vshopService = this._iVShopService;
            List<long> list = new List<long>();
            decimal prodPrice = 0.0M;
            var enumerable = from s in cart.Items.Select(delegate(ShoppingCartItem item)
            {
                ProductInfo product = productService.GetProduct(item.ProductId);
                ShopBranchSkusInfo info2 = this._iShopBranchService.GetSkusByIds(shopBranchId, new List<string> { item.SkuId }).FirstOrDefault<ShopBranchSkusInfo>();
                ShopInfo shop = shopService.GetShop(product.ShopId, false);
                SKUInfo sku = null;
                string str = "";
                if (null != shop)
                {
                    VShopInfo vShopByShopId = vshopService.GetVShopByShopId(shop.Id);
                    sku = productService.GetSku(item.SkuId);
                    if (sku == null)
                    {
                        return null;
                    }
                    prodPrice = sku.SalePrice;
                    if (shop.IsSelf)
                    {
                        prodPrice = sku.SalePrice * discount;
                    }
                    ProductType type = TypeApplication.GetType(product.TypeId);
                    str = "";
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        str = str + sku.Size + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        str = str + sku.Color + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        str = str + sku.Version + "&nbsp;&nbsp;";
                    }
                    return new { bId = shopBranchId, cartItemId = item.Id, skuId = item.SkuId, id = product.Id, name = product.ProductName, price = prodPrice, count = item.Quantity, stock = (info2 == null) ? 0 : info2.Stock, status = (info2 == null) ? 1 : ((info2.Status == ShopBranchSkuStatus.Normal) ? ((item.Quantity > info2.Stock) ? 2 : 0) : 1), skuDetails = str, AddTime = item.AddTime };
                }
                return null;
            })
            where s != null
            orderby s.status, s.AddTime descending
            select s;
            var content = new
            {
                Success = true,
                products = enumerable,
                amount = enumerable.Where(x => x.status == 0).Sum(x => x.price * x.count),
                totalCount = enumerable.Where(x => x.status == 0).Sum(x => x.count),
                DeliveFee = shopBranchById.DeliveFee,
                DeliveTotalFee = shopBranchById.DeliveTotalFee,
                FreeMailFee = shopBranchById.FreeMailFee,
                shopBranchStatus = shopBranchById.Status
            };
            return base.Json(content);

        }

        public object GetClearBranchCartProducts(long shopBranchId)
        {
            long memberId = this.CurrentUser != null ? this.CurrentUser.Id : 0L;
            foreach (ShoppingCartItem shoppingCartItem in this.GetCart(memberId, shopBranchId).Items)
                this.RemoveFromCart(shoppingCartItem.SkuId, memberId, shopBranchId);
            return (object)this.Json(new
            {
                Success = true
            });
        }

        public object GetClearBranchCartInvalidProducts(long shopBranchId)
        {
            long memberId = this.CurrentUser != null ? this.CurrentUser.Id : 0L;
            foreach (ShoppingCartItem shoppingCartItem in this.GetCart(memberId, shopBranchId).Items)
            {
                ShopBranchSkusInfo shopBranchSkusInfo = Enumerable.FirstOrDefault<ShopBranchSkusInfo>((IEnumerable<ShopBranchSkusInfo>)this._iShopBranchService.GetSkusByIds(shopBranchId, (IEnumerable<string>)new List<string>()
        {
          shoppingCartItem.SkuId
        }));
                if (shopBranchSkusInfo.Status != ShopBranchSkuStatus.Normal || shoppingCartItem.Quantity > shopBranchSkusInfo.Stock)
                    this.RemoveFromCart(shoppingCartItem.SkuId, memberId, shopBranchId);
            }
            return (object)this.Json(new
            {
                Success = true
            });
        }

        public void RemoveFromCart(string skuId, long memberId, long shopBranchId)
        {
            if (memberId > 0L)
            {
                this._iBranchCartService.DeleteCartItem(skuId, memberId, shopBranchId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray1 = cookie.Split(',');
                    string str1 = string.Empty;
                    foreach (string str2 in strArray1)
                    {
                        string[] strArray2 = str2.Split(':');
                        string str3 = strArray2[0];
                        string str4 = strArray2[2];
                        if (str3 != skuId && shopBranchId.ToString() != str4)
                            str1 = str1 + "," + str2;
                    }
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART-BRANCH", str1);
                }
            }
        }
    }
}
