﻿using System;
using System.Collections.Generic;
using Himall.IServices;
using AutoMapper;
using Himall.Core;
using Himall.DTO;
using Himall.Core.Plugins.Message;
using Himall.Model;
using Himall.IServices.QueryModel;
using Himall.CommonModel;

namespace Himall.Application
{
    public class ShopApplication
    {
       
        private static IShopService _iShopService = ObjectContainer.Current.Resolve<IShopService>();
        private static IAppMessageService _appMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();

        #region 诊所入驻设置
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="mSettled"></param>
        public static void Settled(Himall.DTO.Settled mSettled)
        {
            Mapper.CreateMap<Himall.DTO.Settled, Himall.Model.SettledInfo>();
            var model = Mapper.Map<Himall.DTO.Settled, Himall.Model.SettledInfo>(mSettled);
            if (model.ID > 0)
            {
                SettledApplication.UpdateSettled(model);
            }
            else
            {
                SettledApplication.AddSettled(model);
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public static Himall.DTO.Settled GetSettled()
        {
            Himall.DTO.Settled mSettled = new Settled();
            Himall.Model.SettledInfo mSettledInfo = SettledApplication.GetSettled();
            if (mSettledInfo != null)
            {
                Mapper.CreateMap<Himall.Model.SettledInfo, Himall.DTO.Settled>();
                mSettled = Mapper.Map<Himall.Model.SettledInfo, Himall.DTO.Settled>(mSettledInfo);
            }
            return mSettled;
        }
        #endregion

        #region 诊所入驻流程

        /// <summary>
        /// 添加诊所管理员
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static Himall.DTO.Manager AddSellerManager(string username, string password, string salt)
        {
            var model = ManagerApplication.AddSellerManager(username, password, salt);
            Himall.DTO.Manager mManagerInfo = new Manager()
            {
                Id = model.Id
            };
            return mManagerInfo;
        }

        /// <summary>
        /// 获取诊所信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="businessCategoryOn"></param>
        /// <returns></returns>
        public static Himall.DTO.Shop GetShop(long id, bool businessCategoryOn = false)
        {
            if(Cache.Exists(CacheKeyCollection.CACHE_SHOPDTO(id, businessCategoryOn)))
                return Cache.Get<Himall.DTO.Shop>(CacheKeyCollection.CACHE_SHOPDTO(id, businessCategoryOn));

            var model = _iShopService.GetShop(id, businessCategoryOn);
            Himall.DTO.Shop mShop = new Shop();
            if (model != null)
            {
                mShop = Mapper.Map<Himall.Model.ShopInfo, Himall.DTO.Shop>(model);
            }
            Cache.Insert<Himall.DTO.Shop>(CacheKeyCollection.CACHE_SHOPDTO(id, businessCategoryOn), mShop, 600);
            return mShop;
        }

        public static ShopInfo GetSelfShop()
        {
            if (Cache.Exists(CacheKeyCollection.CACHE_SELFSHOP))
                return Cache.Get<ShopInfo>(CacheKeyCollection.CACHE_SELFSHOP);

            var model = _iShopService.GetSelfShop();
            Cache.Insert<ShopInfo>(CacheKeyCollection.CACHE_SELFSHOP, model, 600);
            return model;
        }

		/// <summary>
		/// 根据id获取门店
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<DTO.Shop> GetShops(IEnumerable<long> ids)
		{
			var list = _iShopService.GetShops(ids);
			return AutoMapper.Mapper.Map<List<DTO.Shop>>(list);
		}

		/// <summary>
		/// 获取诊所信息（以分页的形式展示）
		/// </summary>
		/// <param name="shopQueryModel">ShopQuery对象</param>
		/// <returns></returns>
		public static QueryPageModel<DTO.Shop> GetShops(ShopQuery shopQueryModel)
		{
			var data= _iShopService.GetShops(shopQueryModel);

			return new QueryPageModel<DTO.Shop>()
			{
				Models = data.Models.Map<List<DTO.Shop>>(),
				Total = data.Total
			};
		}
		
		/// <summary>
		/// 获取诊所名称
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string GetShopName(long id)
		{
			return _iShopService.GetShopName(id);
		}
		
		/// <summary>
		/// 获取诊所名称
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static Dictionary<long, string> GetShopNames(IEnumerable<long> ids)
		{
			return _iShopService.GetShopNames(ids);
		}

        public static ShopInfo GetShopInfo(long id, bool businessCategoryOn = false)
        {
            var model = _iShopService.GetShop(id, businessCategoryOn);
            return model;
        }
        /// <summary>
        /// 诊所入驻第二部
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Himall.DTO.ShopProfileStep1 GetShopProfileStep1(long id, out long CompanyRegionId, out long BusinessLicenceRegionId, out string RefuseReason)
        {
            var shop = _iShopService.GetShop(id);

            var step1 = new Himall.DTO.ShopProfileStep1();
            step1.Address = shop.CompanyAddress;


            step1.BusinessLicenceArea = shop.BusinessLicenceRegionId;
            step1.BusinessLicenceNumber = shop.BusinessLicenceNumber;
            step1.BusinessLicenceNumberPhoto = shop.BusinessLicenceNumberPhoto;
            if (shop.BusinessLicenceEnd.HasValue)
                step1.BusinessLicenceValidEnd = shop.BusinessLicenceEnd.Value;

            if (shop.BusinessLicenceStart.HasValue)
                step1.BusinessLicenceValidStart = shop.BusinessLicenceStart.Value;
            string BusinessLicenseCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (HimallIO.ExistFile(shop.BusinessLicenseCert + string.Format("{0}.png", i)))
                {
                    BusinessLicenseCert += shop.BusinessLicenseCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.BusinessLicenseCert = BusinessLicenseCert.TrimEnd(',');
            step1.BusinessSphere = shop.BusinessSphere;
            step1.CityRegionId = shop.CompanyRegionId;
            if (shop.CompanyFoundingDate.HasValue)
                step1.CompanyFoundingDate = shop.CompanyFoundingDate.Value;
            step1.CompanyName = shop.CompanyName;
            step1.ContactName = shop.ContactsName;
            step1.ContactPhone = shop.ContactsPhone;
            step1.Email = shop.ContactsEmail;
            step1.EmployeeCount = shop.CompanyEmployeeCount;
            step1.GeneralTaxpayerPhoto = shop.GeneralTaxpayerPhot;
            step1.legalPerson = shop.legalPerson;
            step1.OrganizationCode = shop.OrganizationCode;
            step1.OrganizationCodePhoto = shop.OrganizationCodePhoto;
            step1.BusinessType = shop.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : shop.BusinessType.Value;

            string OtherCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (HimallIO.ExistFile(shop.OtherCert + string.Format("{0}.png", i)))
                {
                    OtherCert += shop.OtherCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.OtherCert = OtherCert.TrimEnd(',');
            step1.Phone = shop.CompanyPhone;

            string ProductCert = string.Empty;
            for (int i = 1; i < 4; i++)
            {
                if (HimallIO.ExistFile(shop.ProductCert + string.Format("{0}.png", i)))
                {
                    ProductCert += shop.ProductCert + string.Format("{0}.png", i) + ",";
                }
            }
            step1.ProductCert = ProductCert.TrimEnd(',');
            step1.RegisterMoney = shop.CompanyRegisteredCapital;
            step1.taxRegistrationCert = shop.TaxRegistrationCertificate;
            step1.Settled = GetSettled();

            CompanyRegionId = shop.CompanyRegionId;
            BusinessLicenceRegionId = shop.BusinessLicenceRegionId;
            RefuseReason = null;
            if (shop.ShopStatus == Himall.Model.ShopInfo.ShopAuditStatus.Refuse) RefuseReason = shop.RefuseReason;

            return step1;
        }

        /// <summary>
        /// 个人入驻第二部信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="CompanyRegionId"></param>
        /// <param name="BusinessLicenceRegionId"></param>
        /// <param name="RefuseReason"></param>
        /// <returns></returns>
        public static Himall.DTO.ShopProfileSteps1 GetShopProfileSteps1(long id, out long CompanyRegionId, out long BusinessLicenceRegionId, out string RefuseReason)
        {
            var shop = _iShopService.GetShop(id);

            var step1 = new Himall.DTO.ShopProfileSteps1();
            step1.Address = shop.CompanyAddress;

            step1.CityRegionId = shop.CompanyRegionId;
            step1.CompanyName = shop.CompanyName;

            step1.IDCard = shop.IDCard;
            step1.IDCardUrl = shop.IDCardUrl;
			step1.IDCardUrl2 = shop.IDCardUrl2;
            step1.BusinessType = shop.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : shop.BusinessType.Value;
            step1.Settled = GetSettled();

            CompanyRegionId = shop.CompanyRegionId;
            BusinessLicenceRegionId = shop.BusinessLicenceRegionId;
            RefuseReason = null;
            if (shop.ShopStatus == Himall.Model.ShopInfo.ShopAuditStatus.Refuse) RefuseReason = shop.RefuseReason;

            return step1;
        }
        /// <summary>
        /// 获取诊所入驻第三部信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Himall.DTO.ShopProfileStep2 GetShopProfileStep2(long id, out Himall.Model.ShopInfo.ShopStage Stage)
        {
            var shop = _iShopService.GetShop(id);
            var model = new Himall.DTO.ShopProfileStep2()
            {
                BankAccountName = shop.BankAccountName == null ? "" : shop.BankAccountName,
                BankAccountNumber = shop.BankAccountNumber == null ? "" : shop.BankAccountNumber,
                BankCode = shop.BankCode == null ? "" : shop.BankCode,
                BankName = shop.BankName == null ? "" : shop.BankName,
                BankPhoto = shop.BankPhoto == null ? "" : shop.BankPhoto,
                BankRegionId = shop.BankRegionId,
                TaxpayerId = shop.TaxpayerId == null ? "" : shop.TaxpayerId,
                TaxRegistrationCertificate = shop.TaxRegistrationCertificate == null ? "" : shop.TaxRegistrationCertificate,
                TaxRegistrationCertificatePhoto = shop.TaxRegistrationCertificatePhoto == null ? "" : shop.TaxRegistrationCertificatePhoto,
                WeiXinAddress = shop.WeiXinAddress == null ? "" : shop.WeiXinAddress,
                WeiXinNickName = shop.WeiXinNickName == null ? "" : shop.WeiXinNickName,
                WeiXinOpenId = shop.WeiXinOpenId == null ? "" : shop.WeiXinOpenId,
                WeiXinSex = shop.WeiXinSex == null ? 0 : shop.WeiXinSex.Value,
                WeiXinTrueName = shop.WeiXinTrueName == null ? "" : shop.WeiXinTrueName,
                BusinessType = shop.BusinessType == null ? Himall.CommonModel.ShopBusinessType.Enterprise : shop.BusinessType.Value,
                Settled = GetSettled()
            };
            Stage = shop.Stage.Value;
            return model;
        }

        /// <summary>
        /// 诊所入驻协议
        /// </summary>
        /// <returns></returns>
        public static string GetSellerAgreement()
        {
            Himall.Model.AgreementInfo model = SystemAgreementApplication.GetAgreement(Himall.Model.AgreementInfo.AgreementTypes.Seller);
            if (model != null)
                return model.AgreementContent;
            else
                return "";
        }

        /// <summary>
        /// 注册协议
        /// </summary>
        /// <returns></returns>
        public static string GetBuyersAgreement()
        {
            Himall.Model.AgreementInfo model = SystemAgreementApplication.GetAgreement(Himall.Model.AgreementInfo.AgreementTypes.Buyers);
            if (model != null)
                return model.AgreementContent;
            else
                return "";
        }

        /// <summary>
        /// 诊所入驻诊所信息更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ShopId"></param>
        /// <returns>0、失败；1、成功；-1、诊所名称已经存在</returns>
        public static int UpdateShop(Himall.DTO.ShopProfileStep3 model, long ShopId)
        {
            int result = 0;
            if (_iShopService.ExistShop(model.ShopName, ShopId))
            {
                result = -1;
            }
            else
            {
                Himall.Model.ShopInfo shopInfo = _iShopService.GetShop(ShopId);
                shopInfo.Id = ShopId;
                shopInfo.ShopName = model.ShopName;
                shopInfo.GradeId = model.ShopGrade;
                shopInfo.Stage = Himall.Model.ShopInfo.ShopStage.UploadPayOrder;
                IEnumerable<long> shopCategories = model.Categories;
                _iShopService.UpdateShop(shopInfo, shopCategories);
                Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(ShopId,false));
                Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(ShopId,true));
                result = 1;
            }
            return result;
        }


        #endregion

        #region 诊所信息

        /// <summary>
        /// 商店信息更新
        /// </summary>
        /// <param name="model"></param>
        public static void UpdateShop(Himall.DTO.Shop model)
        {
            var mShop = Mapper.Map<Himall.DTO.Shop, Himall.Model.ShopInfo>(model);
            _iShopService.UpdateShop(mShop);
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(model.Id, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(model.Id, true));
        }



        /// <summary>
        /// 判断公司名称是否存在
        /// </summary>
        /// <param name="companyName">公司名字</param>
        /// <param name="shopId"></param>
        public static bool ExistCompanyName(string companyName, long shopId = 0)
        {
            return _iShopService.ExistCompanyName(companyName, shopId);
        }

        /// <summary>
        /// 检测营业执照号是否重复
        /// </summary>
        /// <param name="BusinessLicenceNumber">营业执照号</param>
        /// <param name="shopId"></param>
        public static bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0)
        {
            return _iShopService.ExistBusinessLicenceNumber(BusinessLicenceNumber, shopId);
        }

        /// <summary>
        /// 判断诊所名名称是否存在
        /// </summary>
        /// <param name="shopName">公司名字</param>
        /// <param name="shopId"></param>
        public static bool ExistShop(string shopName, long shopId = 0)
        {
            return _iShopService.ExistShop(shopName, shopId);
        }

        /// <summary>
        /// 获取诊所等级列表
        /// </summary>
        /// <returns></returns>
        public static List<Himall.DTO.ShopGrade> GetShopGrades()
        {
            List<Himall.DTO.ShopGrade> lmShopGrade = new List<ShopGrade>();
            var model = _iShopService.GetShopGrades();
            foreach (var item in model)
            {
                Mapper.CreateMap<Himall.Model.ShopGradeInfo, Himall.DTO.ShopGrade>();
                lmShopGrade.Add(Mapper.Map<Himall.Model.ShopGradeInfo, Himall.DTO.ShopGrade>(item));
            }
            return lmShopGrade;
        }

		/// <summary>
		/// 获取诊所账户信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public static List<ShopAccount> GetShopAccounts(IEnumerable<long> ids)
		{
			return _iShopService.GetShopAccounts(ids).Map<List<DTO.ShopAccount>>();
		}

        /// <summary>
        /// 获取诊所经营项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Himall.DTO.BusinessCategory> GetBusinessCategory(long id)
        {
            List<Himall.DTO.BusinessCategory> lvBusinessCategory = new List<BusinessCategory>();
            var model = _iShopService.GetBusinessCategory(id);
            foreach (var item in model)
            {
                lvBusinessCategory.Add(new Himall.DTO.BusinessCategory()
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    ShopId = item.ShopId
                });
            }
            return lvBusinessCategory;
        }

