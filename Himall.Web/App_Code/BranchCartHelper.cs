using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Web.App_Code
{
    public class BranchCartHelper
    {
        private IBranchCartService _iBranchCartService;
        private IProductService _iProductService;
        private IShopBranchService _iShopBranchService;

        public BranchCartHelper(IBranchCartService iBranchCartService, IProductService iProductService, IShopBranchService iShopBranchService)
        {
            this._iBranchCartService = iBranchCartService;
            this._iProductService = iProductService;
            this._iShopBranchService = iShopBranchService;
        }

        public BranchCartHelper()
        {
            this._iBranchCartService = ServiceHelper.Create<IBranchCartService>();
            this._iProductService = ServiceHelper.Create<IProductService>();
            this._iShopBranchService = ServiceHelper.Create<IShopBranchService>();
        }

        public void UpdateCartInfoFromCookieToServer(long memberId)
        {
            string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                string[] strArray = cookie.Split(new char[] { ',' });
                ShoppingCartItem[] cartItems = new ShoppingCartItem[strArray.Length];
                int num = 0;
                foreach (string str2 in strArray)
                {
                    string[] strArray2 = str2.Split(new char[] { ':' });
                    string str3 = strArray2[0];
                    int num2 = int.Parse(strArray2[1]);
                    long num3 = long.Parse(strArray2[2]);
                    ShoppingCartItem item = new ShoppingCartItem
                    {
                        ShopBranchId = new long?(num3),
                        SkuId = strArray2[0],
                        Quantity = int.Parse(strArray2[1])
                    };
                    cartItems[num++] = item;
                }
                this._iBranchCartService.AddToCart(cartItems, memberId);
            }
            WebHelper.DeleteCookie("HIMALL-CART-BRANCH");

        }

        public IEnumerable<long> GetCartProductIds(long memberId, long shopBranchId)
        {
            long[] numArray = new long[0];
            if (memberId > 0L)
            {
                numArray = Enumerable.ToArray<long>(Enumerable.Select<ShoppingCartItem, long>(this._iBranchCartService.GetCart(memberId, shopBranchId).Items, (Func<ShoppingCartItem, long>)(item => item.ProductId)));
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray1 = cookie.Split(',');
                    numArray = new long[strArray1.Length];
                    int num = 0;
                    foreach (string str in strArray1)
                    {
                        char[] chArray = new char[1]
            {
              ':'
            };
                        string[] strArray2 = str.Split(chArray);
                        numArray[num++] = long.Parse(strArray2[0]);
                    }
                }
            }
            return (IEnumerable<long>)numArray;
        }

        public IEnumerable<string> GetCartProductSKUIds(long memberId, long shopBranchId)
        {
            string[] strArray1 = new string[0];
            if (memberId > 0L)
            {
                strArray1 = Enumerable.ToArray<string>(Enumerable.Select<ShoppingCartItem, string>(this._iBranchCartService.GetCart(memberId, shopBranchId).Items, (Func<ShoppingCartItem, string>)(item => item.SkuId)));
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART-BRANCH");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray2 = cookie.Split(',');
                    strArray1 = new string[strArray2.Length];
                    int num = 0;
                    foreach (string str in strArray2)
                    {
                        char[] chArray = new char[1]
            {
              ':'
            };
                        string[] strArray3 = str.Split(chArray);
                        strArray1[num++] = strArray3[0];
                    }
                }
            }
            return (IEnumerable<string>)strArray1;
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
                    List<ShoppingCartItem> list = new List<ShoppingCartItem>();
                    foreach (string str in strArray1)
                    {
                        char[] chArray = new char[1]
            {
              ':'
            };
                        string[] strArray2 = str.Split(chArray);
                        if (shopBranchId == 0L || strArray2[2] == shopBranchId.ToString())
                            list.Add(new ShoppingCartItem()
                            {
                                ProductId = long.Parse(strArray2[0].Split('_')[0]),
                                SkuId = strArray2[0],
                                Quantity = int.Parse(strArray2[1]),
                                ShopBranchId = new long?(long.Parse(strArray2[2]))
                            });
                    }
                    shoppingCartInfo.Items = (IEnumerable<ShoppingCartItem>)list;
                }
            }
            return shoppingCartInfo;
        }

        public ShoppingCartInfo GetCartNoCache(long memberId, long shopBranchId)
        {
            ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0L)
            {
                shoppingCartInfo = this._iBranchCartService.GetCartNoCache(memberId, shopBranchId);
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

        public void RemoveFromCart(IEnumerable<string> skuIds, long memberId, long shopBranchId)
        {
            if (memberId > 0L)
            {
                this._iBranchCartService.DeleteCartItem(skuIds, memberId, shopBranchId);
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
                        if (!Enumerable.Contains<string>(skuIds, str3) && shopBranchId.ToString() != str4)
                            str1 = str1 + "," + str2;
                    }
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART-BRANCH", str1);
                }
            }
        }

        public void UpdateCartItem(string skuId, int count, long memberId, long shopBranchId)
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
        }

        public void AddToCart(string skuId, int count, long memberId, long shopBranchId)
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
            if (memberId > 0L)
            {
                this._iBranchCartService.AddToCart(skuId, count, memberId, shopBranchId);
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
                            str1 = str1 + (object)"," + skuId + ":" + (string)(object)(int.Parse(strArray2[1]) + count) + ":" + (string)(object)shopBranchId;
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
        }

        private void CheckSkuIdIsValid(string skuId, long shopBranchId)
        {
            long result = 0L;
            long.TryParse(skuId.Split('_')[0], out result);
            if (result == 0L)
                throw new InvalidPropertyException("SKUId无效");
            if (this._iProductService.GetSku(skuId) == null)
                throw new InvalidPropertyException("SKUId无效");
        }
    }
}