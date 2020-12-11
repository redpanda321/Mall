using System;
using System.Collections.Generic;
using System.Text;
using Mall.Core;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace Mall.Strategy.IO
{
    public class AspNetIO : IMallIO
    {
        public void AppendFile(string fileName, string content)
        {
            byte[] fileContent = System.Text.Encoding.UTF8.GetBytes(content);
            var path = GetPhysicalPath(fileName);
            //在路径fullPath下关联一个FileStream对象
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            //将内容写入文件流
            fs.Write(fileContent, 0, fileContent.Length);
            //必须关闭文件流，否则得到的文本什么内容都没有
            fs.Close();//必须关闭
        }
        private byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        public void AppendFile(string fileName, System.IO.Stream stream)
        {
            var path = GetPhysicalPath(fileName);
            var fileContent = StreamToBytes(stream);
            FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            //将内容写入文件流
            fs.Write(fileContent, 0, fileContent.Length);
            //必须关闭文件流，否则得到的文本什么内容都没有
            fs.Close();//必须关闭
        }

        public void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new MallIOException(IOErrorMsg.FileNotExist.ToDescription());
            }
            var s = GetPhysicalPath(sourceFileName);
            var d = GetPhysicalPath(destFileName);
            var dir = d.Remove(d.LastIndexOf("\\"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!overwrite && ExistFile(destFileName)) //如果不可覆盖文件且目标文件已存在,传相对路径
            {
                throw new MallIOException(IOErrorMsg.FileExist.ToDescription());
            }
            else
            {
                File.Copy(s, d, overwrite);
            }
        }

        public void CreateDir(string dirName)
        {
            var path = GetPhysicalPath(dirName);

            Directory.CreateDirectory(path);
        }

        public void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            byte[] fileContent = System.Text.Encoding.UTF8.GetBytes(content);
            var path = GetPhysicalPath(fileName);
            var dir = path.Remove(path.LastIndexOf("\\"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (fileCreateType == FileCreateType.CreateNew)
            {
                if (!File.Exists(path))
                {
                    using (StreamWriter swhtml = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        foreach (var s in content)
                        {
                            swhtml.Write(s);
                        }
                        swhtml.Flush();
                    }
                    //FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);

                    //将内容写入文件流
                    //fs.Write(fileContent, 0, fileContent.Length);
                    //必须关闭文件流，否则得到的文本什么内容都没有
                    //fs.Close();//必须关闭
                }
                else
                {
                    throw new MallIOException(IOErrorMsg.FileExist.ToDescription());
                }
            }
            else
            {
                using (StreamWriter swhtml = new StreamWriter(path, false, Encoding.UTF8))
                {
                    foreach (var s in content)
                    {
                        swhtml.Write(s);
                    }
                    swhtml.Flush();
                }
                //FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                //将内容写入文件流
                //fs.Write(fileContent, 0, fileContent.Length);
                //必须关闭文件流，否则得到的文本什么内容都没有
                //fs.Close();//必须关闭
            }
        }

        public void CreateFile(string fileName, System.IO.Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
        {
            var path = GetPhysicalPath(fileName);
            var dir = path.Remove(path.LastIndexOf("\\"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (fileCreateType == FileCreateType.CreateNew)
            {
                if (!File.Exists(path))
                {
                    FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                    //将内容写入文件流
                    var fileContent = StreamToBytes(stream);
                    fs.Write(fileContent, 0, fileContent.Length);
                    //必须关闭文件流，否则得到的文本什么内容都没有
                    fs.Close();//必须关闭
                }
                else
                {
                    throw new MallIOException(IOErrorMsg.FileExist.ToDescription());
                }
            }
            else
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                //将内容写入文件流
                var fileContent = StreamToBytes(stream);
                fs.Write(fileContent, 0, fileContent.Length);
                //必须关闭文件流，否则得到的文本什么内容都没有
                fs.Close();//必须关闭
            }
        }

        public void DeleteDir(string dirName, bool recursive = false)
        {
            var path = GetPhysicalPath(dirName);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
            else
            {
                throw new MallIOException(IOErrorMsg.DirNotExist.ToDescription());
            }
        }

        public void DeleteFile(string fileName)
        {
            var file = GetPhysicalPath(fileName);
            if (ExistFile(fileName))
            {
                File.Delete(file);
            }
        }

        public void DeleteFiles(List<string> fileNames)
        {

            foreach (var file in fileNames)
            {
                var pfile = GetPhysicalPath(file);
                if (ExistFile(file))
                {
                    File.Delete(pfile);
                }
            }
        }

        public bool ExistDir(string dirName)
        {
            var file = GetPhysicalPath(dirName);
            var result = Directory.Exists(file);
            return result;
        }

        public bool ExistFile(string fileName)
        {
            var file = GetPhysicalPath(fileName);
            var result = File.Exists(file);
            return result;
        }


        public List<string> GetDirAndFiles(string dirName, bool self = false)
        {
            List<string> files = new List<string>();
            var path = GetPhysicalPath(dirName);
            if (self)
            {
                files.Add(path);
            }
            files.AddRange(GetDirAndFiles(path));
            return files;
        }

        public MetaInfo GetDirMetaInfo(string dirName)
        {
            var path = GetPhysicalPath(dirName);
            MetaInfo info = new MetaInfo();
            info.LastModifiedTime = Directory.GetLastWriteTime(path);
            info.ContentLength = Core.Helper.IOHelper.GetDirectoryLength(path);
            return info;
        }

        public byte[] GetFileContent(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new MallIOException(IOErrorMsg.FileNotExist.ToDescription());
            }
            var f = GetPhysicalPath(fileName);
            FileStream fs = new FileStream(f, FileMode.Open);
            byte[] byteData = new byte[fs.Length];
            fs.Read(byteData, 0, byteData.Length);
            fs.Close();
            return byteData;
        }

        public MetaInfo GetFileMetaInfo(string fileName)
        {
            MetaInfo minfo = new MetaInfo();
            var file = GetPhysicalPath(fileName);
            FileInfo finfo = new FileInfo(file);
            minfo.ContentLength = finfo.Length;


            string contentType = "";
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            minfo.ContentType = contentType;
            minfo.LastModifiedTime = finfo.LastWriteTime;
            // minfo.ObjectType
            return minfo;
        }

        public string GetFilePath(string fileName)
        {
            return string.Format("{0}/{1}", GetHttpUrl(), fileName);
        }

        public List<string> GetFiles(string dirName, bool self = false)
        {
            List<string> files = new List<string>();
            var path = GetPhysicalPath(dirName);
            if (self)
            {
                files.Add(path);
            }
            files.AddRange(GetAllFiles(path));
            return files;
        }

        private List<string> GetAllFiles(string path)
        {
            List<string> f = new List<string>();
            var files = Directory.GetFiles(path);
            f.AddRange(files);
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                f.AddRange(GetAllFiles(dir));
            }
            return f;
        }

        private List<string> GetDirAndFiles(string path)
        {
            List<string> f = new List<string>();
            var files = Directory.GetFiles(path);
            f.AddRange(files);
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                f.Add(dir);
                f.AddRange(GetAllFiles(dir));
            }
            return f;
        }

        public string GetImagePath(string imageName, string styleName = null)
        {
            //  imageName = GetFileName(imageName);
            string url = string.Empty;
            if (!string.IsNullOrWhiteSpace(styleName))
            {
                return imageName;
            }
            else
            {
                return imageName;
            }
        }

        public void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new MallIOException(IOErrorMsg.FileNotExist.ToDescription());
            }
            var s = GetPhysicalPath(sourceFileName);
            var d = GetPhysicalPath(destFileName);
            var dir = d.Remove(d.LastIndexOf("\\"));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!overwrite && ExistFile(d)) //如果不可覆盖文件且目标文件已存在
            {
                throw new MallIOException(IOErrorMsg.FileExist.ToDescription());
            }
            else
            {
                File.Move(s, d);
            }

        }

        private string GetHttpUrl()
        {
            string host = Core.Helper.WebHelper.GetHost();
            var port = Core.Helper.WebHelper.GetPort();
            var Scheme = Core.Helper.WebHelper.GetScheme();
            var portPre = port == "80" ? "" : ":" + port;
            return Scheme + "://" + host + portPre + "/";
        }

        private string GetFileName(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("/"))
            {
                fileName = fileName.Substring(1);
            }
            return fileName;
        }

        private string GetPhysicalPath(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
                    fileName = fileName.Substring(fileName.IndexOf("/", fileName.IndexOf("//") + 2) + 1);

                var index = fileName.LastIndexOf("@");
                if (index > 0)
                    fileName = fileName.Substring(0, index);
            }

            return Core.Helper.IOHelper.GetMapPath(fileName);
        }

        private string GetDirName(string dirName)
        {
            if (dirName.StartsWith("/"))
            {
                dirName = dirName.Substring(1);
            }
            if (!dirName.EndsWith("/"))
            {
                dirName = dirName + "/";
            }
            return dirName;
        }

        /// <summary>
        /// 生成图片缩略图
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
        {
            var s = GetPhysicalPath(sourceFilename);
            var d = GetPhysicalPath(destFilename);
            Core.Helper.ImageHelper.CreateThumbnail(s, d, width, height);
        }

        /// <summary>
        /// 获取不同尺寸的产品图片
        /// </summary>
        /// <param name="productPath"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public string GetProductSizeImage(string productPath, int index, int width = 0)
        {
            // string image = GetFileName(productPath);

            if (!string.IsNullOrEmpty(productPath))
            {
                if (string.IsNullOrEmpty(Path.GetExtension(productPath)) && !productPath.EndsWith("/"))
                {
                    productPath = productPath + "/";
                }
                productPath = Path.GetDirectoryName(productPath).Replace("\\", "/");

                string url = string.Empty;
                if (width == 0)
                {
                    url = string.Format("{0}/{1}.png", productPath, index);
                }

                if (width != 0)
                {

                    url = string.Format(productPath + "/{0}_{1}.png", index, width);
                }
                return url;
            }
            else
            {
                return productPath;
            }
        }

        /// <summary>
        /// 复制目录
        /// </summary>
        /// <param name="fromDirName">源文件夹</param>
        /// <param name="toDirName">目标文件夹</param>
        /// <param name="includeFile">是否包含文件</param>
        /// <returns></returns>
        public bool CopyFolder(string fromDirName, string toDirName, bool includeFile)
        {
            try
            {
                if (!Directory.Exists(toDirName))
                    Directory.CreateDirectory(toDirName);

                // 子文件夹
                foreach (string subName in Directory.GetDirectories(fromDirName))
                    CopyFolder(subName + "\\", toDirName + Path.GetFileName(subName) + "\\", false);

                if (includeFile)
                {
                    // 文件
                    foreach (string fileName in Directory.GetFiles(fromDirName))
                        File.Copy(fileName, toDirName + Path.GetFileName(fileName), true);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new MallIOException("复制目录失败", ex);
            }
        }
    }
}
