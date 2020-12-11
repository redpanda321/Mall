using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
namespace Mall.Core
{
    public static class MallIO
    {
        private static IMallIO IO;
        static MallIO()
        {
            IO = null;
            Load();
        }
        private static void Load()
        {
            try
            {
                //container = builder.Build();
             //   IO =  EngineContext.Current.Resolve<IMallIO>();

                IO = EngineContext.Current.Resolve<IMallIO>();
            }
            catch (Exception ex)
            {
                throw new CacheRegisterException("注册缓存服务异常", ex);
            }
            //IO = StrategyMgr.LoadStrategy<IMallIO>();
        }

        public static IMallIO GetMallIO()
        {
            return IO;
        }
        /// <summary>
        /// 获取文件的绝对路径
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string GetFilePath(string fileName)
        {
            return IO.GetFilePath(fileName);
        }
        /// <summary>
        /// 获取图片的路径
        /// </summary>
        /// <param name="imageName">图片名称</param>
        /// <param name="styleName">样式名称</param>
        /// <returns></returns>
        public static string GetImagePath(string imageName, string styleName = null)
        {
            return IO.GetImagePath(imageName, styleName);
        }
        /// <summary>
        /// 获取文件内容
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static byte[] GetFileContent(string fileName)
        {
            return IO.GetFileContent(fileName);
        }
        /// <summary>
        /// 创建普通文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        /// <param name="fileCreateType"></param>
        public static void CreateFile(string fileName, Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            IO.CreateFile(fileName, stream, fileCreateType);
        }
        /// <summary>
        /// 创建普通文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content">文件内容</param>
        /// <param name="fileCreateType"></param>
        public static void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            IO.CreateFile(fileName, content, fileCreateType);
        }

        /// <summary>
        /// 创建一个目录
        /// </summary>
        /// <param name="dirName"></param>
        public static void CreateDir(string dirName)
        {
            IO.CreateDir(dirName);
        }
        /// <summary>
        /// 是否存在该文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool ExistFile(string fileName)
        {
            if (fileName.Equals(""))
                return false;
            else
                return IO.ExistFile(fileName);
        }

        /// <summary>
        /// 是否存在该目录
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static bool ExistDir(string dirName)
        {
            return IO.ExistDir(dirName);
        }
        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="recursive">要移除 路径中的目录、子目录和文件，则为 true；否则为 false</param>
        public static void DeleteDir(string dirName, bool recursive = false)
        {
            IO.DeleteDir(dirName, recursive);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public static void DeleteFile(string fileName)
        {
            IO.DeleteFile(fileName);
        }
        /// <summary>
        /// 批量删除文件
        /// </summary>
        /// <param name="fileNames"></param>
        public static void DeleteFiles(List<string> fileNames)
        {
            IO.DeleteFiles(fileNames);
        }
        /// <summary>
        /// 复制文件到新目录
        /// </summary>
        /// <param name="sourceFileName">原路径</param>
        /// <param name="destFileName">目标路径</param>
        /// <param name="overwrite">是否覆盖</param>
        public static void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            IO.CopyFile(sourceFileName, destFileName, overwrite);
        }
        /// <summary>
        /// 移动文件到新目录
        /// </summary>
        /// <param name="sourceFileName">原路径</param>
        /// <param name="destFileName">目标路径</param>
        /// <param name="overwrite">是否覆盖</param>
        public static void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            IO.MoveFile(sourceFileName, destFileName, overwrite);
        }
        /// <summary>
        /// 列出目录下的文件和子目录
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="self">是否包含本身 默认为false</param>
        /// <returns></returns>
        public static List<string> GetDirAndFiles(string dirName, bool self = false)
        {
            return IO.GetDirAndFiles(dirName, self);
        }

        /// <summary>
        /// 列出目录下所有文件
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="self">是否包含自身</param>
        /// <returns></returns>
        public static List<string> GetFiles(string dirName, bool self = false)
        {
            return IO.GetFiles(dirName, self);
        }
        /// <summary>
        /// 指定的文件下追加内容（如果文件不存在，则创建可追加文件）
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>

        public static void AppendFile(string fileName, Stream stream)
        {
            IO.AppendFile(fileName, stream);
        }
        /// <summary>
        /// 指定的文件下追加内容（如果文件不存在，则创建可追加文件）
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void AppendFile(string fileName, string content)
        {
            IO.AppendFile(fileName, content);
        }
        /// <summary>
        ///  获取目录基本信息
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static MetaInfo GetDirMetaInfo(string dirName)
        {
            return IO.GetDirMetaInfo(dirName);
        }
        /// <summary>
        /// 获取文件基本信息
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static MetaInfo GetFileMetaInfo(string fileName)
        {
            return IO.GetFileMetaInfo(fileName);
        }

        /// <summary>
        /// 创建缩略图
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
        {
            IO.CreateThumbnail(sourceFilename, destFilename, width, height);
        }

        /// <summary>
        /// 获取不同尺码的商品图片
        /// </summary>
        /// <param name="productPath"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string GetProductSizeImage(string productPath, int index, int width = 0)
        {
            return IO.GetProductSizeImage(productPath, index, width);
        }

        /// <summary>
        /// 获取带(http)的全路径图片给APP或者接口调用
        /// </summary>
        /// <returns></returns>
        public static string GetRomoteImagePath(string imageName, string styleName = null)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return "";
            }
            var path = IO.GetImagePath(imageName, styleName);
            if (!path.StartsWith("http"))
            {
                return GetHttpUrl() + path.Trim('/');
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// 获取带(http)的全路径各种尺寸的图片给APP或者接口调用
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
            var path = IO.GetProductSizeImage(productPath, index, width);
            if (!path.StartsWith("http"))
            {

                return GetHttpUrl() + path;
            }
            else
            {
                return path;
            }
        }

        public static bool CopyFolder(string fromDirName, string toDirName, bool includeFile)
        {
            return IO.CopyFolder(fromDirName, toDirName, includeFile);
        }

        /// <summary>
        /// 获取模版文件
        /// </summary>
        /// <param name="fileName">模版文件路径</param>
        /// <returns>OSS则返回文件,其它则返回null</returns>
        public static byte[] DownloadTemplateFile(string fileName)
        {
            if (IO.GetType().FullName == "Mall.Strategy.OSS")
            {
                if (IO.ExistFile(fileName))
                {
                    return IO.GetFileContent(fileName);
                }
            }
            return null;
        }

        /// <summary>
        /// 是否需要更新本地文件
        /// </summary>
        /// <param name="fileName">模版文件路径</param>
        /// <returns>OSS规则则返回true</returns>
        public static bool IsNeedRefreshFile(string fileName, out MetaInfo metaInfo)
        {
            metaInfo = null;
            if (IO.GetType().FullName == "Mall.Strategy.OSS")
            {
                if (IO.ExistFile(fileName))
                {
                    metaInfo = IO.GetFileMetaInfo(fileName);
                    return true;
                }
            }
            return false;
        }

        private static string GetHttpUrl()
        {
            string host = Core.Helper.WebHelper.GetHost();
            var port = Core.Helper.WebHelper.GetPort();
            var Scheme = Core.Helper.WebHelper.GetScheme();
            var portPre = port == "80" ? "" : ":" + port;
            return Scheme + "://" + host + portPre + "/";
        }
    }
}
