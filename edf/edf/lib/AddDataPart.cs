using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDF;
using System.IO;
using System.Windows.Forms;
using System.Collections;

namespace ElectroencephalographController
{   
    /// <summary>
    /// 添加自定义格式的EDFx文件模块  --by zt
    /// </summary>
    public partial class NeuroControl
    {
        #region 变量
        /// <summary>
        /// EDFx文件
        /// </summary>
        private EDFFile newEDFFile;

        /// <summary>
        /// 导联源
        /// </summary>
        private Hashtable leadSource;

        /// <summary>
        /// 存储文件次数标记，从1开始  --by wdp
        /// </summary>
        private int saveFileNum;
        #endregion

        /// <summary>
        /// 初始化edfx文件 --by zt
        /// </summary>
        public void NewEDFFileInit()
        {
            newEDFFile = new EDFFile();
            leadSource = GetLeadSource();
            saveFileNum = 1;//修改  --by zt
            if (leadSource.Count == 0) 
            {
                leadSource = GetDefaultLeadSource();
            }

            #region 初始化文件头信息
            //版本号
            newEDFFile.Header.Version = "0";

            //DataRecord的数目，开始时赋值-1，最后保存文件时再赋值。
            newEDFFile.Header.NumberOfDataRecords = -1;

            //保留字节，"EDF+C"代表连续，"EDF+D"代表不连续
            newEDFFile.Header.Reserved = "EDF+C";

            //通道数
            newEDFFile.Header.NumberOfSignalsInDataRecord = 25;

            //头文件字节数
            newEDFFile.Header.NumberOfBytes = 256 + newEDFFile.Header.NumberOfSignalsInDataRecord * 256;

            //硬件信息，暂无，先用"xxx"代替
            newEDFFile.Header.RecordingIdentification.RecordingCode = "xxx";
            newEDFFile.Header.RecordingIdentification.RecordingTechnician = "xxx";
            newEDFFile.Header.RecordingIdentification.RecordingEquipment = "xxx";

            //定义物理最大值、最小值及数字最大值、最小值
            float physicalMinimum = (float)-4.5;
            float physicalMaximum = (float)4.5;
            float digitalMinimum = -(float)Convert.ToDouble(Convert.ToInt32("800000", 16));
            float digitalMaximum = (float)Convert.ToDouble(Convert.ToInt32("7FFFFF", 16));
            
            #region 为每个通道赋值
            newEDFFile.Header.Signals = new List<EDFSignal>();

            //第一通道信号是心电信号
            EDFSignal firstEdf_signal = new EDFSignal();
            firstEdf_signal.Label = "EKG";
            firstEdf_signal.PhysicalDimension = "V";
            firstEdf_signal.PhysicalMinimum = physicalMinimum;
            firstEdf_signal.PhysicalMaximum = physicalMaximum;
            firstEdf_signal.DigitalMinimum = digitalMinimum;
            firstEdf_signal.DigitalMaximum = digitalMaximum;

            //每个DataRecord里每路数据里包含的点数
            firstEdf_signal.NumberOfSamplesPerDataRecord = 1;
            newEDFFile.Header.Signals.Add(firstEdf_signal);

            //导联源的几路信号
            for (int i = 0; i < leadSource.Count; i++) 
            {
                EDFSignal edf_signal = new EDFSignal();
                edf_signal.Label = (string)leadSource[i + 1]+"-GND";
                edf_signal.PhysicalDimension = "V";
                edf_signal.PhysicalMinimum = physicalMinimum;
                edf_signal.PhysicalMaximum = physicalMaximum;
                edf_signal.DigitalMinimum = digitalMinimum;
                edf_signal.DigitalMaximum = digitalMaximum;
                edf_signal.NumberOfSamplesPerDataRecord = 1;
                newEDFFile.Header.Signals.Add(edf_signal);
            }
            //剩下的通道
            for (int i = leadSource.Count; i < newEDFFile.Header.NumberOfSignalsInDataRecord-1; i++)
            {
                EDFSignal edf_signal = new EDFSignal();
                edf_signal.Label = "Fp" + (i+2)+"-GND";
                edf_signal.PhysicalDimension = "V";
                edf_signal.PhysicalMinimum = physicalMinimum;
                edf_signal.PhysicalMaximum = physicalMaximum;
                edf_signal.DigitalMinimum = digitalMinimum;
                edf_signal.DigitalMaximum = digitalMaximum;
                edf_signal.NumberOfSamplesPerDataRecord = 1;
                newEDFFile.Header.Signals.Add(edf_signal);
            }
            #endregion

            #endregion

        }

        /// <summary>
        /// 添加病人信息 
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="sex">性别</param>
        /// <param name="birthday">生日</param>
        /// <param name="name">姓名</param>
        /// <param name="startDate">开始采集时间</param>
        public void AddNewEDFFile(string code, string sex, DateTime birthday, string name, DateTime startDate)
        {
            NewEDFFileInit();
            newEDFFile.Header.PatientIdentification.PatientCode = code;
            newEDFFile.Header.PatientIdentification.PatientSex = sex;
            newEDFFile.Header.PatientIdentification.PatientBirthDate = birthday;
            newEDFFile.Header.PatientIdentification.PatientName = name;
            newEDFFile.Header.RecordingIdentification.RecordingStartDate = startDate;
            newEDFFile.Header.StartDateEDF = startDate.ToString("dd.MM.yy");

            LogHelper.WriteLog(typeof(NeuroControl), "添加一个新的病人文件");
        }

        /// <summary>
        /// 获取newEDFFile
        /// </summary>
        /// <returns>newEDFFile</returns>
        public EDFFile GetNewEDFFile()
        {
            return newEDFFile;
        }

        /// <summary>
        /// 删除病例
        /// </summary>
        public void DeleteEDFFile() 
        {
            this.newEDFFile = null;
        }

        /// <summary>
        /// 将回放队列保存至本地文件 
        /// </summary>
        /// 未验证代码
        public void DataCollectSave()
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

        /// <summary>
        /// 保存数据 
        /// </summary>
        /// <param name="filePath">文件路径名</param>
        public void DataCollectSaveNewEDFFile(string filePath)
        {
            
            #region 弹框保存
            //string filePath;
            //SaveFileDialog sfd = new SaveFileDialog();
            ////OpenFileDialog ofd = new OpenFileDialog();
            ////打开时指定默认路径
            //sfd.InitialDirectory = @"C:\";
            //sfd.AddExtension = true;
            //sfd.DefaultExt = "edfx";
            ////如果用户点击确定
            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    filePath = sfd.FileName;
            //    newEDFFile.saveNewFile(filePath);
            //    //.saveNewFile(filePath);
            //}
            #endregion

            #region 直接保存到D:\edfx目录下

            //保存路径名称
            string sPath = @"D:\edfx";
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            filePath = filePath + "_" + saveFileNum.ToString();

            LogHelper.WriteLog(typeof(NeuroControl), "添加文件时序号" + saveFileNum);
            string edfFilePath = sPath+"\\"+ filePath +@".edfx";
            if (newEDFFile.saveNewFile(edfFilePath)) 
            {
                saveFileNum++;
            }
           
            #endregion
        }


    }
}
