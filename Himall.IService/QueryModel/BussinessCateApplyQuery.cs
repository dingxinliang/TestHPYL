using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.IServices.QueryModel
{
    public class BussinessCateApplyQuery : QueryBase
    {
        /// <summary>
        /// 申请的诊所
        /// </summary>
        public long? shopId { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public Model.BusinessCategoriesApplyInfo.BusinessCateApplyStatus? Status { set; get; }

        /// <summary>
        /// 申请的诊所
        /// </summary>
        public string ShopName{ get; set; }
    }
}
