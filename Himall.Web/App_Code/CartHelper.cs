using Autofac;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Himall.Web
{
    public class CartHelper
    {
        private ICartService _iCartService;
        private IProductService _iProductService;

        public CartHelper(ICartService iCartService, IProductService iProductService)
        {
            this._iCartService = iCartService;
            this._iProductService = iProductService;
        }

        public CartHelper()
        {
            this._iCartService = ServiceHelper.Create<ICartService>();
            this._iProductService = ServiceHelper.Create<IProductService>();
        }

        public void UpdateCartInfoFromCookieToServer(long memberId)
        {
            string cookie = WebHelper.GetCookie("HIMALL-CART");
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
                    shoppingCartItemArray[num++] = new ShoppingCartItem()
                    {
                        SkuId = strArray2[0],
                        Quantity = int.Parse(strArray2[1])
                    };
                }
                this._iCartService.AddToCart((IEnumerable<ShoppingCartItem>)shoppingCartItemArray, memberId);
            }
            WebHelper.DeleteCookie("HIMALL-CART");
        }

        public IEnumerable<long> GetCartProductIds(long memberId)
        {
            long[] numArray = new long[0];
            if (memberId > 0L)
            {
                return (from item in this._iCartService.GetCart(memberId).Items select item.ProductId).ToArray<long>();
            }
            string cookie = WebHelper.GetCookie("HIMALL-CART");
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                string[] strArray = cookie.Split(new char[] { ',' });
                numArray = new long[strArray.Length];
                int num = 0;
                foreach (string str2 in strArray)
                {
                    string[] strArray2 = str2.Split(new char[] { ':' });
                    numArray[num++] = long.Parse(strArray2[0]);
                }
            }
            return numArray;

        }

        public IEnumerable<string> GetCartProductSKUIds(long memberId)
        {
            string[] strArray = new string[0];
            if (memberId > 0L)
            {
                return (from item in this._iCartService.GetCart(memberId).Items select item.SkuId).ToArray<string>();
            }
            string cookie = WebHelper.GetCookie("HIMALL-CART");
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                string[] strArray2 = cookie.Split(new char[] { ',' });
                strArray = new string[strArray2.Length];
                int num = 0;
                foreach (string str2 in strArray2)
                {
                    string[] strArray3 = str2.Split(new char[] { ':' });
                    strArray[num++] = strArray3[0];
                }
            }
            return strArray;

        }

        public ShoppingCartInfo GetCart(long memberId)
        {
            ShoppingCartInfo shoppingCartInfo;
            if (memberId > 0L)
            {
                shoppingCartInfo = this._iCartService.GetCart(memberId);
            }
            else
            {
                shoppingCartInfo = new ShoppingCartInfo();
                string cookie = WebHelper.GetCookie("HIMALL-CART");
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
                        shoppingCartItemArray[num++] = new ShoppingCartItem()
                        {
                            ProductId = long.Parse(strArray2[0].Split('_')[0]),
                            SkuId = strArray2[0],
                            Quantity = int.Parse(strArray2[1])
                        };
                    }
                    shoppingCartInfo.Items = (IEnumerable<ShoppingCartItem>)shoppingCartItemArray;
                }
            }
            return shoppingCartInfo;
        }

        public void RemoveFromCart(string skuId, long memberId)
        {
            if (memberId > 0L)
            {
                this._iCartService.DeleteCartItem(skuId, memberId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray = cookie.Split(',');
                    string str1 = string.Empty;
                    foreach (string str2 in strArray)
                    {
                        if (str2.Split(':')[0] != skuId)
                            str1 = str1 + "," + str2;
                    }
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART", str1);
                }
            }
        }

        public void RemoveFromCart(IEnumerable<string> skuIds, long memberId)
        {
            if (memberId > 0L)
            {
                this._iCartService.DeleteCartItem(skuIds, memberId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray = cookie.Split(',');
                    string str1 = string.Empty;
                    foreach (string str2 in strArray)
                    {
                        string str3 = str2.Split(':')[0];
                        if (!Enumerable.Contains<string>(skuIds, str3))
                            str1 = str1 + "," + str2;
                    }
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART", str1);
                }
            }
        }

        public void UpdateCartItem(string skuId, int count, long memberId)
        {
            if (memberId > 0L)
            {
                this._iCartService.UpdateCart(skuId, count, memberId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    string[] strArray = cookie.Split(',');
                    string str1 = string.Empty;
                    foreach (string str2 in strArray)
                    {
                        if (str2.Split(':')[0] == skuId)
                            str1 = str1 + (object)"," + skuId + ":" + (string)(object)count;
                        else
                            str1 = str1 + "," + str2;
                    }
                    if (!string.IsNullOrWhiteSpace(str1))
                        str1 = str1.Substring(1);
                    WebHelper.SetCookie("HIMALL-CART", str1);
                }
                else
                    WebHelper.SetCookie("HIMALL-CART", string.Format("{0}:{1}", (object)skuId, (object)count));
            }
        }
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        public void AddToCart(string skuId, int count, long memberId)
        {
            this.CheckSkuIdIsValid(skuId);
            SKUInfo sku = this._iProductService.GetSku(skuId);
            if (sku == null)
            {
                throw new HimallException("错误的SKU");
            }
            if (count > sku.Stock)
            {
                throw new HimallException("库存不足");
            }
            if (memberId > 0L)
            {
                this._iCartService.AddToCart(skuId, count, memberId);
            }
            else
            {
                string cookie = WebHelper.GetCookie("HIMALL-CART");
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    object obj2;
                    string[] strArray = cookie.Split(new char[] { ',' });
                    string str2 = string.Empty;
                    bool flag = false;
                    foreach (string str3 in strArray)
                    {
                        string[] strArray2 = str3.Split(new char[] { ':' });
                        if (strArray2[0] == skuId)
                        {
                            obj2 = str2;
                            str2 = string.Concat(new object[] { obj2, ",", skuId, ":", int.Parse(strArray2[1]) + count });
                            flag = true;
                        }
                        else
                        {
                            str2 = str2 + "," + str3;
                        }
                    }
                    if (!flag)
                    {
                        obj2 = str2;
                        str2 = string.Concat(new object[] { obj2, ",", skuId, ":", count });
                    }
                    if (!string.IsNullOrWhiteSpace(str2))
                    {
                        str2 = str2.Substring(1);
                    }
                    WebHelper.SetCookie("HIMALL-CART", str2);
                }
                else
                {
                    WebHelper.SetCookie("HIMALL-CART", string.Format("{0}:{1}", skuId, count));
                }
            }
        }

        private void CheckSkuIdIsValid(string skuId)
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