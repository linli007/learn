using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDF;
using System.Collections;

namespace ElectroencephalographController
{
    /// <summary>
    /// 获得设置病例管理模块的显示病人信息面板的参数值--by xcg
    /// </summary>
    public class CaseDataLv
    {
        #region 类的私有变量
        /// <summary>
        /// 病人ID，具有唯一性
        /// </summary>
        private string patientCode;

        /// <summary>
        /// 病人的姓名
        /// </summary>
        private string patientName;

        /// <summary>
        /// 病人的年龄
        /// </summary>
        private string patientAge;

        /// <summary>
        /// 记录的开始时间
        /// </summary>
        private string startTime;

        /// <summary>
        /// 记录开始的日期
        /// </summary>
        private string startDate;

        /// <summary>
        /// 病人的性别
        /// </summary>
        private string patientGender;

        /// <summary>
        /// 记录的持续时长
        /// </summary>
        private string duration;

        /// <summary>
        /// 文件的存储路径，在回放或删除时会用到
        /// </summary>
        private string path;
        #endregion

        #region 获得类的私有变量的值
        public string PatientCode { get { return patientCode; } }

        public string PatientName { get { return patientName; } }

        public string PatientAge { get { return patientAge; } }

        public string StartTime { get { return startTime; } }

        public string StartDate { get { return startDate; } }

        public string PatientGender { get { return patientGender; } }

        public string Duration { get { return duration; } }

        public string Path { get { return path; } }

        #endregion

        #region 类的私有变量赋值
        /// <summary>
        /// 设置类的私有变量的值
        /// </summary>
        /// <param name="patientCode"></param>
        /// <param name="patientName"></param>
        /// <param name="patientAge"></param>
        /// <param name="startTime"></param>
        /// <param name="startDate"></param>
        /// <param name="patientGender"></param>
        /// <param name="duration"></param>
        /// <param name="path"></param>
        public void CaseDataSet(string patientCode, string patientName, string patientAge, string startTime, string startDate, string patientGender, string duration, string path)
        {
            //设置病人ID的值
            this.patientCode = patientCode;

            //设置病人的姓名
            this.patientName = patientName;

            //设置病人的年龄
            this.patientAge = patientAge;

            //设置记录开始的时间
            this.startTime = startTime;

            //设置记录开始的日期
            this.startDate = startDate;

            //设置的病人的性别
            this.patientGender = patientGender;

            //设置记录持续的时长
            this.duration = duration;

            //设置文件的存储路径
            this.path = path;
        }
        #endregion
    }


    /// <summary>
    /// 公共数据池模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        /// <summary>
        /// 公共数据池
        /// </summary>
        private CommonData commonDataPool;

        /// <summary>
        /// 初始化公共数据池
        /// </summary>
        private void CommonDataInit()
        {
            commonDataPool = new CommonData();
        }

        #region 获取公共数据池中的数据

        #region 公共数据池中edf及edfx数据
        /// <summary>
        /// 从公共数据池中获取EDF队列中的数据 
        /// </summary>
        /// <returns></returns>
        public EDFFile GetEDFFromCommonDataQueue()
        {
            return (EDFFile)(commonDataPool.queueOfEDFdata.Dequeue());
        }

        /// <summary>
        /// 从公共数据池中获取实时采集时解析后的数据 
        /// </summary>
        /// <returns></returns>
        public string GetNeuroDataFromCommonDataQueue()
        {
            if (commonDataPool.queueOfNeurodata.Count > 0)
                return (string)commonDataPool.queueOfNeurodata.Dequeue();
            else
                return "";
        }

        /// <summary>
        /// 从公共数据池中获取实时采集时解析后的数据数目 
        /// </summary>
        /// <returns></returns>
        public int GetNeuroDataFromCommonDataNumber()
        {
            return commonDataPool.queueOfNeurodata.Count;
        }

        /// <summary>
        /// 清空公共数据池中实时采集的数据  
        /// </summary>
        public void ClearNeuroDataFromCommonDataQueue() 
        {
            commonDataPool.queueOfNeurodata.Clear();
        }
        #endregion

