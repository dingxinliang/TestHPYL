#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.IServices.DXL
* 项目描述 ：
* 类 名 称 ：FollowtService
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.IServices.DXL
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 18:22:58
* 更新时间 ：2018/8/26 18:22:58
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using Himall.CommonModel;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices
{
    public interface IFollowtService : IService
    {
        ObsoletePageModel<FollowQuery> SearchFollow(FollowSearch search);
        void DeleteProduct(IEnumerable<long> ids, long shopId);
        void BatchOn(IEnumerable<long> ids, long shopId);
        void BatchStop(IEnumerable<long> ids, long shopId);
        void SaveFollow(string name, string cId, string ids, long shopId,long uid);
        QueryPageModel<FollowContentQuery> GetFollowContent(FollowSearch queryModel, long id);
        QueryPageModel<FollowPlan> GetFollowPan(FollowPlanQuery queryModel);
        void DeleteContent(IEnumerable<long> enumerable, long shopId);
        void SaveFollowContent(string cId, string ids, long shopId, int day, string cont);
        QueryPageModel<FollowDoctorQuery> GetFollowDoctor(FollowSearch queryModel);
        void doctorStop(IEnumerable<long> ids, long shopId);
        void doctorOn(IEnumerable<long> ids, long shopId);
        void Deletedoctor(IEnumerable<long> ids, long shopId);
        void Creatdoctor(string cid, string title,string pic, string remark, string ids, long shopId, long id);
        List<FollowDoctorQuery> GetDoctor(string ids, long shopId);
        void DeletedoctorContent(IEnumerable<long> ids, long shopId);
        void DeleteFollowPlan(IEnumerable<long> ids, long shopId);
        QueryPageModel<DoctorContentQuery> ListDoctorContent(FollowSearch queryModel, long id);
        void CreatedoctorContent(string cId, string ids, long shopId, int day, string cont);
        void CancelFollowPlan(IEnumerable<long> ids, long shopId);
        void DownFollowPlan(IEnumerable<long> ids, long shopId, string LastUser, string result);
        FollowPlanView UserFollowPlan(IEnumerable<long> ids);
    }
}
