using Himall.IServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Himall.Web
{
    public class GalleryHelper
    {
        private string tempLatePath = "";
        private string templateCuName = "";
        private const string adminTemplateDic = "/Areas/Admin/Templates/vshop/";
        private const string sellerAdminTemplateDic = "/Areas/SellerAdmin/Templates/vshop/{0}/";

        /// <summary>
        /// 更新模版名称
        /// </summary>
        /// <param name="tName"></param>
        /// <param name="name"></param>
        /// <param name="shopId"></param>
        public void UpdateTemplateName(string tName, string name, long shopId = 0L)
        {
            string path1 = "/Areas/Admin/Templates/vshop/";
            if (shopId != 0L)
                path1 = string.Format("/Areas/SellerAdmin/Templates/vshop/{0}/", (object)shopId);
            foreach (string path2 in Directory.GetDirectories(HttpContext.Current.Server.MapPath(path1)))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path2);
                if (directoryInfo.Name.ToLower().Equals(tName.ToLower()))
                {
                    FileStream fileStream = Enumerable.FirstOrDefault<FileInfo>((IEnumerable<FileInfo>)directoryInfo.GetFiles("template.xml")).OpenWrite();
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load((Stream)fileStream);
                    xmlDocument.SelectSingleNode("root/Name").InnerText = name;
                    fileStream.Close();
                }
            }
        }
        /// <summary>
        /// 记在主题
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<ManageThemeInfo> LoadThemes(long shopId = 0L)
        {
            string path1 = "/Areas/Admin/Templates/vshop/";
            if (shopId != 0L)
                path1 = string.Format("/Areas/SellerAdmin/Templates/vshop/{0}/", (object)shopId);
            XmlDocument xmlDocument = new XmlDocument();
            List<ManageThemeInfo> list = new List<ManageThemeInfo>();
            foreach (string path2 in Directory.Exists(HttpContext.Current.Server.MapPath(path1)) ? Directory.GetDirectories(HttpContext.Current.Server.MapPath(path1)) : (string[])null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path2);
                string str = directoryInfo.Name.ToLower(CultureInfo.InvariantCulture);
                if (str.Length > 0 && !str.StartsWith("_"))
                {
                    foreach (FileInfo fileInfo in directoryInfo.GetFiles("template.xml"))
                    {
                        ManageThemeInfo manageThemeInfo = new ManageThemeInfo();
                        FileStream fileStream = fileInfo.OpenRead();
                        xmlDocument.Load((Stream)fileStream);
                        fileStream.Close();
                        manageThemeInfo.Name = xmlDocument.SelectSingleNode("root/Name").InnerText;
                        manageThemeInfo.ThemeName = str;
                        manageThemeInfo.ThemeImgUrl = string.Format("{0}{1}/{2}", (object)path1, (object)str, (object)directoryInfo.GetFiles("default.*")[0].Name);
                        if (str == this.tempLatePath)
                            this.templateCuName = xmlDocument.SelectSingleNode("root/Name").InnerText;
                        list.Add(manageThemeInfo);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 读取图片名称
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetImgName(string fileName)
        {
            return "/Templates/vshop/" + fileName + "/default.png";
        }

    }

}