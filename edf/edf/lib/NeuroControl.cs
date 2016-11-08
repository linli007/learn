using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using EDF;

namespace ElectroencephalographController
{
    /// <summary>
    /// Controller类  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        /// <summary>
        /// 功能状态
        /// </summary>
        private enum FunctionState { Nil,Configuration, DataCollectConfig, DataCollect, ListView, Analyse, Statistics, Report};
        
        /// <summary>
        /// 软件运行状态
        /// </summary>
        public enum TestState { Nil, DT, PlayBack };


        #region 变量
        /// <summary>
        /// Form列表
        /// </summary>
        private List<string> myFormList;

        /// <summary>
        /// 当前Form
        /// </summary>
        private string myCurrentForm;

        /// <summary>
        /// 当前功能状态
        /// </summary>
        private FunctionState myCurrentState;

        /// <summary>
        /// 当前运行状态
        /// </summary>
        private TestState myTestState = TestState.Nil;

        /// <summary>
        /// 播放标志
        /// </summary>
        private bool flagPlay;

        /// <summary>
        /// 主Form
        /// </summary>
        public FormMain myMainForm;

        /// <summary>
        /// 线程
        /// </summary>
        private Thread myPlayThread;

        /// <summary>
        /// 两条回放数据间的时间间隔
        /// </summary>
        private int[] sleepTime = new int[] { 500, 250, 100, 50, 25, 1 };

        /// <summary>
        /// 回放数据总量
        /// </summary>
        private int allPlayBackNum;

        /// <summary>
        /// 当前回放数据量
        /// </summary>
        private int currentPlayBackNum;

        /// <summary>
        /// 表示软件启动状态，包括待机/模块测试等，暂未用到
        /// </summary>
        private string softStatus;
        #endregion

        #region 访问器
        public int AllPlayBackNum
        {
            get { return allPlayBackNum; }
            set { allPlayBackNum = value; }
        }

        public TestState MyTestState
        {
            get { return myTestState; }
            set
            {
                myTestState = value;
                InitAllButtonsState(myTestState);
            }
        }

        public string SoftStatus
        {
            get { return softStatus; }
            set { softStatus = value; }
        }
        #endregion 

        /// <summary>
        /// 设置按钮状态
        /// </summary>
        /// <param name="myTestState"></param>
        private void InitAllButtonsState(TestState myTestState)
        {
            myDataCollectForm.InitDataCollectFormButtons(myTestState);
        }
          
        public NeuroControl(FormMain mainForm)
        {
            //实例化主窗口
            myMainForm = mainForm;
            //置状态
            myCurrentState = FunctionState.Nil;
            //初始化
            flagPlay = false;
            ModuleInitial();            
        }

        /// <summary>
        /// 初始化各子模块
        /// </summary>
        private void ModuleInitial()
        {
            myFormList = new List<string>();
            
            //公共数据
            CommonDataInit();
            //commonDataPool.beginDTDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //串口控制模块初始化
            ModuleConfigInit();

            //StatisticsInit();//统计           
            //AnalyseInit();//分析 
          
            //配置模块初始化
            FormConfigInit();

            //脑电波形显示窗口初始化
            FormDataCollectInit();

            //脑电数据列表窗口初始化
            FormListViewInit();

            //ListViewOfMsgInit();//信令列表            
            //MeasurementReportInit();//测量报告
            //LoadMap();//地图            
            //ReportFormInit();//报表   
    
            myFormList.Add("Configure");
            myFormList.Add("ListView");
            myFormList.Add("DataCollect");

            //PlayBackInit();//回放
            //DataBaseInit();//数据库

            //if (commonDataPool.DBConnected)
                //myConfigForm.SetBaseStationList();
        }

        /// <summary>
        /// 设置功能状态
        /// </summary>
        /// <param name="functionName"></param>
        public void SetFunctionState(string functionName)
        {
            switch (functionName)
            {
                case "系统配置":
                    myCurrentState = FunctionState.Configuration;
                    break;
                case "脑电数据采集":
                    myCurrentState = FunctionState.DataCollect;
                    //myMainForm.SetTopLabelText("地图");
                    break;
                case "脑电数据列表":
                    myCurrentState = FunctionState.ListView;
                    break;
                default:
                    myCurrentState = FunctionState.Nil;
                    break;
            }
            //myMainForm.ChangeButtonBackColor(functionName);
        }

