using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FightGroupsInfo
    {
        /// <summary>
        /// ��������
        /// </summary>
        [NotMapped]
        public string ShopName { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [NotMapped]
        public string ShopLogo { get; set; }
        /// <summary>
        /// �����ۼ�
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// ��Ʒ����
        ///</summary>
        [NotMapped]
        public string ProductName { get; set; }
        /// <summary>
        /// ��ƷͼƬĿ¼
        /// </summary>
        [NotMapped]
        public string ProductImgPath { get; set; }
        /// <summary>
        /// ��ƷĬ��ͼƬ
        [NotMapped]
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// �ų��û���
        /// </summary>
        [NotMapped]
        public string HeadUserName { get; set; }
        /// <summary>
        /// �ų�ͷ��
        /// </summary>
        [NotMapped]
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// �ų�ͷ����ʾ
        /// <para>Ĭ��ͷ��ֵ����</para>
        /// </summary>
        [NotMapped]
        public string ShowHeadUserIcon
        {
            get
            {
                string defualticon = "";
                string result = HeadUserIcon;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = defualticon;
                }
                return result;
            }
        }
        /// <summary>
        /// ����״̬ ������  �ɹ�   ʧ��
        ///</summary>
        [NotMapped]
        public FightGroupBuildStatus BuildStatus { get
            {
                return (FightGroupBuildStatus)this.GroupStatus;
            }
        }
        /// <summary>
        /// ƴ�Ŷ�����
        /// </summary>
        public List<FightGroupOrderInfo> GroupOrders { get; set; }
        /// <summary>
        /// ����ʱ�ޣ��룩
        /// </summary>
        public int? Seconds { get; set; }
        
    }
}