        /// <summary>
        /// 获取单个入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SettledPayment GetSettledPaymentRecord(long id)
        {
            var item = _iShopService.GetShopRenewRecord(id);
            var shopName = _iShopService.GetShop(item.ShopId).ShopName;
            SettledPayment model = new SettledPayment();
            model.Id = item.Id;
            model.OperateType = item.OperateType.ToDescription();
            model.OperateDate = item.OperateDate.ToString("yyyy-MM-dd HH:mm");
            model.Operate = item.Operator;
            model.Content = item.OperateContent;
            model.Amount = item.Amount;
            model.ShopName = shopName;
            return model;
        }


        /// <summary>
        /// 修改诊所银行帐户
        /// </summary>
        /// <param name="bankAccount"></param>
        public static void UpdateBankAccount(Himall.DTO.BankAccount bankAccount)
        {
            Himall.Model.ShopInfo shopInfo = _iShopService.GetShop(bankAccount.ShopId);
            shopInfo.BankAccountName = bankAccount.BankAccountName;
            shopInfo.BankAccountNumber = bankAccount.BankAccountNumber;
            shopInfo.BankCode = bankAccount.BankCode;
            shopInfo.BankName = bankAccount.BankName;
            shopInfo.BankRegionId = bankAccount.BankRegionId;
            _iShopService.UpdateShop(shopInfo);
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(bankAccount.ShopId, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(bankAccount.ShopId, true));
        }

