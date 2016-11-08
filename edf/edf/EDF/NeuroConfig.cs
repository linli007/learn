using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ElectroencephalographController
{
    interface INeuroConfig
    {
        void SetDefaultConfig();
    }

    [Serializable]
    class NeuroConfig : INeuroConfig
    {
        #region 定义配置文件中保存的变量
        public int hardware;//0x61	1~0xFFFFFF	硬件电路板版本号
        public int firmware;//0x62	1~0xFFFFFF	固件版本号
        public int software;//0x63	1~0xFFFFFF	上位机软件版本号
        public int registerConfig3_BiasSignal;//0x43	CONFIG3寄存器的值	写：Bias信号相关（注2）
        public int registerLoff_ElectrodeDroppedOff;//0x44	LOFF寄存器的值	写：电极掉落检测配置相关（注2）
        public int channelConfig1;//0x45-0x4C	CH1SET-CH8SET	写：通道1-8配置相关（注2）
        public int channelConfig2;
        public int channelConfig3;
        public int channelConfig4;
        public int channelConfig5;
        public int channelConfig6;
        public int channelConfig7;
        public int channelConfig8;
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
        public int bias_Loff_Sens;//是否开启bias极的脱离检测，默认0不开启

        ///LOFF寄存器的COMP_TH2~COMP_TH0属性，其关于普通电极的脱离检测灵敏度
        ///正极0=95%（默认），1=92.5%，2=90%，3=87.5%，4=85%，5=80%，6=75%，7=70%
        ///负极0= 5%（默认），1=7 .5%，2=10%，3=12.5%，4=15%，5=20%，6=25%，7=30%
        ///xx%值越大则越灵敏，但也越容易误报。
        public int comp_th;

        ///LOFF寄存器的ILEAD_OFF1~ILEAD_OFF0属性，其关于普通电极的脱离检测注入电流大小
        ///0=6nA（默认），1=24nA，2=6uA，3=24uA
        ///电流越小对人体影响、对信号采集影响越小，但是电流越小在做电极脱离检测时候准确度越低
        public int ilead_off;

        ///LOFF寄存器的FLEAD_OFF1~FLEAD_OFF0属性，其关于电极脱离检测用的是交流信号还是直流信号
        ///0=DC信号（默认)，1=7.8Hz交流信号，2=31.2Hz交流信号，3=fDR/4交流信号
        public int flead_off;
		//导联参数 by zt
		public Hashtable leadList;//导联配置
        public Hashtable leadCodeList;//导联编码
		public Hashtable leadSource;//导联源
		public Hashtable defaultLeadSource;//系统默认导联源
		public string currentLeadConfig;//当前导联配置情况
        public int demarcateCV;//标定电压

        //以下变量为FORMEEG中涉及到图纸显示所需要的全局变量

        public double speed;                     //走纸速度                                 -- by lxl
        public int sensitivity;                 //灵敏度                                    -- by lxl
        public double pixelPerCM;                   //一厘米有多少像素                           -- by lxl
        public Color chartColor0;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor1;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor2;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor3;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor4;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor5;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor6;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor7;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor8;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor9;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor10;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor11;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor12;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor13;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor14;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor15;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor16;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor17;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor18;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor19;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor20;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor21;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor22;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor23;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor24;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Color chartColor25;            //画图时的信道的color，后面的数字与下标对应      -- by lxl
        public Boolean signalDisplay0;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay1;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay2;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay3;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay4;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay5;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay6;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay7;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay8;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay9;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay10;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay11;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay12;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay13;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay14;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay15;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay16;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay17;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay18;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay19;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay20;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay21;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay22;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay23;         //信道是否显示，后面的数字对应下标              -- by lxl
        public Boolean signalDisplay24;         //信道是否显示，后面的数字对应下标              -- by lxl
        public double secondsPerPage;             //X轴右多少秒，用于配置1cm为1s时用             -- by lxl

        #endregion

        public void SetDefaultConfig()
        {
            //测试软硬件版本不一致情况
            //hardware = 0x01;
            //firmware = 0x01;
            //software = 0x01;
            //测试软硬件版本一致情况
            hardware = 0x010101;
            firmware = 0x010101;
            software = 0x010101;
            registerConfig3_BiasSignal = 0;
            registerLoff_ElectrodeDroppedOff = 0;
            channelConfig1 = 0;
            channelConfig2 = 0;
            channelConfig3 = 0;
            channelConfig4 = 0;
            channelConfig5 = 0;
            channelConfig6 = 0;
            channelConfig7 = 0;
            channelConfig8 = 0;
            loff_Sensp = 0;
            loff_Senen = 0;
            mm = 0;
            nn = 0;
            jj = 0;
            bias_Loff_Sens = 0;
            comp_th = 0;

            #region 导联参数 --by zt
            leadList = new Hashtable();
            leadCodeList = new Hashtable();
            leadSource = new Hashtable();
            currentLeadConfig = "default";
            defaultLeadSource = new Hashtable();
			ArrayList defaultLeadArrayList = new ArrayList();
            //string[] leadname = new string[19] { "FP1", "FP2", "F3", "F4", "F7", "F8", "C3", "C4", "T3", "T4", "P3", "P4", "T5", "T6", "O1","O2","Fz", "Cz", "Pz" };
            //for (int i = 0; i < 19;i++ )
            //    defaultLeadSource.Add(i+1, leadname[i]);            
            //for (int i = 0; i < 19; i++)
            //{
            //    if (leadname[i] != "")
            //         defaultLeadArrayList.Add(leadname[i] + "-GND");
            //}
            //string[] leadname = new string[19] { "FP1", "FP2", "F3", "F4", "F7", "F8", "C3", "C4", "T3", "T4", "P3", "P4", "T5", "T6", "O1", "O2", "Fz", "Cz", "Pz" };
			string[] leadname = new string[18] { "FP1", "F7", "F3", "T3", "C3", "T5", "P3", "O1", "FP2", "F8", "F4", "T4", "C4", "T6", "P4", "O2", "Fz", "Cz"};
            for (int i = 0; i < leadname.Length; i++)
                defaultLeadSource.Add(i + 1, leadname[i]);
            for (int i = 0; i < leadname.Length; i++)
            {
                if (leadname[i] != "")
                    defaultLeadArrayList.Add(leadname[i] + "-GND");
            }
            leadList.Add(currentLeadConfig, defaultLeadArrayList);
            leadCodeList.Add(currentLeadConfig, 0xfffff8);
            demarcateCV = 0;

            //以下变量为FORMEEG中涉及到图纸显示所需要的全局变量  -- by lxl
            chartColor0 = Color.FromArgb(230, 239, 252);
            chartColor1 = Color.FromArgb(28, 114, 201);
            chartColor2 = Color.FromArgb(17, 150, 65); 
            chartColor3 = Color.FromArgb(82, 100, 129); 
            chartColor4 = Color.FromArgb(151, 13, 155); 
            chartColor5 = Color.FromArgb(118, 116, 13);
            chartColor6 = Color.FromArgb(16, 109, 152);
            chartColor7 = Color.FromArgb(9, 127, 116);
            chartColor8 = Color.FromArgb(130, 0, 19);
            chartColor9 = Color.FromArgb(13, 89, 181);
            chartColor10 = Color.FromArgb(26, 141, 181);
            chartColor11 = Color.FromArgb(218, 76, 27);
            chartColor12 = Color.FromArgb(10, 30, 84);
            chartColor13 = Color.FromArgb(28, 114, 201);
            chartColor14 = Color.FromArgb(17, 150, 65);
            chartColor15 = Color.FromArgb(82, 100, 129);
            chartColor16 = Color.FromArgb(151, 13, 155);
            chartColor17 = Color.FromArgb(118, 116, 13);
            chartColor18 = Color.FromArgb(16, 109, 152);
            chartColor19 = Color.FromArgb(9, 127, 116);
            chartColor20 = Color.FromArgb(130, 0, 19);
            chartColor21 = Color.FromArgb(13, 89, 181);
            chartColor22 = Color.FromArgb(26, 141, 181);
            chartColor23 = Color.FromArgb(218, 76, 27);
            chartColor24 = Color.FromArgb(10, 30, 84);
            chartColor25 = Color.FromArgb(28, 114, 201);
            signalDisplay0 = true;
            signalDisplay1 = true;
            signalDisplay2 = true;
            signalDisplay3 = true;
            signalDisplay4 = true;
            signalDisplay5 = true;
            signalDisplay6 = true;
            signalDisplay7 = true;
            signalDisplay8 = true;
            signalDisplay9 = true;
            signalDisplay10 = true;
            signalDisplay11 = true;
            signalDisplay12 = true;
            signalDisplay13 = true;
            signalDisplay14 = true;
            signalDisplay15 = true;
            signalDisplay16 = true;
            signalDisplay17 = true;
            signalDisplay18 = true;
            signalDisplay19 = true;
            signalDisplay20 = true;
            signalDisplay21 = true;
            signalDisplay22 = true;
            signalDisplay23 = true;
            signalDisplay24 = true;
            speed = 1D;
            sensitivity = 1000;
            secondsPerPage = 23;
            pixelPerCM = -1;
            #endregion
        }
    }
}
