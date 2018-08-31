#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.Application
* 项目描述 ：
* 类 名 称 ：FollowManagerApplication
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.Application
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 18:20:44
* 更新时间 ：2018/8/26 18:20:44
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.CommonModel;
using Himall.IServices.QueryModel;

namespace Himall.Application
{
   public class FollowManagerApplication
    {
        #region 字段 构造函数
        private static IServices.IFollowtService _followwService;

        static FollowManagerApplication()
        {
            _followwService = Himall.Core.ObjectContainer.Current.Resolve<Himall.IServices.IFollowtService>();

        }



        #endregion

        #region 方法

        public static QueryPageModel<FollowQuery> GetFollow(FollowSearch queryModel)
        {
            var data = _followwService.SearchFollow(queryModel);

            return new QueryPageModel<FollowQuery>()
            {
                Models = data.Models.ToList().Map<List<FollowQuery>>(),
                Total = data.Total
            };
            
        }
        public static void SaveFollow(string name, string cId, string ids, long shopId,long uid)
        {
            _followwService.SaveFollow(name, cId, ids ,shopId,uid);
        }
        public static void DeleteProduct(IEnumerable<long> ids, long shopId)
        {
            _followwService.DeleteProduct(ids, shopId);
        }

        public static void BatchOn(IEnumerable<long> ids, long shopId)
        {
            _followwService.BatchOn(ids, shopId);
        }

        public static void Creatdoctor(string cid, string title, string pic, string remark, string ids, long shopId, long id)
        {
            _followwService.Creatdoctor(cid,title, pic, remark, ids, shopId, id);
        }

        public static void BatchStop(IEnumerable<long> ids, long shopId)
        {
            _followwService.BatchStop(ids, shopId);
        }

        public static QueryPageModel<FollowContentQuery> GetFollowContent(FollowSearch queryModel, long id)
        {
           return  _followwService.GetFollowContent(queryModel, id);
        }

        public static void DeleteContent(IEnumerable<long> enumerable, long shopId)
        {
            _followwService.DeleteContent(enumerable, shopId);
        }

        public static void SaveFollowContent(string cId, string ids, long shopId, int day, string cont)
        {
            _followwService.SaveFollowContent(cId, ids, shopId, day, cont);
        }

        public static QueryPageModel<FollowDoctorQuery> GetFollowDoctor(FollowSearch queryModel)
        {
            return _followwService.GetFollowDoctor(queryModel);
        }

        public static void doctorStop(IEnumerable<long> ids, long shopId)
        {
            _followwService.doctorStop(ids, shopId);
        }

        public static void doctorOn(IEnumerable<long> ids, long shopId)
        {
            _followwService.doctorOn(ids, shopId);
        }

        public static void Deletedoctor(IEnumerable<long> ids, long shopId)
        {
            _followwService.Deletedoctor(ids, shopId);
        }
        #endregion

    }
}
