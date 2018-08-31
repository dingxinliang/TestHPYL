#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：Himall.DTO.Follow
* 项目描述 ：
* 类 名 称 ：Follow
* 类 描 述 ：
* 所在的域 ：QH-20160830FLFX
* 命名空间 ：Himall.DTO.Follow
* 机器名称 ：QH-20160830FLFX 
* CLR 版本 ：4.0.30319.42000
* 作    者 ：丁新亮
* 创建时间 ：2018/8/26 16:25:55
* 更新时间 ：2018/8/26 16:25:55
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

namespace Himall.DTO
{
   public class Follow
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long HTP_ID { get; set; }
        /// <summary>
        /// 类别ID
        /// </summary>
        public long HFT_ID { get; set; }
        /// <summary>
        /// 随访名称
        /// </summary>
        public string HTP_Name { get; set; }
        /// <summary>
        /// 启用状态
        /// </summary>
        public int HTP_State { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime HTP_CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public long HTP_UserId { get; set; }
    }
}