        /// <summary>
        /// 修改诊所微信帐户
        /// </summary>
        /// <param name="weChatAccount"></param>
        public static void UpdateWeChatAccount(Himall.DTO.WeChatAccount weChatAccount)
        {
            Himall.Model.ShopInfo shopInfo = _iShopService.GetShop(weChatAccount.ShopId);
            shopInfo.WeiXinOpenId = weChatAccount.WeiXinOpenId;
            shopInfo.WeiXinSex = weChatAccount.Sex.Equals("男") ? 1 : 0;
            shopInfo.WeiXinTrueName = weChatAccount.WeiXinRealName;
            shopInfo.WeiXinNickName = weChatAccount.WeiXinNickName;
            shopInfo.WeiXinAddress = weChatAccount.Address;
            shopInfo.WeiXinImg = weChatAccount.Logo;
            _iShopService.UpdateShop(shopInfo);
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(weChatAccount.ShopId, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(weChatAccount.ShopId, true));

        }
        #endregion


        #region 管理员认证
        /// <summary>
        /// 获取诊所认证情况
        /// </summary>
        /// <param name="ShopId">诊所ID</param>
        /// <returns></returns>
        public static Himall.DTO.MemberAccountSafety GetShopAccountSafety(long ShopId)
        {
            Himall.DTO.MemberAccountSafety model = new Himall.DTO.MemberAccountSafety();
            long UserId = _iShopService.GetShopManagers(ShopId);
            model.UserId = UserId;
            List<Himall.Model.MemberContactsInfo> lmMemberContactsInfo = MessageApplication.GetMemberContactsInfo(UserId);

            foreach (Himall.Model.MemberContactsInfo item in lmMemberContactsInfo)
            {
                if (item.ServiceProvider.Contains("SMS"))
                {
                    model.Phone = item.Contact;
                    model.BindPhone = true;
                }
                else if (item.ServiceProvider.Contains("Email"))
                {
                    model.Email = item.Contact;
                    model.BindEmail = true;
                }
            }

            return model;
        }

