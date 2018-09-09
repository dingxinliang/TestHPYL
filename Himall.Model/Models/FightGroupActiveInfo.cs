using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;
using Himall.Model.Models;

namespace Himall.Model
{
    public partial class FightGroupActiveInfo
    {
        /// <summary>
        /// ������
        /// <para>�ֶ�����</para>
        /// </summary>
        [NotMapped]
        public string ShopName { get; set; }
        /// <summary>
        /// ������ĿͼƬ��ַ
        /// </summary>
        [NotMapped]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// ƴ�Ż״̬
        /// </summary>
        [NotMapped]
        public FightGroupActiveStatus ActiveStatus {
            get
            {
                FightGroupActiveStatus result = FightGroupActiveStatus.Ending;
                if(EndTime<DateTime.Now)
                {
                    result = FightGroupActiveStatus.Ending;
                }
                else
                {
                    if(StartTime>DateTime.Now)
                    {
                        result = FightGroupActiveStatus.WillStart;
                    }
                    else
                    {
                        result = FightGroupActiveStatus.Ongoing;
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// ���
        /// <para>�ֶ�ά��</para>
        /// </summary>
        [NotMapped]
        public List<FightGroupActiveItemInfo> ActiveItems { get; set; }
        /// <summary>
        /// ��ƴ��
        /// </summary>
        [NotMapped]
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// ����ۼ�
        /// </summary>
        [NotMapped]
        public decimal MiniSalePrice { get; set; }
        /// <summary>
        /// �˷�ģ��
        /// </summary>
        [NotMapped]
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// ������Ŀ�����
        /// </summary>
        [NotMapped]
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// ������Ŀ������
        /// </summary>
        [NotMapped]
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// ������Ŀ����
        /// </summary>
        [NotMapped]
        public string ProductCode { get; set; }
        /// <summary>
        /// ������Ŀ��λ
        /// </summary>
        [NotMapped]
        public string MeasureUnit { get; set; }
        /// <summary>
        /// ������Ŀ�Ƿ�ɹ���
        /// </summary>
        [NotMapped]
        public bool CanBuy { get; set; }
        /// <summary>
        /// ������Ŀ�Ƿ��п��
        /// </summary>
        [NotMapped]
        public bool HasStock { get; set; }

        public long SaleCounts { get; set; }
        /// <summary>
        /// �������״̬
        /// </summary>
        [NotMapped]
        public FightGroupManageAuditStatus FightGroupManageAuditStatus { get
            {
                FightGroupManageAuditStatus result = FightGroupManageAuditStatus.Normal;
                if(ManageAuditStatus==-1)
                {
                    result = FightGroupManageAuditStatus.SoldOut;
                }
                return result;
            }
        }
        [NotMapped]
        public List<ComboDetail> ComboList { get; set; }

        /// <summary>
        /// ������ĿĬ��ͼƬ
        /// </summary>
        [NotMapped]
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// ������Ŀ����ͼƬ
        /// </summary>
        [NotMapped]
        public List<string> ProductImages { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [NotMapped]
        public string ShowMobileDescription { get; set; }
    }
}
