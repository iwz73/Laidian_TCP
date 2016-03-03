using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qiniu.IO.Resumable;
using Qiniu.RS;
using Qiniu.IO;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;

namespace ClS
{
    /// <summary>
    /// 获取
    /// </summary>
    public class GetLog
    { 
        public string[]logNames;
        public string msg;
        public int result;
    }
    /// <summary>
    /// 根据指令自动获取日志
    /// 2015-9-14
    /// </summary>
    public class AutoGetLog
    {
        public static int compressionLevel = 9;
        public static byte[] buffer = new byte[9048000];
        public AutoGetLog()
        {
            //
            Qiniu.Conf.Config.ACCESS_KEY = "A8SIAnj_MoLaBFRnPlmdCi78eLSUdY57VbMgFJZy";
            Qiniu.Conf.Config.SECRET_KEY = "dMS_sXCxuoRzvQU6eRvk1zETOWH21IQo2p1RRIsS";
        
        }
        /// <summary>  
        /// 断点续传  
        /// </summary>  
        /// <param name="bucket"></param>  
        /// <param name="key"></param>  
        /// <param name="fname"></param>  
        public  void ResumablePutFile(string bucket, string key, string fname)
        {
            Console.WriteLine("\n===> ResumablePutFile {0}:{1} fname:{2}", bucket, key, fname);
            PutPolicy policy = new PutPolicy(bucket);
            string upToken = policy.Token();
            Qiniu.IO.Resumable.Settings setting = new Qiniu.IO.Resumable.Settings();
            ResumablePutExtra extra = new ResumablePutExtra();
            extra.Notify += new EventHandler<PutNotifyEvent>(extra_Notify); //+= PutNotifyEvent(int blkIdx, int blkSize, BlkputRet ret);//上传进度通知事件  
            ResumablePut client = new ResumablePut(setting, extra);
            client.PutFile(upToken, fname, key);
        }

        public bool GZipFile(string sourcefilename, string zipfilename)
        {
            bool blResult;//表示压缩是否成功的返回结果
            //为源文件创建读取文件的流实例
            FileStream srcFile = File.OpenRead(sourcefilename);
            //为压缩文件创建写入文件的流实例，
            GZipOutputStream zipFile = new GZipOutputStream(File.Open(zipfilename, FileMode.Create));
            try
            {
                byte[] FileData = new byte[srcFile.Length];//创建缓冲数据
                srcFile.Read(FileData, 0, (int)srcFile.Length);//读取源文件
                zipFile.Write(FileData, 0, FileData.Length);//写入压缩文件
                blResult = true;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                blResult = false;
            }
            srcFile.Close();//关闭源文件
            zipFile.Close();//关闭压缩文件
            return blResult;
        }