        /// <summary>
        /// 获取诊所管理员ID
        /// </summary>
        /// <param name="ShopId">诊所ID</param>
        /// <returns></returns>
        public static long GetShopManagers(long ShopId)
        {
            long UserId = _iShopService.GetShopManagers(ShopId);
            return UserId;
        }

        /// <summary>
        /// 发送验证码，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool SendShopCode(string pluginId, string destination, string UserName, string SiteName)
        {
            var timeout = CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Get(timeout) != null)
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(UserName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginReBindTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// 验证码验证，认证管理员
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckShopCode(string pluginId, string code, string destination, long userId)
        {
            var member = MemberApplication.GetMembers(userId);
            int result = 0;
            var cache = CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                if (MessageApplication.GetMemberContactsInfo(pluginId, destination, Himall.Model.MemberContactsInfo.UserTypes.General) != null)
                {
                    result = -1;
                }
                else
                {
                    if (pluginId.ToLower().Contains("email"))
                    {
                        member.Email = destination;
                    }
                    else if (pluginId.ToLower().Contains("sms"))
                    {
                        member.CellPhone = destination;
                    }

                    MemberApplication.UpdateMember(member);

                    MessageApplication.UpdateMemberContacts(new Himall.Model.MemberContactsInfo() { Contact = destination, ServiceProvider = pluginId, UserId = userId, UserType = Himall.Model.MemberContactsInfo.UserTypes.General });

                    Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(member.UserName, pluginId));
                    Core.Cache.Remove(CacheKeyCollection.Member(userId));//移除用户缓存
                    Core.Cache.Remove("Rebind" + userId);
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static bool IsSelfShop(long shopId)
        {
            return _iShopService.IsSelfShop(shopId);
        }



        #endregion
        /// <summary>
        /// 检查、初始化诊所模版
        /// </summary>
        /// <param name="shopid"></param>
        public static void CheckInitTemplate(long shopid)
        {
            _iShopService.CheckInitTemplate(shopid);
        }


        #region 验证码

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="UserName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool SendCode(string pluginId, string destination, string UserName, string SiteName)
        {
            var timeout = CacheKeyCollection.ShopPluginAuthenticateTime(UserName, pluginId); //验证码超时时间
            if (Core.Cache.Get(timeout) != null)
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
            Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginAuthenticate(UserName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = UserName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginAuthenticateTime(UserName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }


        /// <summary>
        /// 验证码验证
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int CheckCode(string pluginId, string code, string destination, string userName)
        {
            int result = 0;
            var cache = CacheKeyCollection.ShopPluginAuthenticate(userName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                Core.Cache.Remove(CacheKeyCollection.ShopPluginAuthenticate(userName, pluginId + destination));
                result = 1;
            }
            return result;
        }


        /// <summary>
        /// (绑定银行卡)发送验证码
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userName">会员账号</param>
        /// <param name="SiteName">站点设置</param>
        /// <returns></returns>
        public static bool BindBankSendCode(string pluginId, string destination, string userName, string SiteName)
        {
            var timeout = CacheKeyCollection.ShopPluginBindBankTime(userName, pluginId); //验证码超时时间
            if (Core.Cache.Get(timeout) != null)
            {
                return false;
            }
            var checkCode = new Random().Next(10000, 99999);
           // Log.Debug("Code:" + checkCode);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination), checkCode, cacheTimeout);
            var user = new MessageUserInfo() { UserName = userName, SiteName = SiteName, CheckCode = checkCode.ToString() };
            MessageApplication.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.ShopPluginBindBankTime(userName, pluginId), "0", DateTime.Now.AddSeconds(110));//验证码超时时间
            return true;
        }

        /// <summary>
        /// （绑定银行卡）验证码验证
        /// </summary>
        /// <param name="pluginId">信息类别</param>
        /// <param name="code">验证码</param>
        /// <param name="destination">联系号码</param>
        /// <param name="userId">会员ID</param>
        /// <returns></returns>
        public static int BindBankCheckCode(string pluginId, string code, string destination, string userName)
        {
            int result = 0;
            var cache = CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination);
            var cacheCode = Core.Cache.Get(cache);
            if (cacheCode != null && cacheCode.ToString() == code)
            {
                Core.Cache.Remove(CacheKeyCollection.ShopPluginBindBank(userName, pluginId + destination));
                result = 1;
            }
            return result;
        }

        #endregion
        /// <summary>
        /// 获取指定诊所等级信息
        /// </summary>
        /// <param name="id">诊所等级Id</param>
        /// <returns></returns>
        public static ShopGrade GetShopGrade(long id)
        {
            Mapper.CreateMap<Himall.Model.ShopGradeInfo, Himall.DTO.ShopGrade>();
            return _iShopService.GetShopGrade(id).Map<DTO.ShopGrade>();
        }
    }
}
