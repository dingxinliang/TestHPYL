using Himall.API.Helper;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.ServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Himall.API
{
    public class BranchShippingAddressController : BaseApiController
    {
        public object GetShippingAddressList(long shopBranchId)
        {
            if (shopBranchId == 0L)
                throw new HimallException("获取门店ID失败");
            this.CheckUserLogin();
            IQueryable<ShippingAddressInfo> shippingAddressByUserId = Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(this.CurrentUser.Id);
            List<ShippingAddressInfo> list = new List<ShippingAddressInfo>();
            ShopBranchInfo shopBranchById = Instance<IShopBranchService>.Create.GetShopBranchById(shopBranchId);
            if (shopBranchById == null || !shopBranchById.IsStoreDelive)
                throw new HimallException("门店ID错误,或不支持配送");
            foreach (ShippingAddressInfo shippingAddressInfo1 in (IEnumerable<ShippingAddressInfo>)shippingAddressByUserId)
            {
                int? serveRadius = shopBranchById.ServeRadius;
                if (serveRadius.HasValue)
                {
                    string fromLatLng = string.Format("{0},{1}", (object)shippingAddressInfo1.Latitude, (object)shippingAddressInfo1.Longitude);
                    if (fromLatLng.Length > 1)
                    {
                        double distancesFromApi = Instance<IShopBranchService>.Create.GetLatLngDistancesFromAPI(fromLatLng, string.Format("{0},{1}", (object)shopBranchById.Latitude, (object)shopBranchById.Longitude));
                        double num = distancesFromApi;
                        serveRadius = shopBranchById.ServeRadius;
                        if ((num >= (double)serveRadius.GetValueOrDefault() ? 0 : (serveRadius.HasValue ? 1 : 0)) != 0 && distancesFromApi != 0.0)
                            shippingAddressInfo1.CanDelive = true;
                    }
                }
                ShippingAddressInfo shippingAddressInfo2 = new ShippingAddressInfo()
                {
                    Id = shippingAddressInfo1.Id,
                    ShipTo = shippingAddressInfo1.ShipTo,
                    Phone = shippingAddressInfo1.Phone,
                    RegionFullName = shippingAddressInfo1.RegionFullName,
                    Address = shippingAddressInfo1.Address,
                    RegionId = shippingAddressInfo1.RegionId,
                    RegionIdPath = shippingAddressInfo1.RegionIdPath,
                    IsDefault = shippingAddressInfo1.IsDefault,
                    Latitude = shippingAddressInfo1.Latitude,
                    Longitude = shippingAddressInfo1.Longitude,
                    CanDelive = shippingAddressInfo1.CanDelive
                };
                list.Add(shippingAddressInfo2);
            }
            return (object)this.Json(new
            {
                Success = "true",
                ShippingAddress = list
            });
        }

        public object GetShippingAddress(long id, long shopBranchId)
        {
            if (shopBranchId == 0L)
                throw new HimallException("获取门店ID失败");
            this.CheckUserLogin();
            ShippingAddressInfo shippingAddressInfo = Queryable.FirstOrDefault<ShippingAddressInfo>(Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(this.CurrentUser.Id), (Expression<Func<ShippingAddressInfo, bool>>)(e => e.Id == id));
            ShopBranchInfo shopBranchById = Instance<IShopBranchService>.Create.GetShopBranchById(shopBranchId);
            if (shippingAddressInfo == null || shopBranchById == null || !shopBranchById.IsStoreDelive)
                return (object)this.Json(new
                {
                    Success = "true",
                    ShippingAddress = new ShippingAddressInfo()
                });
            int? serveRadius = shopBranchById.ServeRadius;
            if (serveRadius.HasValue)
            {
                string fromLatLng = string.Format("{0},{1}", (object)shippingAddressInfo.Latitude, (object)shippingAddressInfo.Longitude);
                if (fromLatLng.Length > 1)
                {
                    double distancesFromApi = Instance<IShopBranchService>.Create.GetLatLngDistancesFromAPI(fromLatLng, string.Format("{0},{1}", (object)shopBranchById.Latitude, (object)shopBranchById.Longitude));
                    double num = distancesFromApi;
                    serveRadius = shopBranchById.ServeRadius;
                    if ((num >= (double)serveRadius.GetValueOrDefault() ? 0 : (serveRadius.HasValue ? 1 : 0)) != 0 && distancesFromApi != 0.0)
                        shippingAddressInfo.CanDelive = true;
                }
            }
            return (object)this.Json(new
            {
                Success = "true",
                ShippingAddress = new ShippingAddressInfo()
                {
                    Id = shippingAddressInfo.Id,
                    ShipTo = shippingAddressInfo.ShipTo,
                    Phone = shippingAddressInfo.Phone,
                    RegionFullName = shippingAddressInfo.RegionFullName,
                    Address = shippingAddressInfo.Address,
                    RegionId = shippingAddressInfo.RegionId,
                    RegionIdPath = shippingAddressInfo.RegionIdPath,
                    Latitude = shippingAddressInfo.Latitude,
                    Longitude = shippingAddressInfo.Longitude,
                    CanDelive = shippingAddressInfo.CanDelive
                }
            });
        }

        public object PostAddShippingAddress(ShippingAddressAddModel value)
        {
            long? shopbranchid;
            int num;
            if (value.shopbranchid.HasValue)
            {
                shopbranchid = value.shopbranchid;
                num = shopbranchid.Value != 0L ? 1 : 0;
            }
            else
                num = 0;
            if (num == 0)
                throw new HimallException("获取门店ID失败");
            shopbranchid = value.shopbranchid;
            long id = shopbranchid.Value;
            this.CheckUserLogin();
            ShippingAddressInfo shipinfo = new ShippingAddressInfo();
            shipinfo.UserId = this.CurrentUser.Id;
            shipinfo.RegionId = value.regionId;
            shipinfo.Address = value.address;
            shipinfo.Phone = value.phone;
            shipinfo.ShipTo = value.shipTo;
            shipinfo.Latitude = new float?(value.latitude);
            shipinfo.Longitude = new float?(value.longitude);
            ShopBranchInfo shopBranchById = Instance<IShopBranchService>.Create.GetShopBranchById(id);
            if (shopBranchById == null || !shopBranchById.IsStoreDelive)
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "门店不提供配送服务"
                });
            if (shopBranchById.ServeRadius.HasValue)
            {
                string fromLatLng = string.Format("{0},{1}", (object)shipinfo.Latitude, (object)shipinfo.Longitude);
                if (fromLatLng.Length <= 1)
                    return (object)this.Json(new
                    {
                        Success = "false",
                        Msg = "地址经纬度获取失败"
                    });
                double distancesFromApi = Instance<IShopBranchService>.Create.GetLatLngDistancesFromAPI(fromLatLng, string.Format("{0},{1}", (object)shopBranchById.Latitude, (object)shopBranchById.Longitude));
                int? serveRadius = shopBranchById.ServeRadius;
                if ((distancesFromApi <= (double)serveRadius.GetValueOrDefault() ? 0 : (serveRadius.HasValue ? 1 : 0)) != 0)
                    return (object)this.Json(new
                    {
                        Success = "false",
                        Msg = "距离超过门店配送距离的不可配送"
                    });
            }
            try
            {
                Instance<IShippingAddressService>.Create.AddShippingAddress(shipinfo);
            }
            catch (Exception ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = ex.Message
                });
            }
            return (object)this.Json(new
            {
                Success = "true"
            });
        }

        public object PostDeleteShippingAddress(ShippingAddressDeleteModel value)
        {
            this.CheckUserLogin();
            Instance<IShippingAddressService>.Create.DeleteShippingAddress(value.id, this.CurrentUser.Id);
            return (object)this.Json(new
            {
                Success = "true"
            });
        }

        public object PostEditShippingAddress(ShippingAddressEditModel value)
        {
            long? shopbranchid;
            int num;
            if (value.shopbranchid.HasValue)
            {
                shopbranchid = value.shopbranchid;
                num = shopbranchid.Value != 0L ? 1 : 0;
            }
            else
                num = 0;
            if (num == 0)
                throw new HimallException("获取门店ID失败");
            shopbranchid = value.shopbranchid;
            long id = shopbranchid.Value;
            this.CheckUserLogin();
            ShippingAddressInfo shipinfo = new ShippingAddressInfo();
            shipinfo.UserId = this.CurrentUser.Id;
            shipinfo.Id = value.id;
            shipinfo.RegionId = value.regionId;
            shipinfo.Address = value.address;
            shipinfo.Phone = value.phone;
            shipinfo.ShipTo = value.shipTo;
            shipinfo.Longitude = new float?(value.longitude);
            shipinfo.Latitude = new float?(value.latitude);
            ShopBranchInfo shopBranchById = Instance<IShopBranchService>.Create.GetShopBranchById(id);
            if (shopBranchById == null || !shopBranchById.IsStoreDelive)
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = "门店不提供配送服务"
                });
            if (shopBranchById.ServeRadius.HasValue)
            {
                string fromLatLng = string.Format("{0},{1}", (object)shipinfo.Latitude, (object)shipinfo.Longitude);
                if (fromLatLng.Length <= 1)
                    return (object)this.Json(new
                    {
                        Success = "false",
                        Msg = "收货地址经纬度获取失败"
                    });
                double distancesFromApi = Instance<IShopBranchService>.Create.GetLatLngDistancesFromAPI(fromLatLng, string.Format("{0},{1}", (object)shopBranchById.Latitude, (object)shopBranchById.Longitude));
                int? serveRadius = shopBranchById.ServeRadius;
                if ((distancesFromApi <= (double)serveRadius.GetValueOrDefault() ? 0 : (serveRadius.HasValue ? 1 : 0)) != 0)
                    return (object)this.Json(new
                    {
                        Success = "false",
                        Msg = "距离超过门店配送距离的不可配送"
                    });
            }
            try
            {
                Instance<IShippingAddressService>.Create.UpdateShippingAddress(shipinfo);
            }
            catch (Exception ex)
            {
                return (object)this.Json(new
                {
                    Success = "false",
                    Msg = ex.Message
                });
            }
            return (object)this.Json(new
            {
                Success = "true"
            });
        }

        public object PostSetDefaultAddress(ShippingAddressSetDefaultModel value)
        {
            this.CheckUserLogin();
            Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(value.addId, this.CurrentUser.Id);
            return (object)this.Json(new
            {
                Success = "true"
            });
        }

        public object GetRegion(string fromLatLng = "")
        {
            string address = string.Empty;
            string province = string.Empty;
            string city = string.Empty;
            string district = string.Empty;
            string street = string.Empty;
            string str = string.Empty;
            string newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            string addressComponents = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (addressComponents.Split(',').Length <= 3)
                newStreet = string.Empty;
            return (object)this.Json(new
            {
                fullPath = addressComponents,
                showCity = string.Format("{0} {1} {2}", (object)province, (object)city, (object)district),
                street = newStreet
            });
        }
    }
}
