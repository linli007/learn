using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using EDF;

namespace ElectroencephalographController
{
    /// <summary>
    /// 回放文件模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        /// <summary>
        /// 回放文件
        /// </summary>
        EDFFile playBack;

        /// <summary>
        /// 初始化
        /// </summary>
        public void PlayBackInit()
        {
            playBack = new EDFFile();
        }

        /// <summary>
        /// 将本地EDF文件提取至队列
        /// </summary>
        /// <param name="path"></param>
        public int PlayBackLoad(string path)
        {
            PlayBackInit();
            commonDataPool.queueOfEDFdata.Clear();
            playBack.readFile(path);
            commonDataPool.queueOfEDFdata.Enqueue(playBack);
            return commonDataPool.queueOfEDFdata.Count;
        }

        /// <summary>
        /// 将回放队列保存至本地文件
        /// </summary>
        public void PlayBackSave()
        {
            string folder;
            if (commonDataPool.replayDir == null || commonDataPool.replayDir == "")
                folder = AppDomain.CurrentDomain.BaseDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
            else
                folder = commonDataPool.replayDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                folder = folder + "\\" + DateTime.Now.ToString("HHmmss");
            }
            else
            {
                folder = folder + "\\" + DateTime.Now.ToString("HHmmss");
            }
            playBack.saveFile(folder);
            
            commonDataPool.queueOfPlayBack.Clear();
        }
    }
}
