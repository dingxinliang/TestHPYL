using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace Himall.Web.Common
{
    /// <summary>
    /// 门店相关操作
    /// </summary>
    public class ShopbranchHelper
    {
        /// <summary>
        /// 通过坐标获取地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static GaodeGetAddressByLatLngResult GetAddressByLatLng(string latLng, ref string address, ref string province, ref string city, ref string district, ref string street)
        {
            GaodeGetAddressByLatLngResult result;
            string str3;
            string[] strArray = latLng.Split(new char[] { ',' });
            string str = strArray[1] + "," + strArray[0];
            string fromLatLng = Math.Round(decimal.Parse(strArray[1]), 4) + "," + Math.Round(decimal.Parse(strArray[0]), 4);
            object obj2 = Cache.Get(CacheKeyCollection.LatlngCacheKey(fromLatLng));
            if (obj2 != null)
            {
                result = (GaodeGetAddressByLatLngResult)obj2;
                if ((result.status == 1) && (result.info == "OK"))
                {
                    province = result.regeocode.addressComponent.province;
                    city = string.IsNullOrEmpty(result.regeocode.addressComponent.city) ? result.regeocode.addressComponent.province : result.regeocode.addressComponent.city;
                    district = result.regeocode.addressComponent.district;
                    street = result.regeocode.addressComponent.township;
                    if (string.IsNullOrEmpty(result.regeocode.addressComponent.building.name))
                    {
                        address = result.regeocode.addressComponent.neighborhood.name;
                    }
                    else
                    {
                        address = result.regeocode.addressComponent.building.name;
                    }
                    if (string.IsNullOrEmpty(address))
                    {
                        str3 = province + result.regeocode.addressComponent.city + district + street;
                        address = result.regeocode.formatted_address.Remove(0, str3.Length);
                    }
                }
                return result;
            }
            string str4 = "53e4f77f686e6a2b5bf53521e178c6e7";
            result = ParseFormJson<GaodeGetAddressByLatLngResult>(GetResponseResult("http://restapi.amap.com/v3/geocode/regeo?output=json&radius=3000&location=" + str + "&key=" + str4));
            if ((result.status == 1) && (result.info == "OK"))
            {
                DateTime cacheTime = DateTime.Now.AddDays(1.0);
                Cache.Insert<GaodeGetAddressByLatLngResult>(CacheKeyCollection.LatlngCacheKey(fromLatLng), result, cacheTime);
                province = result.regeocode.addressComponent.province;
                city = string.IsNullOrWhiteSpace(result.regeocode.addressComponent.city) ? result.regeocode.addressComponent.province : result.regeocode.addressComponent.city;
                district = result.regeocode.addressComponent.district;
                street = result.regeocode.addressComponent.township;
                if (string.IsNullOrWhiteSpace(result.regeocode.addressComponent.building.name))
                {
                    address = result.regeocode.addressComponent.neighborhood.name;
                }
                else
                {
                    address = result.regeocode.addressComponent.building.name;
                }
                if (string.IsNullOrWhiteSpace(address))
                {
                    str3 = province + result.regeocode.addressComponent.city + district + street;
                    address = result.regeocode.formatted_address.Remove(0, str3.Length);
                }
            }
            return result;

        }

        /// <summary>
        /// 把JSON字符串还原为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="szJson">JSON字符串</param>
        /// <returns>对象实体</returns>
        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }

        /// <summary>
        /// 获取Web请求返回的字符串数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetResponseResult(string url)
        {
            string result;
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)req.GetResponse())
                {
                    using (Stream receiveStream = response.GetResponseStream())
                    {

                        using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                        {
                            result = readerOfStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("根据经纬度获取地理位置异常",ex);
                result = "";
            }
            return result;
        }
    }
}