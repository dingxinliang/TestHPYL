using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Himall.Core;
using Himall.CommonModel;

namespace Himall.Model
{
    public partial class FightGroupOrderInfo
    {
        /// <summary>
        /// ��ʵ����
        /// </summary>
        [NotMapped]
        public string RealName { get; set; }
        /// <summary>
        /// �û�ͷ��
        /// </summary>
        [NotMapped]
        public string Photo { get; set; }
        /// <summary>
        /// �����û���
        /// </summary>
        [NotMapped]
        public string UserName { get; set; }
        /// <summary>
        /// ����״̬
        /// </summary>
        [NotMapped]
        public FightGroupBuildStatus GroupStatus { get; set; }
        /// <summary>
        /// ������������
        /// </summary>
        public int LimitedNumber { get; set; }
        /// <summary>
        /// ʱ������
        /// </summary>
        public decimal LimitedHour { get; set; }
        /// <summary>
        /// �Ѳ�������
        /// </summary>
        public int JoinedNumber { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime AddGroupTime { get; set; }
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// ��ƷͼƬ
        /// </summary>
        public string IconUrl { get; set; }
        /// <summary>
        /// ��ƴ�۸�
        /// </summary>
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// ���ųɹ��û�
        /// </summary>
        public List<UserInfo> UserInfo { get; set; }
        /// <summary>
        /// �ų�����
        /// </summary>
        public string HeadUserName { get; set; }
        /// <summary>
        /// �ų�ͷ��
        /// </summary>
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// �ų�ͷ����ʾ��Ĭ��ͷ��ֵ���䣩
        /// </summary>
        public string ShowHeadUserIcon { get; set; }
        /// <summary>
        /// ����ʱ�ޣ��룩
        /// </summary>
        public int? Seconds { get; set; }
        /// <summary>
        /// ����״̬
        /// </summary>
        [NotMapped]
        public FightGroupOrderJoinStatus GetJoinStatus { get
            {
            
                return (FightGroupOrderJoinStatus)this.JoinStatus;
            }
        }
        /// <summary>
        /// �Ƿ�����˿�
        /// </summary>
        [NotMapped]
        public bool CanRefund
        {
            get
            {
                bool result = false;
                switch (GetJoinStatus)
                {
                    case FightGroupOrderJoinStatus.BuildSuccess:
                        result = true;
                        break;
                    case FightGroupOrderJoinStatus.BuildFailed:
                        result = true;
                        break;
                    case FightGroupOrderJoinStatus.JoinFailed:
                        result = true;
                        break;
                }
                return result;
            }
        }
        /// <summary>
        /// �Ƿ���Է���
        /// </summary>
        [NotMapped]
        public bool CanSendGood
        {
            get
            {
                bool result = false;
                switch (GetJoinStatus)
                {
                    case FightGroupOrderJoinStatus.BuildSuccess:
                        result = true;
                        break;
                }
                return result;
            }
        }
        public bool IsCurrentDay { get; set; }
              

    }
    public class UserInfo
    {
        /// <summary>
        /// �û�ͷ��
        /// </summary>
        [NotMapped]
        public string Photo { get; set; }
        /// <summary>
        /// �����û���
        /// </summary>
        [NotMapped]
        public string UserName { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        [NotMapped]
        public DateTime? JoinTime { get; set; }

    }
}
