#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.Model.Models
* 项目描述 ：
* 类 名 称 ：FollowInfo
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.Model.Models
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 12:55:06
* 更新时间 ：2018/8/26 12:55:06
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ Administrator 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.Models
{
   public class FollowInfo
    {
        public enum FollowInfoAuditStatus
        {
            /// <summary>
            /// 未使用
            /// </summary>
            [Description("未使用")]
            WaitForAuditing = 0,

            /// <summary>
            /// 已启用
            /// </summary>
            [Description("已启用")]
            Audited=1
        }

    }
}
