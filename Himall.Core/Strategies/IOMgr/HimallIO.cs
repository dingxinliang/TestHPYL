using System;
using System.Collections.Generic;
using System.IO;
namespace Himall.Core
{
    public static class HimallIO
    {
        private static IHimallIO _himallIO;
        static HimallIO()
        {
            _himallIO = null;
            Load();
        }
        private static void Load()
        {
            try
            {
                //container = builder.Build();
                _himallIO = ObjectContainer.Current.Resolve<IHimallIO>();
            }
            catch (Exception ex)
            {
                throw new CacheRegisterException("ע�Ỻ������쳣", ex);
            }
            //_himallIO = StrategyMgr.LoadStrategy<IHimallIO>();
        }
        public static byte[] DownloadTemplateFile(string fileName)
        {
            if ((_himallIO.GetType().FullName == "Himall.Strategy.OSS") && _himallIO.ExistFile(fileName))
            {
                return _himallIO.GetFileContent(fileName);
            }
            return null;
        }

        public static bool CopyFolder(string fromDirName, string toDirName, bool includeFile)
        {
            return _himallIO.CopyFolder(fromDirName, toDirName, includeFile);
        }
        public static IHimallIO GetHimallIO()
        {
            return _himallIO;
        }
        /// <summary>
        /// ��ȡ�ļ��ľ���·��
        /// </summary>
        /// <param name="fileName">�ļ�����</param>
        /// <returns></returns>
        public static string GetFilePath(string fileName)
        {
            return _himallIO.GetFilePath(fileName);
        }

        public static bool IsNeedRefreshFile(string fileName, out MetaInfo metaInfo)
        {
            metaInfo = null;
            if ((_himallIO.GetType().FullName == "Himall.Strategy.OSS") && _himallIO.ExistFile(fileName))
            {
                metaInfo = _himallIO.GetFileMetaInfo(fileName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ��ȡͼƬ��·��
        /// </summary>
        /// <param name="imageName">ͼƬ����</param>
        /// <param name="styleName">��ʽ����</param>
        /// <returns></returns>
        public static string GetImagePath(string imageName, string styleName = null)
        {
            return _himallIO.GetImagePath(imageName, styleName);
        }
        /// <summary>
        /// ��ȡ�ļ�����
        /// </summary>
        /// <param name="fileName">�ļ���</param>
        /// <returns></returns>
        public static byte[] GetFileContent(string fileName)
        {
            return _himallIO.GetFileContent(fileName);
        }
        /// <summary>
        /// ������ͨ�ļ�
        /// </summary>
        /// <param name="fileName">�ļ���</param>
        /// <param name="stream">�ļ���</param>
        /// <param name="fileCreateType"></param>
        public static void CreateFile(string fileName, Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            _himallIO.CreateFile(fileName, stream, fileCreateType);
        }
        /// <summary>
        /// ������ͨ�ļ�
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content">�ļ�����</param>
        /// <param name="fileCreateType"></param>
        public static void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            _himallIO.CreateFile(fileName, content, fileCreateType);
        }

        /// <summary>
        /// ����һ��Ŀ¼
        /// </summary>
        /// <param name="dirName"></param>
        public static void CreateDir(string dirName)
        {
            _himallIO.CreateDir(dirName);
        }
        /// <summary>
        /// �Ƿ���ڸ��ļ�
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ExistFile(string fileName)
        {
            if (fileName.Equals(""))
                return false;
            else
                return _himallIO.ExistFile(fileName);
        }

        /// <summary>
        /// �Ƿ���ڸ�Ŀ¼
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static bool ExistDir(string dirName)
        {
            return _himallIO.ExistDir(dirName);
        }
        /// <summary>
        /// ɾ��Ŀ¼
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="recursive">Ҫ�Ƴ� ·���е�Ŀ¼����Ŀ¼���ļ�����Ϊ true������Ϊ false</param>
        public static void DeleteDir(string dirName, bool recursive = false)
        {
            _himallIO.DeleteDir(dirName, recursive);
        }

        /// <summary>
        /// ɾ���ļ�
        /// </summary>
        /// <param name="fileName"></param>
        public static void DeleteFile(string fileName)
        {
            _himallIO.DeleteFile(fileName);
        }
        /// <summary>
        /// ����ɾ���ļ�
        /// </summary>
        /// <param name="fileNames"></param>
        public static void DeleteFiles(List<string> fileNames)
        {
            _himallIO.DeleteFiles(fileNames);
        }
        /// <summary>
        /// �����ļ�����Ŀ¼
        /// </summary>
        /// <param name="sourceFileName">ԭ·��</param>
        /// <param name="destFileName">Ŀ��·��</param>
        /// <param name="overwrite">�Ƿ񸲸�</param>
        public static void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            _himallIO.CopyFile(sourceFileName, destFileName, overwrite);
        }
        /// <summary>
        /// �ƶ��ļ�����Ŀ¼
        /// </summary>
        /// <param name="sourceFileName">ԭ·��</param>
        /// <param name="destFileName">Ŀ��·��</param>
        /// <param name="overwrite">�Ƿ񸲸�</param>
        public static void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            _himallIO.MoveFile(sourceFileName, destFileName, overwrite);
        }
        /// <summary>
        /// �г�Ŀ¼�µ��ļ�����Ŀ¼
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="self">�Ƿ�������� Ĭ��Ϊfalse</param>
        /// <returns></returns>
        public static List<string> GetDirAndFiles(string dirName, bool self = false)
        {
            return _himallIO.GetDirAndFiles(dirName, self);
        }