        /// 压缩文件
        ///
        /// 要压缩的文件路径
        /// 压缩后的文件路径
        public void ZipFile(string fileToZip, string zipedFile)
        {
            if (!File.Exists(fileToZip))
            {
                throw new FileNotFoundException("The specified file " + fileToZip + " could not be found.");
            }

            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipedFile)))
            {
                string fileName = Path.GetFileName(fileToZip);
                ZipEntry zipEntry = new ZipEntry(fileName);
                zipStream.PutNextEntry(zipEntry);
                zipStream.SetLevel(compressionLevel);

                using (FileStream streamToZip = new FileStream(fileToZip, FileMode.Open, FileAccess.Read))
                {
                    int size = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, size);

                    while (size < streamToZip.Length)
                    {
                        int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                        zipStream.Write(buffer, 0, sizeRead);
                        size += sizeRead;
                    }
                }
            }
        }
        /// 解压缩文件
        ///
        /// 压缩文件路径
        /// 解压缩文件路径
        public void UnZipFile(string zipFilePath, string unZipFilePatah)
        {
            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry zipEntry = null;
                while ((zipEntry = zipStream.GetNextEntry()) != null)
                {
                    string fileName = Path.GetFileName(zipEntry.Name);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        if (zipEntry.CompressedSize == 0)
                            break;
                        using (FileStream stream = File.Create(unZipFilePatah + fileName))
                        {
                            while (true)
                            {
                                int size = zipStream.Read(buffer, 0, buffer.Length);
                                if (size > 0)
                                {
                                    stream.Write(buffer, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 上传文件测试
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="fname"></param>
        public  bool PutFile(string bucket, string key, string fname)
        {
            var policy = new PutPolicy(bucket);
            string upToken = policy.Token();
            PutExtra extra = new PutExtra();
            IOClient client = new IOClient();
            PutRet atr= client.PutFile(upToken, key, fname, extra);
            return atr.OK;
        }
        FileStream heBingFile;//新建的将要合并的文件流
        FileInfo f1;//用来保存选中要合并的文件对象

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="space">七牛的空间的名称</param>
        /// <param name="key">存储到七牛的文件名称</param>
        /// <param name="filepath">要上传的文件名称</param>
        public bool uploadfile(string space, string key, string filepath)
        {


            //如果没有上传文件夹 则创建一个文件夹
            if (!Directory.Exists(System.Environment.CurrentDirectory + "\\upload"))
            {
                Directory.CreateDirectory(System.Environment.CurrentDirectory + "\\upload");
            }
            else
            {
                try
                {
                    DirectoryInfo folder = new DirectoryInfo(System.Environment.CurrentDirectory + @"\upload");

                    foreach (FileInfo file in folder.GetFiles("*.*"))
                    {
                        File.Delete(System.Environment.CurrentDirectory + @"\upload\" + file.Name);
                    }
                }
                catch
                { }
            }

            if (File.Exists(filepath))
            {
                try
                {
                    FileInfo f = new FileInfo(filepath);
                    FileStream srcFileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);//实例化并打开分割的文件流
                    StreamReader streamReader = new StreamReader(srcFileStream);

                    int fenGeKuaiShu;//分割块数
                    int fenGeKuaiDaXiao = 10 * 1024;//分割块大小10M
                    int wenBenChangDu = (int)f.Length / 1024;
                    //开始分割文件

                    if (wenBenChangDu % fenGeKuaiDaXiao > 0)//如果文件长度刚好能整除分割块大小则分割块数=文件长度/分割块大小，否则分割块数=文件长度/分割块大小+1；
                    {
                        fenGeKuaiShu = wenBenChangDu / fenGeKuaiDaXiao + 1;
                    }
                    else
                    {
                        fenGeKuaiShu = wenBenChangDu / fenGeKuaiDaXiao;
                    }
                    //int index = 0;
                    List<string> strlist = new List<string>();
                    for (int i = 0; i < fenGeKuaiShu; i++)//因为要求要用多线程来进行文件的分割、合并的，所以这里调用新的线程
                    {
                        string pf = System.Environment.CurrentDirectory + @"\upload\" + key + "-" + i.ToString() + ".txt";
                        using (FileStream fenGeFileStream = new FileStream(pf, FileMode.OpenOrCreate, FileAccess.Write))//新建文件流按照指定的块数分割文件，//这里默认的是保存在和源文件同一目录下，也可以自己另行保存
                        {
                            int data = 0;

                            byte[] buffer = new byte[fenGeKuaiDaXiao * 1024];
                            if ((data = srcFileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {

                                BinaryWriter bw = new BinaryWriter(fenGeFileStream, Encoding.Default);
                                bw.Write(buffer, 0, data);
                                strlist.Add(pf);
                            }
                            else
                            {

                            }
                        }
                    }
                    //using(FileStream fi=new FileStream(
                    for (int ls = 0; ls < strlist.Count; ls++)
                    {
                        ZipFile(strlist[ls], System.Environment.CurrentDirectory + "\\upload" + "\\" + key + "-" + ls.ToString() + ".rar");
                        Thread.Sleep(1000);
                        PutFile(space, key + "-" + ls.ToString(), System.Environment.CurrentDirectory + "\\upload" + "\\" + key + "-" + ls.ToString() + ".rar");
                    }
                }
                catch
                {
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public void extra_Notify(object sender, PutNotifyEvent e)
        {
            
            //删除压缩文件
            //通知下一步
           // MessageBox.Show("wancheng");
        }

        public void GetTaskFromSvr(string[]files)
        {
            foreach (string str in files)
            { 

            }
            
        }
    }
}
