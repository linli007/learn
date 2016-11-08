using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace ElectroencephalographController
{
    /// <summary>
    /// 通信模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        #region 变量
        /// <summary>
        /// 串口采集模块
        /// </summary>
        public NeuroPortManager moduleTest;

        /// <summary>
        /// 实时采集配置Form
        /// </summary>
        private FormConfigDataCollect myConfigDataCollectForm;

        /// <summary>
        /// 向下位机发送开始命令
        /// </summary>
        private string startDataCollectingCmdAck = "A5A597080000009FAAAA";

        /// <summary>
        /// 向下位机发送结束命令
        /// </summary>
        private string stopDataCollectingCmdAck = "A5A5970A0000009DAAAA";
		
		//wdp 0615
        private string startDataCollectingCmd = "A5A595080000009DAAAA";
		
        //wdp 0711
        private string heartbeatCmd = "A5A595090000009CAAAA";
        /// <summary>
        /// 电极脱落通道
        /// </summary>
        private List<short> fallOffChannelNum;

        /// <summary>
        /// 最近一次电极脱落时间
        /// </summary>
        private DateTime electrodeFallOffTime;

        /// <summary>
        /// 电极脱落信号超时，超出该时间未接收到电极脱落信号则视为电极未脱落
        /// </summary>
        private double electrodeFallOffTimeout = 10;

        /// <summary>
        /// 第一次采集数据标志  --by zt
        /// </summary>
        private bool firstDataFlag;

        /// <summary>
        /// 本次接收数据开始时间
        /// </summary>
        private DateTime timeOfStartCollecting;

        /// <summary>
        /// 本次接收数据结束时间
        /// </summary>
        private DateTime timeOfLatestDataBlock;

        /// <summary>
        /// 按采样计数器和初始时间插值产生的数据时间  --by zt
        /// </summary>
        private DateTime timeofDataArrival;
        
        /// <summary>
        /// 硬件版本号标志，0为错误，1为正确，-1为未检测
        /// </summary>
        private int hardware_correct = 1;

        /// <summary>
        /// 固件版本号标志，0为错误，1为正确，-1为未检测
        /// </summary>
        private int firmware_correct = 1;

        /// <summary>
        /// 软件版本号标志，0为错误，1为正确，-1为未检测
        /// </summary>
        private int software_correct = 1;

        /// <summary>
        /// 硬件版本号、固件版本号、软件版本号标志，0为错误，1为正确，-1为未检测
        /// </summary>
        private int hardware_firmware_software_correct = -1;

        /// <summary>
        /// 第一次采集数据的时间  --by zt
        /// </summary>
        private DateTime firstTimeOfStartCollecting;
        
        /// <summary>
        /// 数据采集递增计数器上次采集数据值
        /// </summary>
        private int data_Collect_Count_PreviousOne;

        /// <summary>
        /// 当前处理数据在本次处理数据中的位置索引
        /// </summary>
        private int data_Collect_Index;

        /// <summary>
        /// 数据采样时间间隔（ms），先初始化
        /// </summary>
        private double timeInterval = 3;

        /// <summary>
        /// 定义一个定时器，每隔一段时间读取串口缓存数据
        /// </summary>
        private System.Timers.Timer timerOfSerialDataDealing;

        /// <summary>
        /// 收到的一行消息
        /// </summary>
        private byte[] oneMsg;

        /// <summary>
        /// 每次从串口获取完整报告后剩余的数据，
        /// 如假设一次读取到的数据为A5 A5 11 01 AA AA A5 A5 22，则该变量中存放的是A5 A5 22
        /// </summary>
        private byte[] EEGLeft;

        /// <summary>
        /// 缺省数据个数  --by zt
        /// </summary>
        private int numberOfLockData = 0; 

        /// <summary>
        /// 每次清除数据的时间  --by zt
        /// </summary>
        private DateTime timeOfClear;
        
        /// <summary>
        /// 定义一个定时器，每隔一段时间保存一次内存数据到EDF文件  
        /// </summary>
        private System.Timers.Timer timerOfEDFSaving;
		
		private double timeOutRestart=0.5;//超时未接收到数据的时间（s），超过该时间值，尝试重开串口
		
		private int timeOutRestartCount = 0;//超时未接收数据次数，该参数在serialDataDealing事件中计数。
		
		private bool firstTimeOut = true;//第一次超时未接收到数据标志

        private bool isDataRecWarningShown = false;

        /// <summary>
        /// 数据接收异常提醒时间（s），初定为5s接收不到数据弹框显示异常
        /// </summary>
        private double timeOutPopup = 5;
        private int timeOutPopupCount = 0;//超时未接收数据次数，该参数在serialDataDealing事件中计数。
        private double timeOutHeartBeat = 0.5;//心跳包发送时间间隔
        private int timeOutHeartBeatCount = 0;
        private FormDataRecWarning myDataRecWarningForm;//数据接收异常弹框
        private int isComSendSucceed = 1;//指示串口是否打开的标志位,0为打开，1为关闭
        private delegate void dataRecWarningFormShow();
        private delegate void dataRecWarningFormHide();
        private event dataRecWarningFormShow dateRecWarningFormShowEvent;
        #endregion 
        
        #region 属性访问器  --by zt

        public DateTime TimeofDataArrival
        {
            get { return timeofDataArrival; }
            set { timeofDataArrival = value; }
        }

        public DateTime TimeOfStartCollecting
        {
            get
            {
                return timeOfStartCollecting;
            }
            set
            {
                timeOfStartCollecting = value;
            }
        }

        public DateTime TimeOfClear
        {
            get
            {
                return timeOfClear;
            }
            set
            {
                timeOfClear = value;
            }
        }

        public DateTime FirstTimeOfStartColleting
        {
            get
            {
                return firstTimeOfStartCollecting;
            }
            set
            {
                firstTimeOfStartCollecting = value;
            }
        }
        #endregion
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void ModuleConfigInit()
        {
            if (myDataRecWarningForm == null)
            {
                myDataRecWarningForm = new FormDataRecWarning();
                myDataRecWarningForm.ReigisterNeuroControl(this);
                //加载窗体不显示，wdp
                myDataRecWarningForm.ShowInTaskbar = false;
                double srcOpacity = myDataRecWarningForm.Opacity;
                myDataRecWarningForm.Opacity = 0;
                myDataRecWarningForm.Show();
                myDataRecWarningForm.Hide();
                myDataRecWarningForm.ShowInTaskbar = true;
                myDataRecWarningForm.Opacity = srcOpacity;
            }
            moduleTest = new NeuroPortManager();
            myConfigDataCollectForm = new FormConfigDataCollect(moduleTest);
            myConfigDataCollectForm.ReigisterNeuroControl(this);
            moduleTest.ReceivedAllMsgEvent -= new NeuroPortManager.ReceivedAllMsgEventHandler(serialDataEnqueue);
            moduleTest.ReceivedAllMsgEvent += new NeuroPortManager.ReceivedAllMsgEventHandler(serialDataEnqueue);
            //formConfigdataCollect.SerialPortOpenedEvent += new FormConfigDataCollect.SerialPortOpenedEventHandler(ModuleInit);
            fallOffChannelNum=new List<short>();
            electrodeFallOffTime = DateTime.Now.AddSeconds(-10);
            data_Collect_Count_PreviousOne = 0;

            //实例化Timer类，设置间隔时间为100ms
            timerOfSerialDataDealing = new System.Timers.Timer(100);

            //到达时间的时候执行事件
            timerOfSerialDataDealing.Elapsed += new System.Timers.ElapsedEventHandler(serialDataDealing);

            //设置是执行一次（false）还是一直执行(true)；
            timerOfSerialDataDealing.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed事件；疑问
            timerOfSerialDataDealing.Enabled = false; //true;
            firstDataFlag = true;
            timeOfStartCollecting = DateTime.Now;
            timeOfLatestDataBlock = DateTime.Now;
            data_Collect_Index = 0;
            //20分钟保存一次数据，单位是ms
            //timerOfEDFSaving = new System.Timers.Timer(120000); //2分钟
            timerOfEDFSaving = new System.Timers.Timer(1200000);
            //timerOfEDFSaving = new System.Timers.Timer(30000);
            timerOfEDFSaving.Elapsed += new System.Timers.ElapsedEventHandler(EDFFileSaving);
            timerOfEDFSaving.AutoReset = true;
            //timerOfEDFSaving.Enabled = true;  // false;
            timerOfEDFSaving.Enabled = false;
            
         
        }

        /// <summary>
        /// 把接收到的串口数据存入公共数据池  --by wdp
        /// </summary>
        /// <param name="AllMsg"></param>
        public void serialDataEnqueue(byte[] AllMsg)
        {
            for (int i = 0; i < AllMsg.Length;i++ )
                commonDataPool.queueOfSerialPortData.Enqueue(AllMsg[i]);
        }

        /// <summary>
        /// 定时对串口缓冲存入公共数据池的数据进行处理  --by wdp
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void serialDataDealing(object source, System.Timers.ElapsedEventArgs e)
        {
            int bytesLeft = 0; int i, oneMsgLength;
            ArrayList IndexOfA5A5 = new ArrayList();

            lock (commonDataPool.queueOfSerialPortData) 
            {
                int serailPortDataCount = commonDataPool.queueOfSerialPortData.Count;
                timeOfLatestDataBlock = DateTime.Now;//记录本次数据到达时间
                timeOfStartCollecting = timeOfLatestDataBlock.AddMilliseconds(-timerOfSerialDataDealing.Interval);//上次数据到达时间,默认100ms
                data_Collect_Index = 0;//数据索引清零
                #region 数据超时处理
                //wdp 0615
                timeOutRestartCount++;
                //数据处理超时判断
                if (timeOutRestartCount > timeOutRestart * 1000 / timerOfSerialDataDealing.Interval)
                {
                    if (firstTimeOut)
                    {
                        firstTimeOut = false;
                        //此处保存EDF文件---
                        //timerOfEDFSaving.Stop();
                        EDFFileSaving(null, null);
                    }
                    timeOutRestartCount = 0;
                    if (moduleTest.CommNeuroOpen_WithoutWarning() == 0)
                    {
                        //发送通道开关命令
                        int leadCode = Convert.ToInt32(commonDataPool.leadCodeList[commonDataPool.currentLeadConfig]);
                        string leadCodeCmd = moduleTest.GenerateMsgCmd("95", "0B", leadCode.ToString("X6"));
                        isComSendSucceed = moduleTest.CommTxOneA5A5Message(leadCodeCmd);
                        //发送开始数据采集命令
                        moduleTest.CommTxOneA5A5Message(startDataCollectingCmd);
                        //方波标定状态
                        if (commonDataPool.isSquareWaveDemarcated)
                        {
                            DemarcateSquareWave();
                            DemarcateSquareWave();
                        }

                        //此处更新EDF文件开始时间
                        timeOfClear = timeOfStartCollecting;
                    }
                }
                #endregion
                #region 数据接收超时弹框显示
                timeOutPopupCount++;
                //弹框数据超时
                if (timeOutPopupCount > timeOutPopup * 1000 / timerOfSerialDataDealing.Interval)
                {
                    //弹窗显示数据接收异常事件
                    //DataRecFormShow();
                    //文本高亮显示数据接收异常事件
                    myMainForm.DataRecExceptionShow(isComSendSucceed, moduleTest.IsOpen, firstTimeOut);
                    timeOutPopupCount = 0;
                    isDataRecWarningShown = true;
                }
                #endregion
                #region 硬件心跳包数据
                if (timeOutHeartBeatCount > timeOutHeartBeat*1000/timerOfSerialDataDealing.Interval)
                {
                    isComSendSucceed = moduleTest.CommTxOneA5A5Message(heartbeatCmd);
                    timeOutHeartBeatCount = 0;
                }
                else
                {
                    timeOutHeartBeatCount++;
                }
                #endregion
                #region 正常数据处理
                if (serailPortDataCount > 0)
                {
                    //wdp 0615
                    timeOutRestartCount = 0;
                    firstTimeOut = true;
                    //wdp 0624
                    timeOutPopupCount = 0;
                    //数据正常接收
                    if (isDataRecWarningShown == true)
                    {
                        //DataRecFormHide();
                        myMainForm.DataRecExceptionHide();
                        isDataRecWarningShown = false;
                    }
                    oneMsgLength = serailPortDataCount;
                    if (EEGLeft != null)
                    {
                        bytesLeft = EEGLeft.Count();
                        oneMsgLength = bytesLeft + serailPortDataCount;
                        oneMsg = new byte[oneMsgLength];
                        //EEGLeft.CopyTo(oneMsg, 0);
                        for (i = 0; i < bytesLeft; i++)
                        {
                            oneMsg[i] = EEGLeft[i];
                            if (i > 0 && oneMsg[i] == 0xA5 && oneMsg[i - 1] == 0xA5)
                            {
                                IndexOfA5A5.Add(i - 1);
                            }
                        }
                    }
                    else
                    {
                        oneMsg = new byte[oneMsgLength];
                    }
                    for (i = 0; i < serailPortDataCount; i++)
                    {
                        oneMsg[bytesLeft + i] = Convert.ToByte(commonDataPool.queueOfSerialPortData.Dequeue());
                        if ((bytesLeft + i) > 0 && oneMsg[bytesLeft + i] == 0xA5 && oneMsg[bytesLeft + i - 1] == 0xA5)
                        {
                            IndexOfA5A5.Add(bytesLeft + i - 1);
                        }
                    }
                    if (IndexOfA5A5.Count < 1 || oneMsg.Length < 10)
                        return;
                    //计算数据点间时间间隔（ms）
                    timeInterval = timerOfSerialDataDealing.Interval / IndexOfA5A5.Count;//本次数据每个数据点间间隔
                    try
                    {
                        for (i = 0; i < IndexOfA5A5.Count - 1; i++)
                        {
                            if ((oneMsg[Convert.ToInt32(IndexOfA5A5[i + 1]) - 1] == 0xAA && oneMsg[Convert.ToInt32(IndexOfA5A5[i + 1]) - 2] == 0xAA))
                            {
                                int MsgLength = (int)IndexOfA5A5[i + 1] - (int)IndexOfA5A5[i];//一条完整EEG报告的长度
                                int startIndex = (int)IndexOfA5A5[i];//一条完整EEG报告在oneMsg中的起始位置
                                if (MsgLength != 82 && MsgLength != 10 && MsgLength != 13)
                                {
                                    continue;
                                }
                                string completeMsg = null;
                                for (int j = 0; j < MsgLength; j++)
                                {
                                    completeMsg += oneMsg[startIndex + j].ToString("X2");
                                }
                                ParseEEGData(completeMsg);//对一条EEG报告进行处理
                                completeMsg = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(typeof(NeuroControl), "解析EEG数据出错：" + ex.Message);
                        return;
                    }
                    //处理最后一条可能完整的EEG报告数据
                    try
                    {
                        int LeftMsgLength = oneMsg.Length - (int)IndexOfA5A5[IndexOfA5A5.Count - 1];//一条完整EEG报告的长度
                        if (LeftMsgLength <= 0)
                        {
                            EEGLeft = null;
                            return;
                        }
                        int LeftstartIndex = (int)IndexOfA5A5[IndexOfA5A5.Count - 1];//一条完整EEG报告在oneMsg中的起始位置

                        string incompleteMsg = null;
                        if (oneMsg[oneMsg.Length - 1] == 0xAA && oneMsg[oneMsg.Length - 2] == 0xAA)//以0xAA 0xAA结尾，则为完整的EEG报告
                        {
                            for (int j = 0; j < LeftMsgLength; j++)
                            {
                                incompleteMsg += oneMsg[LeftstartIndex + j].ToString("X2");
                            }
                            ParseEEGData(incompleteMsg);//将完整的一条EEG报告加入EEG消息队列，程序复杂后可能会耽误时间，以后还需考虑
                            incompleteMsg = null;
                            EEGLeft = null;
                        }
                        else
                        {
                            EEGLeft = new byte[LeftMsgLength];
                            for (i = 0; i < LeftMsgLength; i++)
                            {
                                EEGLeft[i] = oneMsg[i + LeftstartIndex];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog(typeof(NeuroControl), "处理最后一条可能完整的EEG报告数据:" + ex.Message);
                    }
                }
                #endregion
            }
            
        }
       
        /// <summary>
        /// 接收数据显示窗体
        /// </summary>
        private void DataRecFormShow()
        {
            if (myDataRecWarningForm.InvokeRequired)
            {                
                dataRecWarningFormShow datashow = new dataRecWarningFormShow(DataRecFormShow);
                myDataRecWarningForm.Invoke(datashow);
            }
            else
            {
                //myDataRecWarningForm.Hide();
                myDataRecWarningForm.Show();
                myDataRecWarningForm.BringToFront();
            }
        }
        /// <summary>
        /// 接收数据隐藏窗体
        /// </summary>
        public void DataRecFormHide()
        {
            if (myDataRecWarningForm.InvokeRequired)
            {
                dataRecWarningFormHide datashow = new dataRecWarningFormHide(DataRecFormHide);
                myDataRecWarningForm.Invoke(datashow);
            }
            else
            {
                myDataRecWarningForm.Hide();
            }
        }
        //序号	长度	   格式	       描述
        //01	2	   0xA5A5	   帧头
        //02	1	   type	       值为0x85，类型指示，表示此帧为采样数据结果（附录1）
        //03	1	   count	   递增计数器，范围0~99
        //04	3	  EKG_data	   EKG数据区，每通道3字节，为ADC采样的直接结果
        //05	3*24  EEG_data	   EEG数据区，24个通道则数据顺序为第1通道、第2通道、...、第24通道，每通道3字节，为ADC采样的直接结果
        //06	1	    cs	       序号02~05数据的异或校验码（附录2）
        //07	2	  0xAAAA	   帧尾
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="OneMsg"></param>
        public void ParseEEGData(string OneMsg)
        {
            if ((hardware_firmware_software_correct==1) && (commonDataPool.startDataCollectingFlag == true))
            {
                if (OneMsg == stopDataCollectingCmdAck)//接收到停止接收数据回复命令
                {
                    stopDataCollecting();
                }
                else
                {
                    //电极脱落检测
                    testElectrodeFalloff(OneMsg);
                    //重新开机命令检测
                    restartEEGCollect(OneMsg);

                    //采集数据入队
                    if (OneMsg.Length != 164)//一条标准采集数据有82字节
                        return;
                    if (!(OneMsg.Substring(0, 4) == "A5A5" || OneMsg.Substring(OneMsg.Length - 4, OneMsg.Length) == "AAAA"))
                        return;
                    if (CheckDataRight(OneMsg) > 0)//校验出错
                        return;

                    //DateTime timeofDataArrival;//按采样计数器和初始时间插值产生的数据时间 zt
                    if (firstDataFlag)
                    {
                        firstDataFlag = false;
                        firstTimeOfStartCollecting = timeOfStartCollecting;//一个EEG文件的最开始时间
                        timeOfClear = timeOfStartCollecting; //清除数据的时间
                        timeofDataArrival = firstTimeOfStartCollecting;
                    }
                    data_Collect_Index++;
                    timeofDataArrival = timeOfStartCollecting.AddMilliseconds(data_Collect_Index * timeInterval);//当前时间偏移数据起始计数器的时间

                    //string neuroData = timeofDataArrival.ToString() + ";" + OneMsg;
                    string neuroData = timeofDataArrival.ToString("yyyy-MM-dd HH:mm:ss.fff") + ";" + OneMsg;
                    //string neuroData = timeofDataArrival.Ticks + ";" + OneMsg; // by zt
                    commonDataPool.queueOfNeurodata.Enqueue(neuroData);
                    //defaultTime = new TimeSpan(0, 5, 0);
                    ////如果时间大于defaultTime，则保存文件、清空数据 by zt
                    //if ((timeofDataArrival - timeOfClear) >= defaultTime)
                    //{
                    //    newEDFFile.Header.StartDateTime = timeOfClear;
                    //    newEDFFile.Header.StartTimeEDF = timeOfClear.ToString("HH.mm.ss");//头文件起始时间改变
                    //    newEDFFile.Header.EndTimeEDF = timeofDataArrival;//头文件结束时间改变
                    //    //保存最后一个点
                    //    if (newEDFFile != null)
                    //    {
                    //        string OneData = OneMsg.Substring(8, 150);
                    //        CompleteLockedData(numberOfLockData, OneData);
                    //    }
                    //    DataCollectSaveNewEDFFile(timeOfClear.ToString("yyyyMMddHHmmss"));//保存文件
                    //    newEDFFile.DataHex.Clear();//清空数据
                    //    timeOfClear = timeofDataArrival;
                    //}
                    //EDF文件添加数据 by --zt
                    if (newEDFFile != null)
                    {
                        string OneData = OneMsg.Substring(8, 150);
                        CompleteLockedData(numberOfLockData, OneData);
                    }
                }
            }
            else
            {
                //开机自检数据核对，包括硬件版本号、软件版本号、固件版本号
                checkPowerOnInfo(OneMsg);
                if (hardware_firmware_software_correct == 0)//为0时参数不正确，为-1时未检测，为1时检测正确
                {
                    stopDataCollecting();
                    hardware_firmware_software_correct = -1;
                    MessageBox.Show("软硬件版本不正确", " 开机自检提示", MessageBoxButtons.OK);
                    return;
                }
                if (hardware_firmware_software_correct==1 && commonDataPool.recStarDataCollectingAck==true)
                {
                   commonDataPool.startDataCollectingFlag = true;//收到测试开始回复命令
                }
            }
        }

        /// <summary>
        /// 每隔一段时间进行一次EDF文件存储
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void EDFFileSaving(object source, System.Timers.ElapsedEventArgs e)
        {
            if (newEDFFile != null) 
            {
                newEDFFile.Header.StartDateTime = timeOfClear;
                newEDFFile.Header.StartTimeEDF = timeOfClear.ToString("HH.mm.ss");//头文件起始时间改变
                newEDFFile.Header.EndTimeEDF = timeofDataArrival;//头文件结束时间改变
            
                DataCollectSaveNewEDFFile(timeOfClear.ToString("yyyyMMddHHmmss"));//保存文件
                newEDFFile.DataHex.Clear();//清空数据
                timeOfClear = timeofDataArrival;
            }
            LogHelper.WriteLog(typeof(NeuroControl), "保存文件");
            
        }

        public int IsHardware_Firmware_Software_Correct()
        {
            return hardware_firmware_software_correct;
        }

        /// <summary>
        /// 停止采集数据
        /// </summary>
        public void stopDataCollecting()
        {
            commonDataPool.recStarDataCollectingAck = false;
            commonDataPool.startDataCollectingFlag = false;
            //moduleTest.ClearPortData();
            stopTimer();
            moduleTest.CommNeuroClose();
        }

        /// <summary>
        /// 触发EEG窗体中的停止按钮事件
        /// </summary>
        public void stopEEGDataCollecting()
        {
            myDataCollectForm.stopEEGDataCollecting();
        }
        /// <summary>
        /// 数据校验函数
        /// </summary>
        /// <param name="data"></param>
        /// <returns>0,校验正确，非0，校验错误</returns>
        public int CheckDataRight(string data)
        {
            Int16 i;
            byte checksum = 0;
            byte[] buf = HexToByte(data);
            for (i = 2; i < buf.Length - 2; i++)
            {
                checksum ^= buf[i];
            }
            return checksum;
        }

        public void ModuleInit()
        {
        }

        /// <summary>
        /// 检测是否有电极脱落  
        /// </summary>
        /// <param name="oneMsg"></param>
        public void testElectrodeFalloff(string oneMsg)
        {
            int data_p,data_n,data_result;
            int x = 0x800000;
            if (oneMsg.Length == 26)
            {
                if (oneMsg.Substring(4, 2) == "87")
                {
                    //01	2	0xA5A5	帧头
                    //02	1	type	值为0x87，类型指示，表示电极脱离情况（附录1）
                    //03	3	data_p	通道1-24的正电极脱离情况,每位0表示不脱离，1表示脱离
                    //04	3	data_n	通道1-24的负电极脱离情况,每位0表示不脱离，1表示脱离
                    //05	1	data_ref	参考电极脱离情况,0表示不脱离，1表示脱离
                    //06	1	cs	    序号02~05数据的异或校验码（附录2）
                    //07	2	0xAAAA	帧尾
                    // data_p和data_n数据对应通道情况： 数据     byte1          |      byte2              |         byte3 
                    //                                 通道 8 7 6 5 4 3 2 1   | 16 15 14 13 12 11 10 9   | 24 23 22 21 20 19 18 17

                    electrodeFallOffTime = DateTime.Now;//记录最近一次电极脱落时间
                    data_p = Convert.ToInt32(oneMsg.Substring(10, 2)+oneMsg.Substring(8,2)+oneMsg.Substring(6, 2), 16);
                    data_n = Convert.ToInt32(oneMsg.Substring(16, 2) + oneMsg.Substring(14, 2) + oneMsg.Substring(12, 2), 16);
                    data_result=data_p | data_n;
                    fallOffChannelNum.Clear();
                    for(short i=24;i>0;i--)
                    {
                        if((data_result & x)>0)
                        {
                            channelOfElectrodeFalloff(i);
                        }
                        x = x >> 1;
                    }
                }
            }
            if ((DateTime.Now.Subtract(electrodeFallOffTime)).Seconds > electrodeFallOffTimeout)
            {
                fallOffChannelNum.Clear();
            }
        }
		
        /// <summary>
        /// 重新开机命令检测
        /// </summary>
        /// <param name="oneMsg"></param>
        public void restartEEGCollect(string oneMsg)
        {
            if (oneMsg.Length == 14)
            {
                if (oneMsg.Substring(4, 2) == "86")
                {
                    moduleTest.CommTxOneA5A5Message(startDataCollectingCmd);
                    //if (moduleTest.CommTxOneA5A5Message(startDataCollectingCmd) != 0)
                    //{
                        //MyTestState = NeuroControl.TestState.Nil;
                        //return;
                    //}
                }
            }
        }
		
        /// <summary>
        /// 开机检测参数校对
        /// </summary>
        /// <param name="oneMsg"></param>
        public void checkPowerOnInfo(string oneMsg)
        {
            if (oneMsg.Length == 20)
            {
                if (oneMsg.Substring(4, 2) == "97")
                {
                    if(oneMsg.Substring(6,2)=="61")
                    {
                        if (oneMsg.Substring(8, 6) != commonDataPool.hardware.ToString("X6"))
                            hardware_correct = 0;
                        else
                            hardware_correct = 1;
                    }
                    else if(oneMsg.Substring(6,2)=="62")
                    {
                        if (oneMsg.Substring(8, 6) != commonDataPool.firmware.ToString("X6"))
                            firmware_correct = 0;
                        else
                            firmware_correct = 1;
                    }
                    else if(oneMsg.Substring(6,2)=="63")
                    {
                        if (oneMsg.Substring(8, 6)!=commonDataPool.software.ToString("X6"))
                            software_correct = 0;
                        else
                            software_correct = 1;
                        hardware_firmware_software_correct = hardware_correct * firmware_correct * software_correct;
                        LogHelper.WriteLog(typeof(NeuroControl), hardware_firmware_software_correct.ToString());
                    }
                    else if (oneMsg.Substring(6, 2) == "67")
                    {
                        //信号源标定ACK
                        moduleTest.CommNeuroClose();
                        MessageBox.Show("信号源标定成功！","信号源标定", MessageBoxButtons.OK);
                    }
                    else if(oneMsg.Substring(6,2)=="64")
                    {
                        commonDataPool.isSquareWaveDemarcated = true;
                    }
                    else if(oneMsg.Substring(6,2)=="65")
                    {
                        commonDataPool.isSquareWaveDemarcated = false;
                    }
                    else if(oneMsg.Substring(6,2)=="08")
                    {
                        commonDataPool.recStarDataCollectingAck = true;
                    }
                }
            }
        }

        /// <summary>
        /// 从string类型转换到hex
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 记录电极掉落通道号
        /// </summary>
        /// <param name="channelNum"></param>
        private void channelOfElectrodeFalloff(short channelNum)
        {
            fallOffChannelNum.Add(channelNum);
        }

        /// <summary>
        /// 获取电极脱落通道
        /// </summary>
        /// <returns></returns>
        public List<short> GetChannelsOfElectrodeFallOff()
        {
            return fallOffChannelNum;
        }

        /// <summary>
        /// 清除电极脱落数据
        /// </summary>
        public void ClearChannelOfElectrodeFallOff()
        {
            fallOffChannelNum.Clear();
        }

        public void ElectrodeFallOffWarning()
        {
            myMainForm.ElectrodeFallOffWarning();
        }

        public void ElectrodeFallOffUnwarning()
        {
            myMainForm.ElectrodeFallOffUnwarning();
        }

        /// <summary>
        /// 补全缺省数据
        /// 现在的算法是：让缺省的数据跟此时的数据一致。以后改进算法。暂未用到。
        /// --by zt
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        public void CompleteLockedData(int number,string data) 
        {
            for (int k = 0; k <= numberOfLockData; k++)
            {
                newEDFFile.DataHex.Append(data);
            }
        }
        public void setFirstDataFlag(bool firstDataFlag)
        {
            this.firstDataFlag = firstDataFlag;
        }

        /// <summary>
        /// 开机自检参数传递
        /// </summary>
        public void GetRegisterChannelPara()
        {
            int leadCode=Convert.ToInt32(commonDataPool.leadCodeList[commonDataPool.currentLeadConfig]);
            moduleTest.SetRegisterChannelPara(commonDataPool.hardware,commonDataPool.firmware,commonDataPool.software, commonDataPool.registerConfig3_BiasSignal,commonDataPool.registerLoff_ElectrodeDroppedOff,commonDataPool.channelConfig,commonDataPool.loff_Sensp,commonDataPool.loff_Senen, commonDataPool.mm,commonDataPool.nn,commonDataPool.jj,leadCode);
        }

        public void NeuroQuit()
        {
            string stopDataCollectingCmd = "A5A5950A0000009FAAAA";
            moduleTest.CommTxOneA5A5Message(stopDataCollectingCmd);
            SaveXmlConfig();
        }

        public void Demarcate(int channelSelected,int demarcation)
        {
            string data = channelSelected.ToString("X2") + demarcation.ToString("X4");
            if(moduleTest.IsOpen==false)
            {
                foreach (string Com_Num in System.IO.Ports.SerialPort.GetPortNames())
                {
                    commonDataPool.neuroComName = Com_Num;
                }
               // commonDataPool.neuroBaud = "230400";
                commonDataPool.neuroBaud = "460800";
                moduleTest.CommNeuroConfig(commonDataPool.neuroComName,commonDataPool.neuroBaud);
                if(moduleTest.CommNeuroOpen()!=0)
                    return;
            }
            if(moduleTest.CommTxOneA5A5Message("67",data)!=0)
            {
                return;
            }
            startEEGTimer(100);
            startSerialPortDataDealingTimer();//等待ack数据
            Thread.Sleep(1000);
            stopDataCollecting();
        }

        /// <summary>
        /// 标定方波信号
        /// </summary>
        public void DemarcateSquareWave()
        {
            if(myTestState!=NeuroControl.TestState.DT)
            {
                MessageBox.Show("请在数据采集状态下进行标定！");
            }
            else
            {
                if(moduleTest.IsOpen==true)
                {
                    //方波信号标定数据类型为“0x64”
                    if(moduleTest.CommTxOneA5A5Message("64","ffffff")!=0)
                    {
                        SetDemarcateState(false);
                        return;
                    }
                    SetDemarcateState(true);
                }
                else
                {
                    SetDemarcateState(false);
                    MessageBox.Show("请打开串口再进行标定");
                }
            }
        }
        /// <summary>
        /// 停止标定方波信号
        /// </summary>
        public void StopDemarcateSquareWave()
        {
            if (myTestState != NeuroControl.TestState.DT)
            {
                MessageBox.Show("请在数据采集状态下进行标定！");
            }
            else
            {
                if (moduleTest.IsOpen == true)
                {
                    //方波信号标定数据类型为“0x64”
                    if (moduleTest.CommTxOneA5A5Message("65", "ffffff") != 0)
                    {
                        return;
                    }
                    SetDemarcateState(false);
                }
                else
                {
                    MessageBox.Show("串口已关闭");
                    SetDemarcateState(false);
                }
            }
        }

        /// <summary>
        /// 将配置界面串口名称传递至公共数据池
        /// </summary>
        /// <param name="neuroCom">COM号</param>
        public void setSerialComName(string neuroCom)
        {
            commonDataPool.neuroComName = neuroCom;
        }

        /// <summary>
        /// 将配置界面波特率传递至公共数据池
        /// </summary>
        /// <param name="neuroBaud">波特率</param>
        public void setSerialBaud(string neuroBaud)
        {
            commonDataPool.neuroBaud = neuroBaud;
        }
    }
}
