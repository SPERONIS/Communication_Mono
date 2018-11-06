using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace BizUtils.Zip
{
    public static class Zipper
    {
        /// <summary>
        /// 将多个流进行zip压缩，返回压缩后的流
        /// </summary>
        /// <param name="streams">key：文件名；value：文件名对应的要压缩的流</param>
        /// <returns>压缩后的流</returns>
        public static Stream PackageManyZip(Dictionary<string, Stream> streams)
        {
            byte[] buffer = new byte[6500];
            MemoryStream returnStream = new MemoryStream();
            var zipMs = new MemoryStream();
            using (ZipOutputStream zipStream = new ZipOutputStream(zipMs))
            {
                zipStream.SetLevel(1);
                foreach (var kv in streams)
                {
                    string fileName = kv.Key;
                    using (var streamInput = kv.Value)
                    {
                        zipStream.PutNextEntry(new ZipEntry(fileName));
                        while (true)
                        {
                            var readCount = streamInput.Read(buffer, 0, buffer.Length);
                            if (readCount > 0)
                            {
                                zipStream.Write(buffer, 0, readCount);
                            }
                            else
                            {
                                break;
                            }
                        }
                        zipStream.Flush();
                    }
                }
                zipStream.Finish();
                zipMs.Position = 0;
                zipMs.CopyTo(returnStream, 5600);
            }
            returnStream.Position = 0;
            return returnStream;
        }

        /// <summary>
        /// 将多个流进行zip压缩，返回压缩后的流
        /// </summary>
        /// <param name="streams">key：文件名；value：文件名对应的要压缩的流</param>
        /// <returns>压缩后的流</returns>
        public static void PackageManyZip(Dictionary<string, Stream> streams, Stream returnStream)
        {
            byte[] buffer = new byte[6500];
            var zipMs = new MemoryStream();
            using (ZipOutputStream zipStream = new ZipOutputStream(zipMs))
            {
                zipStream.SetLevel(5);
                foreach (var kv in streams)
                {
                    string fileName = kv.Key;
                    using (var streamInput = kv.Value)
                    {
                        zipStream.PutNextEntry(new ZipEntry(fileName));
                        while (true)
                        {
                            var readCount = streamInput.Read(buffer, 0, buffer.Length);
                            if (readCount > 0)
                            {
                                zipStream.Write(buffer, 0, readCount);
                            }
                            else
                            {
                                break;
                            }
                        }
                        zipStream.Flush();
                    }
                }
                zipStream.Finish();
                zipMs.Position = 0;
                zipMs.CopyTo(returnStream, 5600);
            }
            returnStream.Position = 0;
        }
    }
}
