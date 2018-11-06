using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using Nini.Config;
using Newtonsoft.Json;
using System.IO;
using BizUtils.Rest;
using System.Collections.Specialized;

namespace BizFileStore
{
    internal class FileStoreSrv
    {
        private DCClient clt;
        private string rootDir = @"D:\filedata";

        internal FileStoreSrv()
        { }

        internal void Start()
        {
            IConfigSource cs =
                new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BizFileStore.ini"));
            IConfig c = cs.Configs["config"];
            rootDir = c.Get("RootDir");

            clt = DCClient.Instance("BizFileStore", "global");
            clt.Srv("/filestore/genfilename", genfilename);
            clt.Srv("/filestore/appendfile", appendfile);
            clt.Srv("/filestore/savefile", savefile);
            clt.Srv("/filestore/readfile", readfile);
        }

        internal void Stop()
        {
            clt.UnSrv("/filestore/genfilename", genfilename);
            clt.UnSrv("/filestore/appendfile", appendfile);
            clt.UnSrv("/filestore/savefile", savefile);
            clt.UnSrv("/filestore/readfile", readfile);
            DCClient.Dispose(clt);
        }

        private byte[] genfilename(string extInfo, byte[] req)
        {
            IDictionary<string, string> ext = DCUtil.ParaStrToDic(extInfo);
            if (!ext.ContainsKey("filecate"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo(),
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "请求参数filecate为空"},
                                    }
                            );
            }
            string filecate = ext["filecate"];

            string filename = genFileNameInternal(filecate);

            return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.Succeed },
                                new ListDictionary {
                                    {"respnum", 1},
                                    {"respmsg", "文件名生成成功"},
                                    {"filename", filename},
                                    }
            );
        }

        private List<string> latestFileNames = new List<string>();
        private string genFileNameInternal(string filecate)
        {
            string fileName = null;

            string fileNamePre = string.Concat(filecate, "_", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            lock (latestFileNames)
            {
                for (int seed = 0; seed < 1000000; seed++)
                {
                    fileName = string.Concat(fileNamePre, "_", seed.ToString("000000"));
                    if (!latestFileNames.Contains(fileName))
                        break;
                }

                latestFileNames.Add(fileName);
                if (latestFileNames.Count > 1000000)
                {
                    latestFileNames.RemoveRange(0, 1000000 / 100);
                }
            }
            return fileName;
        }

        private string genFilePathInternal(string filename)
        {
            int idx = filename.IndexOf('_');
            string cate = filename.Substring(0, idx);
            string other = filename.Substring(idx + 1);
            int idx2 = other.IndexOf('_');
            string date = other.Substring(0, idx2);
            string filepath = Path.Combine(rootDir, cate, date, filename);

            try
            {
                Directory.CreateDirectory(Path.Combine(rootDir, cate, date));
            }
            catch { }

            return filepath;
        }

        private byte[] appendfile(string extInfo, byte[] req)
        {
            IDictionary<string, string> ext = DCUtil.ParaStrToDic(extInfo);
            if (!ext.ContainsKey("filename"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo(),
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "请求参数filename为空"},
                                    }
                            );
            }
            string filename = ext["filename"];

            string filepath = this.genFilePathInternal(filename);

            try
            {
                FileStream fs = new FileStream(Path.Combine(rootDir, filepath), FileMode.OpenOrCreate, FileAccess.Write);
                if (ext.ContainsKey("offset"))
                {
                    long offset = Convert.ToInt64(ext["offset"]);
                    if (offset > fs.Length)
                    {
                        int gap = (int)(offset - fs.Length);
                        fs.Seek(0, SeekOrigin.End);
                        fs.Write(new byte[gap], 0, gap);
                    }
                    else if (offset < fs.Length)
                    {
                        fs.Seek(offset, SeekOrigin.Begin);
                    }
                }
                fs.Write(req, 0, req.Length);
                fs.Close();

                return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.Succeed },
                                new ListDictionary {
                                    {"respnum", req.Length},
                                    {"respmsg", "文件追加成功"},
                                    {"filename", filename},
                                    }
                );
            }
            catch (Exception ex)
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.ServerSideError },
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "文件追加异常 " + ex.Message},
                                    }
                            );
            }
        }

        private byte[] savefile(string extInfo, byte[] req)
        {
            IDictionary<string, string> ext = DCUtil.ParaStrToDic(extInfo);
            if (!ext.ContainsKey("filecate"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo(),
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "请求参数filecate为空"},
                                    }
                            );
            }
            string filecate = ext["filecate"];

            string filename = this.genFileNameInternal(filecate);
            string filepath = this.genFilePathInternal(filename);

            try
            {
                FileStream fs = new FileStream(Path.Combine(rootDir, filepath), FileMode.Create, FileAccess.Write);
                fs.Write(req, 0, req.Length);
                fs.Close();

                return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.Succeed },
                                new ListDictionary {
                                    {"respnum", req.Length},
                                    {"respmsg", "文件保存成功"},
                                    {"filename", filename},
                                    }
                            );
            }
            catch (Exception ex)
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.ServerSideError },
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "文件保存异常 " + ex.Message},
                                    }
                            );
            }
        }

        private byte[] readfile(string extInfo, byte[] req)
        {
            IDictionary<string, string> ext = DCUtil.ParaStrToDic(extInfo);
            if (!ext.ContainsKey("filename"))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo(),
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "请求参数filename为空"},
                                    }
                            );
            }
            string filename = ext["filename"];

            int offset = 0;
            if (ext.ContainsKey("offset"))
            {
                offset = int.Parse(ext["offset"]);
            }

            int size = 0;
            if (ext.ContainsKey("size"))
            {
                size = int.Parse(ext["size"]);
            }

            string filepath = this.genFilePathInternal(filename);

            if (!File.Exists(filepath))
            {
                return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.NotFound },
                                new ListDictionary {
                                    {"respnum", -1},
                                    {"respmsg", "文件不存在"},
                                    }
                            );
            }

            FileStream fs = new FileStream(Path.Combine(rootDir, filepath), FileMode.Open, FileAccess.Read);

            if (size == 0)
            {
                long realSize = fs.Length;
                if (realSize > 1024 * 1024 * 20)
                {
                    return PackOrb.PackRespose(
                                new HttpHeadInfo { StatusCode = HttpStatusCode.TooLarge },
                                    new ListDictionary {
                                    {"respnum", -2},
                                    {"respmsg", "文件超长"},
                                    }
                                );
                }

                size = (int)realSize;
            }

            byte[] result = new byte[size];
            int realLen = fs.Read(result, offset, size);
            if (realLen < size)
            {
                Array.Resize(ref result, realLen);
            }

            string contentType = this.judgeFileType(filename);

            return PackOrb.PackRespose(                                
                new HttpHeadInfo 
                { 
                    StatusCode = HttpStatusCode.Succeed,
                    ContentType = judgeFileType(filename),
                }, 
                result);
        }

        private string judgeFileType(string filename)
        {
            int idx = filename.LastIndexOf('.');
            string ext = string.Empty;
            if (idx > 0)
                ext = filename.Substring(idx + 1).ToLower();
            switch (ext)
            {
                case "":
                default:
                    return "application";
                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                    return "image";
                case "dbf":
                    return "application";
            }
        }
    }
}