        /// <summary>
        /// �г�Ŀ¼�������ļ�
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="self">�Ƿ��������</param>
        /// <returns></returns>
        public static List<string> GetFiles(string dirName, bool self = false)
        {
            return _himallIO.GetFiles(dirName, self);
        }
        /// <summary>
        /// ָ�����ļ���׷�����ݣ�����ļ������ڣ��򴴽���׷���ļ���
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>

        public static void AppendFile(string fileName, Stream stream)
        {
            _himallIO.AppendFile(fileName, stream);
        }
        /// <summary>
        /// ָ�����ļ���׷�����ݣ�����ļ������ڣ��򴴽���׷���ļ���
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void AppendFile(string fileName, string content)
        {
            _himallIO.AppendFile(fileName, content);
        }
        /// <summary>
        ///  ��ȡĿ¼������Ϣ
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static MetaInfo GetDirMetaInfo(string dirName)
        {
            return _himallIO.GetDirMetaInfo(dirName);
        }
        /// <summary>
        /// ��ȡ�ļ�������Ϣ
        /// </summary>
        /// <param name="fileName">�ļ�����</param>
        /// <returns></returns>
        public static MetaInfo GetFileMetaInfo(string fileName)
        {
            return _himallIO.GetFileMetaInfo(fileName);
        }

        /// <summary>
        /// ��������ͼ
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
        {
            _himallIO.CreateThumbnail(sourceFilename, destFilename, width, height);
        }

        /// <summary>
        /// ��ȡ��ͬ�����������ĿͼƬ
        /// </summary>
        /// <param name="productPath"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string GetProductSizeImage(string productPath, int index, int width = 0)
        {
            return _himallIO.GetProductSizeImage(productPath, index, width);
        }

        /// <summary>
        /// ��ȡ��(http)��ȫ·��ͼƬ��APP���߽ӿڵ���
        /// </summary>
        /// <returns></returns>
        public static string GetRomoteImagePath(string imageName, string styleName=null)
        {
            if(string.IsNullOrWhiteSpace(imageName))
            {
                return "";
            }
            var path = _himallIO.GetImagePath(imageName, styleName);
            if (!path.StartsWith("http"))
            {
                return GetHttpUrl() + path;
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// ��ȡ��(http)��ȫ·�����ֳߴ��ͼƬ��APP���߽ӿڵ���
        /// </summary>
        /// <param name="productPath"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string GetRomoteProductSizeImage(string productPath, int index, int width = 0)
        {
            if (string.IsNullOrWhiteSpace(productPath))
            {
                return "";
            }
            var path = _himallIO.GetProductSizeImage(productPath, index, width);
            if (!path.StartsWith("http"))
            {
                return GetHttpUrl() + path;
            }
            else
            {
                return path;
            }
        }
        private static string GetHttpUrl()
        {
            string host = Core.Helper.WebHelper.GetHost();
            var port = Core.Helper.WebHelper.GetPort();
            var portPre = port == "80" ? "" : ":" + port;
            return "http://" + host + portPre + "/";
        }
    }
}
