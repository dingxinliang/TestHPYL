using Himall.CommonModel;
using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace Himall.Service
{
    public class TopicService : ServiceBase, ITopicService
    {

        public ObsoletePageModel<TopicInfo> GetTopics(int pageNo, int pageSize, PlatformType platformType = PlatformType.PC)
        {
            var topic = new ObsoletePageModel<TopicInfo>();
            int total;
            topic.Models = Context.TopicInfo.FindBy(item => item.PlatForm == platformType, pageNo, pageSize, out total, item => item.Id, false);
            topic.Total = total;
            return topic;
        }

        public QueryPageModel<TopicInfo> GetTopics(IServices.QueryModel.TopicQuery topicQuery)
        {
            int num;
            QueryPageModel<TopicInfo> model = new QueryPageModel<TopicInfo>();
            IQueryable<TopicInfo> source = from item in base.Context.TopicInfo
                                           where ((int)item.PlatForm) == ((int)topicQuery.PlatformType)
                                           select item;
            if (topicQuery.ShopId > 0L)
            {
                source = from item in source
                         where item.ShopId == topicQuery.ShopId
                         select item;
            }
            else
            {
                source = from item in source
                         where item.ShopId == 0L
                         select item;
            }
            if (topicQuery.IsRecommend.HasValue)
            {
                source = from item in source
                         where item.IsRecommend == topicQuery.IsRecommend.Value
                         select item;
            }
            if (!string.IsNullOrWhiteSpace(topicQuery.Name))
            {
                topicQuery.Name = topicQuery.Name.Trim();
                source = from item in source
                         where item.Name.Contains(topicQuery.Name)
                         select item;
            }
            if (!string.IsNullOrWhiteSpace(topicQuery.Tags))
            {
                topicQuery.Tags = topicQuery.Tags.Trim();
                source = from item in source
                         where item.Tags.Contains(topicQuery.Tags)
                         select item;
            }
            Func<IQueryable<TopicInfo>, IOrderedQueryable<TopicInfo>> orderBy = source.GetOrderBy<TopicInfo>(d => from o in d
                                                                                                                  orderby o.Id descending
                                                                                                                  select o);
            if (topicQuery.IsAsc)
            {
                orderBy = source.GetOrderBy<TopicInfo>(d => from o in d
                                                            orderby o.Id
                                                            select o);
            }
            model.Models = source.GetPage<TopicInfo>(out num, topicQuery.PageNo, topicQuery.PageSize, orderBy).ToList<TopicInfo>();
            model.Total = num;
            return model;

        }


        public void DeleteTopic(long id)
        {
            //删除数据
            Context.TopicInfo.Remove(id);
            if (Context.MobileHomeTopicsInfo.Where(item => item.TopicId == id).Count() > 0)
                throw new HimallException("你的微信首页推荐专题选择了该专题，请先解除选定再删除！");
            //删除对应图片目录。。。。。。。。
            string topicDir = "/Storage/Plat/Topic" + "/" + id;
            if (Core.HimallIO.ExistDir(topicDir))
                Core.HimallIO.DeleteDir(topicDir, true);
            Context.SaveChanges();
        }


        public void AddTopic(TopicInfo topicInfo)
        {
            if (string.IsNullOrWhiteSpace(topicInfo.Name))
                throw new ArgumentNullException("专题名称不能为空");

            bool moduleName = topicInfo.TopicModuleInfo.Any(a => a.Name.Length > 25);

            if (moduleName)
            {
                throw new HimallException("模块名称不能超过25个字符");
            }

            //string topicImageDir = string.Empty;
            TransactionScope scope = new TransactionScope();
            try
            {
              
                //保存数据
                Context.TopicInfo.Add(topicInfo);
                Context.SaveChanges();


                ////转移图片
                //string topImage = topicInfo.TopImage, backgroundImage = topicInfo.BackgroundImage, frontImage = topicInfo.FrontCoverImage;
                //topicImageDir = MoveImages(topicInfo.Id, ref backgroundImage, ref topImage, ref frontImage);
                //topicInfo.TopImage = topImage;
                //topicInfo.BackgroundImage = backgroundImage;
                //topicInfo.FrontCoverImage = frontImage;

                //Context.SaveChanges();

                scope.Complete();//提交更新

            }
            catch (Exception ex)
            {
                //if (!string.IsNullOrWhiteSpace(topicImageDir))
                //{
                //    try
                //    {
                //       Core.HimallIO.DeleteDir(topicImageDir,true);
                //    }
                //    catch { }
                //}
                throw new Himall.Core.HimallException("创建专题失败", ex);
            }
            finally
            {
                scope.Dispose();
            }

        }


        public TopicInfo GetTopicInfo(long id)
        {
            var topic = Context.TopicInfo.FindById(id);
            if (topic != null)
            {
                topic.TopicModuleInfo = Context.TopicModuleInfo.Where(item => item.TopicId == topic.Id).ToList();
            }
            return topic;
        }


        public void UpdateTopic(TopicInfo topicInfo)
        {
            bool moduleName = topicInfo.TopicModuleInfo.Any(a => a.Name.Length > 25);

            if (moduleName)
            {
                throw new HimallException("模块名称不能超过25个字符");
            }
            //string topicImageDir = string.Empty;
            try
            {
                //string topImage = topicInfo.TopImage, backgroundImage = topicInfo.BackgroundImage, frontImage = topicInfo.FrontCoverImage;
                //topicImageDir = MoveImages(topicInfo.Id, ref backgroundImage, ref topImage, ref frontImage);

                var topicModuleList = new List<TopicModuleInfo>();
                var topicModules = topicInfo.TopicModuleInfo.ToArray();
                foreach (var moduleItem in topicModules)
                    topicModuleList.Add(new TopicModuleInfo() { Name = moduleItem.Name, TitleAlign=moduleItem.TitleAlign ,TopicId = moduleItem.TopicId, ModuleProductInfo = moduleItem.ModuleProductInfo.ToList() });
                Context.TopicModuleInfo.Remove(item => item.TopicId == topicInfo.Id);//先删除
                Context.SaveChanges();

                //保存数据
                TopicInfo newTopicInfo = Context.TopicInfo.FindById(topicInfo.Id);
                newTopicInfo.Name = topicInfo.Name;
                //newTopicInfo.BackgroundImage = backgroundImage;
                //newTopicInfo.TopImage = topImage;
                //newTopicInfo.FrontCoverImage = frontImage;
                newTopicInfo.Tags = topicInfo.Tags;
                newTopicInfo.TopicModuleInfo = topicModuleList;
                newTopicInfo.IsRecommend = topicInfo.IsRecommend;
                newTopicInfo.SelfDefineText = topicInfo.SelfDefineText;
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                //if (!string.IsNullOrWhiteSpace(topicImageDir))
                //{
                //    try
                //    {
                //       Core.HimallIO.DeleteDir(topicImageDir);
                //    }
                //    catch { }
                //}
                throw new Himall.Core.HimallException("修改专题失败", ex);
            }

        }

        /// <summary>
        /// 图片转移
        /// </summary>
        /// <param name="topicId">专题编号</param>
        /// <param name="backGroundImage">临时背景图地址，返回正式地址</param>
        /// <param name="topImage">临时top图地址，返回正式地址</param>
        /// <returns>专题图片目录</returns>
        string MoveImages(long topicId, ref string backGroundImage, ref string topImage, ref string frontImage)
        {
            string topicImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Topic/" + topicId + "/";

            if (backGroundImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string backgroundOriUrl = backGroundImage.Substring(backGroundImage.LastIndexOf("/temp"));
                topicImageDir = relativeDir;
                Core.HimallIO.CopyFile(backgroundOriUrl,topicImageDir+ Path.GetFileName(backGroundImage),true);
                //目标地址
                backGroundImage = topicImageDir+ Path.GetFileName(backGroundImage);;
            }

            if (topImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string topImageOriUrl = topImage.Substring(topImage.LastIndexOf("/temp"));

                Core.HimallIO.CopyFile(topImageOriUrl, relativeDir + Path.GetFileName(topImage),true);
                topImage = relativeDir + Path.GetFileName(topImage);
            }

            if (!string.IsNullOrWhiteSpace(frontImage) && frontImage.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string frontImageOriUrl = frontImage.Substring(frontImage.LastIndexOf("/temp"));

                Core.HimallIO.CopyFile(frontImageOriUrl, relativeDir + Path.GetFileName(frontImage),true);

                frontImage = relativeDir + Path.GetFileName(frontImage);
            }

            return topicImageDir;
        }



    }
}