        #region 公共数据池中导联信息
        /// <summary>
        /// 从公共数据池中获取导联参数  --by zt
        /// </summary>
        /// <returns></returns>
        public Hashtable GetLeadList() 
        {
            return commonDataPool.leadList;
        }
        /// <summary>
        /// 从公共数据池中获取导联编码
        /// </summary>
        /// <returns></returns>
        public Hashtable GetLeadCodeList()
        {
            return commonDataPool.leadCodeList;
        }
        /// <summary>
        /// 为导联赋值  --by zt
        /// </summary>
        /// <param name="list"></param>
        public void SetLeadList(Hashtable list)
        {
            commonDataPool.leadList = list;
        }

        /// <summary>
        /// 为导联编码赋值  --by wdp
        /// </summary>
        /// <param name="listCode"></param>
        public void SetLeadCodeList(Hashtable listCode)
        {
            commonDataPool.leadCodeList = listCode;
        }

        /// <summary>
        /// 从公共数据池中获取导联源  --by zt
        /// </summary>
        /// <returns></returns>
        public Hashtable GetLeadSource() 
        {
            return commonDataPool.leadSource;
        }

        /// <summary>
        /// 设置导联源  --by zt
        /// </summary>
        /// <param name="list"></param>
        public void SetLeadSource(Hashtable list)
        {
            commonDataPool.leadSource = list;
        }

        /// <summary>
        /// 从公共数据池中获取当前导联配置的名称 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentLeadConfig()
        {
            return commonDataPool.currentLeadConfig;
        }

        /// <summary>
        /// 设置当前导联配置的名称  
        /// </summary>
        /// <param name="leadConfig"></param>
        public void SetCurrentLeadConfig(string leadConfig)
        {
            commonDataPool.currentLeadConfig = leadConfig;
        }

        /// <summary>
        /// 从公共数据池中获取默认导联源  --by zt
        /// </summary>
        /// <returns></returns>
        public Hashtable GetDefaultLeadSource()
        {
            return commonDataPool.defaultLeadSource;
        }

        /// <summary>
        /// 设置默认导联源，暂未用到  --by zt
        /// </summary>
        /// <param name="list"></param>
        public void SetDefaultLeadSource(Hashtable list)
        {
            commonDataPool.defaultLeadSource = list;
        }
        #endregion

        #region 公共数据池中标定电压值
        /// <summary>
        /// 从公共数据池中获取标定电压的值  
        /// </summary>
        /// <returns></returns>
        public int GetDemarcateCV() 
        {
           return  commonDataPool.demarcateCV;
        }

        public void SetDemarcateCV(int value) 
        {
            commonDataPool.demarcateCV = value;
        }
        #endregion

        #region 公共数据池中ECG信号显示标志
        public bool GetECGShownState()
        {
            return commonDataPool.isECGShown;
        }
        #endregion

        #region 公共数据池中方波标定状态
        /// <summary>
        /// 获取方波标定状态，true为开始标定，false为停止标定  --by wdp
        /// </summary>
        /// <returns></returns>
        public bool GetDemarcateState()
        {
            return commonDataPool.isSquareWaveDemarcated;
        }

        public void SetDemarcateState(bool isSquareWaveDemarcated)
        {
            commonDataPool.isSquareWaveDemarcated = isSquareWaveDemarcated;
        }
        #endregion

        #region 获取画图相关参数  -- by lxl
        public double getspeed()
        {
            return commonDataPool.speed;
        }

        public void setspeed(double speed)
        {
            commonDataPool.speed = speed;
        }

        public double GetPixelPerCM()
        {
            return commonDataPool.pixelPerCM;
        }

        public void SetPixelPerCM(double value)
        {
            commonDataPool.pixelPerCM = value;
        }

        public int getsensivity()
        {
            return commonDataPool.sensitivity;
        }

        public void setsensitivity(int sens)
        {
            commonDataPool.sensitivity = sens;
        }

        public void setSecondsPerPage(double sec)
        {
            commonDataPool.secondsPerPage = sec;
        }

        public double getSecondsPerPage()
        {
            return commonDataPool.secondsPerPage;
        }

        public System.Drawing.Color getColor(int i)
        {
            return commonDataPool.chartColor[i];
        }

        public void setColor(int i,System.Drawing.Color col)
        {
            commonDataPool.chartColor[i]=col;
        }

        public Boolean getSignalDisplay(int i)
        {
            return commonDataPool.signalDisplay[i];
        }

        public void setSignalDisplay(int i, Boolean flag)
        {
            commonDataPool.signalDisplay[i] = flag;
        }
        #endregion
        #endregion
    }
}