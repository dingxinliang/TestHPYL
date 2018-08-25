using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FightGroupActiveItemInfo
    {
        /// <summary>
        /// �������
        /// </summary>
        [NotMapped]
        public string SkuName { get; set; }
        /// <summary>
        /// ��Ʒ�ۼ�
        /// </summary>
        [NotMapped]
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// ��Ʒ�ɱ���
        /// </summary>
        [NotMapped]
        public decimal ProductCostPrice { get; set; }
        /// <summary>
        /// ����
        ///</summary>
        [NotMapped]
        public long ProductStock { get; set; }
        /// <summary>
        /// ��ɫ
        /// </summary>
        [NotMapped]
        public string Color { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        [NotMapped]
        public string Size { get; set; }
        /// <summary>
        /// �汾
        /// </summary>
        [NotMapped]
        public string Version { get; set; }
        /// <summary>
        /// ��ʾͼƬ
        /// <para>��ɫ����</para>
        /// </summary>
        [NotMapped]
        public string ShowPic { get; set; }
    }
}