        /// <summary>
        /// 显示特定的窗口
        /// </summary>
        public void ShowAppointedForm()
        {
            switch (myCurrentState)
            {
                case FunctionState.Configuration:                    
                    HideAllForm();
                    if (myConfigForm != null)
                    {                       
                        ShowConfigForm();
                    }
                    myCurrentForm = "Configuration";
                    break;
                case FunctionState.DataCollect:
                    HideAllForm();
                    if (myDataCollectForm != null)
                    {                     
                        ShowDataCollectForm();
                    }
                    myCurrentForm = "DataCollect";
                    break;
                case FunctionState.ListView:
                    HideAllForm();
                    if (myListViewForm != null)
                    {                       
                        ShowListViewForm();
                    }
                    myCurrentForm = "ListView";
                    break;
                case FunctionState.Analyse:
                    HideAllForm();
                    break;                              
                case FunctionState.Nil:
                    HideAllForm();
                    if (myDataCollectForm != null)
                    {                       
                        ShowDataCollectForm();
                    }
                    myCurrentForm = "DataCollect";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 隐藏所有窗口
        /// </summary>
        private void HideAllForm()
        {
            myConfigForm.Hide();
        }

        /// <summary>
        /// 获取回放路径
        /// </summary>
        /// <returns></returns>
        public string GetReplayFileDir()
        {
            return myConfigForm.GetReplayFileName();
        }

        /// <summary>
        /// 开始前工作
        /// </summary>
        public void InitBeforeStart()
        {            
            commonDataPool.queueOfEDFdata.Clear();
            commonDataPool.queueOfPlayBack.Clear();
            commonDataPool.queueOfNeurodata.Clear();
            commonDataPool.recStarDataCollectingAck = false;
            commonDataPool.startDataCollectingFlag = false;
            commonDataPool.isSquareWaveDemarcated = false;
            allPlayBackNum = 0;
            currentPlayBackNum = 0;
        }

        /// <summary>
        /// 结束后工作
        /// </summary>
        public void ReleaseAfterStop()
        {
            if (commonDataPool.queueOfPlayBack.Count != 0)
            {
                //if (myTestState == TestState.DT)
                    //PlayBackSave();
            }
            
            commonDataPool.queueOfEDFdata.Clear();
            commonDataPool.queueOfPlayBack.Clear();
            commonDataPool.queueOfNeurodata.Clear();
            commonDataPool.recStarDataCollectingAck = false;
            commonDataPool.startDataCollectingFlag = false;
            allPlayBackNum = 0;
            currentPlayBackNum = 0;
        }

        /// <summary>
        /// 回放文件
        /// </summary>
        /// 
        public void neuroStartReplay() 
        {
            flagPlay = true;
            while (flagPlay)
            {
                if (commonDataPool.queueOfEDFdata.Count <= 0)
                {
                    System.Threading.Thread.Sleep(1000);
                    break; ;
                }
                //从队列中取出EDF文件进行波形显示               
                EDFFile myEDF = GetEDFFromCommonDataQueue();
                DealWithEEGData(myEDF);
            }
            NeuroStop();
            ReleaseAfterStop();
            MyTestState = NeuroControl.TestState.PlayBack;
        }

        /// <summary>
        /// 
        /// </summary>
        public void neuroStart()
        {
            try
            {
                flagPlay = true;
                myPlayThread = new Thread(new ThreadStart(HappyRun));
                myPlayThread.IsBackground = true;
                myPlayThread.Start();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("开始运行异常！");
                LogHelper.WriteLog(typeof(NeuroControl), "开始运行异常:" + ex.Message);
            }
        }


        public void ZJUStop()
        {
            flagPlay = false;
        }

        /// <summary>
        /// 线程开启
        /// </summary>
        private void HappyRun()
        {
            while (flagPlay)
            {
                if (commonDataPool.queueOfEDFdata.Count <= 0)
                {
                    System.Threading.Thread.Sleep(1000);
                    break; ;
                }
                //从队列中取出EDF文件进行波形显示               
                EDFFile myEDF = GetEDFFromCommonDataQueue();
                DealWithEEGData(myEDF);
            }
        }

        public void NeuroStop()
        {
            flagPlay = false;
        }

        #region 开启、关闭定时器
        /// <summary>
        /// 开启定时器
        /// </summary>
        /// <param name="interval"></param>
        public void startEEGTimer(int interval)
        {
            myDataCollectForm.startEEGTimer(interval);
        }

        /// <summary>
        /// 关闭定时器
        /// </summary>
        public void stopTimer()
        {
            myDataRecWarningForm.Hide();
            myDataCollectForm.stopEEGTimer();
            timerOfSerialDataDealing.Stop();
            MyTestState = NeuroControl.TestState.Nil;
            timerOfEDFSaving.Stop();
            timerOfEDFSaving.Enabled = false;

            EDFFileSaving(null, null);//关闭定时器前保存一次文件
        }

        /// <summary>
        /// 开启读取串口缓存数据的定时器
        /// </summary>
        public void startSerialPortDataDealingTimer()
        {
            timerOfSerialDataDealing.Start();
        }

        /// <summary>
        /// 开启保存文件的定时器
        /// </summary>
        public void startEDFFileSavingTimer()
        {
            if (timerOfEDFSaving.Enabled == false) 
            {
                timerOfEDFSaving.Enabled = true;
                timerOfEDFSaving.Start();
            }
                
        }
        #endregion
    }
}
