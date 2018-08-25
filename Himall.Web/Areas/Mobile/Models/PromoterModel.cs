using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Himall.Model;

namespace Himall.Web.Areas.Mobile
{
    public class PromoterModel
    {
        public UserMemberInfo Member { get; set; }

        public RecruitSettingInfo RecruitSetting { get; set; }

        public string RegionPath { get; set; }

        public bool IsBindMobile { get; set; }

        public bool IsRefused { get; set; }

        public string ShopName { get; set; }

        public PromoterInfo.PromoterStatus Status { get; set; }

        public bool IsHavePostData { get; set; }
    }
}