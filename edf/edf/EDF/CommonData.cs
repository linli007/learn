using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElectroencephalographController
{
    public partial class CommonData
    {
        #region 定义EDF文件操作相关公共变量

        public Queue queueOfEDFdata = Queue.Synchronized(new Queue());//EDF文件数据队列
        public Queue queueOfPlayBack = Queue.Synchronized(new Queue());//EDF回放文件队列

        #endregion

        #region 定义EDF文件以外操作相关公共变量

        public string replayDir;
        public Queue queueOfNeurodata = Queue.Synchronized(new Queue());//定义实时采集数据时存放数据队列
        //表示是否开始接收硬件设备发送过来的数据，为true时上位机开始接收数据，为false时上位机停止接收数据
        public bool recStarDataCollectingAck;
        public bool startDataCollectingFlag;
        public Queue queueOfSerialPortData = Queue.Synchronized(new Queue());//存放串口收到的实时字符串数据
        public string neuroComName;
        public string neuroBaud;
        public bool filt50data;//是否滤除50Hz数据
        public bool isECGShown=true;//ECK信号是否显示
        public bool isSquareWaveDemarcated;//是否打开方波标定
        #endregion

        #region 定义配置文件中保存的变量       
        public int hardware;//0x61	1~0xFFFFFF	硬件电路板版本号
        public int firmware;//0x62	1~0xFFFFFF	固件版本号
        public int software;//0x63	1~0xFFFFFF	上位机软件版本号
        public int registerConfig3_BiasSignal;//0x43	CONFIG3寄存器的值	写：Bias信号相关（注2）
        public int registerLoff_ElectrodeDroppedOff;//0x44	LOFF寄存器的值	写：电极掉落检测配置相关（注2）
        public int[] channelConfig = new int[8];//0x45-0x4C	CH1SET-CH8SET	写：通道1-8配置相关（注2）
        public int loff_Sensp;//0x4F	LOFF_SENSP	写：掉落检测正极配置（注2）
        public int loff_Senen;//0x50	LOFF_SENSN	写：掉落检测负极配置（注2）
        public int mm;
        public int nn;
        public int jj;//0x71	0xmm 0xnn 0xjj,其中mm、nn、jj的取值可以是：
        //0. 50 sps
        //1. 100 sps
        //2. 200 sps（默认）
        //3. 250 sps
        //4. 500 sps
        //5. 1k sps
        //	mm、nn、jj分别对应1-8、9-16、17-24通道的采样率，一般mm=nn=jj，但也可不同。
        //例如0x00 0x05 0x02表示1-8、9-16、17-24通道采样率为50sps、1ksps、200sps

        public Hashtable leadList;//导联配置  --by zt
        public Hashtable leadCodeList;//导联编码  --by wdp
        public Hashtable leadSource;//导联源  --by zt
        public Hashtable defaultLeadSource;//系统默认导联源  --by zt
		public string currentLeadConfig;//当前导联名称  --by wdp

        public int demarcateCV;//标定值  --by wdp

        //以下变量为FORMEEG中涉及到图纸显示所需要的全局变量

        public double speed;                                  //走纸速度                                   -- by lxl
        public int sensitivity;                            //灵敏度                                     -- by lxl
        public Boolean[] signalDisplay = new Boolean[25]; //信道是否显示                                -- by lxl
        public double secondsPerPage;                     //一页有多少秒，用于配置X轴为1cm=为1s时用        -- by lxl
        public System.Drawing.Color[] chartColor = new System.Drawing.Color[26];      //画图时的信道的color                         -- by lxl
        public double pixelPerCM;                             //一CM有多少像素点                             -- by lxl
        #endregion
    }
}
