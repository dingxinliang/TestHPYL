using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.IServices.QueryModel
{
    public class ApplyWithDrawQuery:QueryBase
    {
        public ApplyWithDrawInfo.ApplyWithDrawStatus? withDrawStatus{get;set;}

        public long? memberId{get;set;}

        public long? withDrawNo { get; set; }

        public UserWithdrawType? ApplyType { get; set; }

    }
}
