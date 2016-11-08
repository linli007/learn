using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectroencephalographController
{
    /// <summary>
    /// 配置文件模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        #region 变量
        /// <summary>
        /// 配置Form
        /// </summary>
        private FormConfigure myConfigForm;
        
        /// <summary>
        /// 配置文件实例
        /// </summary>
        private NeuroConfig xmlConfig;
        
        /// <summary>
        /// 用于序列化和反序列化配置
        /// </summary>
        private INeuroFileSave<NeuroConfig> configManage;
        #endregion

        /// <summary>
        /// 初始化配置文件  
        /// </summary>
        private void FormConfigInit()
        {
            configManage = new NeuroFileSave<NeuroConfig>();
            try
            {
                xmlConfig = configManage.GetFromFile("config");
                ImportXmlData();
            }
            catch (Exception ex)
            {
              // System.Windows.Forms.MessageBox.Show("读配置文件信息出错！");
                if (xmlConfig == null)
                {
                    xmlConfig = new NeuroConfig();
                    xmlConfig.SetDefaultConfig();
                    ImportXmlData();
                    configManage.SaveToFile("config", xmlConfig);
                }
            }
            myConfigForm = new FormConfigure(moduleTest);
            myConfigForm.SetFormAsChild();
            myConfigForm.ReigisterNeuroControl(this);
            myMainForm.AddSubForm(myConfigForm);
            
        }

        /// <summary>
        /// ShowForm
        /// </summary>
        private void ShowConfigForm()
        {
            if (myConfigForm != null)
            {
                myConfigForm.ShowForm(myDataCollectForm.IsChartEmpty());
            }
        }

        /// <summary>
        /// 导入配置文件初始化公共数据池  
        /// </summary>
        private void ImportXmlData()
        {
            commonDataPool.hardware = xmlConfig.hardware;
            commonDataPool.firmware = xmlConfig.firmware;
            commonDataPool.software = xmlConfig.software;
            commonDataPool.registerConfig3_BiasSignal = xmlConfig.registerConfig3_BiasSignal;
            commonDataPool.registerLoff_ElectrodeDroppedOff = xmlConfig.registerLoff_ElectrodeDroppedOff;
            commonDataPool.channelConfig[0] = xmlConfig.channelConfig1;
            commonDataPool.channelConfig[1] = xmlConfig.channelConfig2;
            commonDataPool.channelConfig[2] = xmlConfig.channelConfig3;
            commonDataPool.channelConfig[3] = xmlConfig.channelConfig4;
            commonDataPool.channelConfig[4] = xmlConfig.channelConfig5; 
            commonDataPool.channelConfig[5] = xmlConfig.channelConfig6;
            commonDataPool.channelConfig[6] = xmlConfig.channelConfig7;
            commonDataPool.channelConfig[7] = xmlConfig.channelConfig8;
            commonDataPool.loff_Sensp = xmlConfig.loff_Sensp;
            commonDataPool.loff_Senen = xmlConfig.loff_Senen;
            commonDataPool.mm = xmlConfig.mm;
            commonDataPool.nn = xmlConfig.nn;
            commonDataPool.jj = xmlConfig.jj;
            commonDataPool.leadList = xmlConfig.leadList;
            commonDataPool.leadCodeList = xmlConfig.leadCodeList;
            commonDataPool.leadSource = xmlConfig.leadSource;
            commonDataPool.defaultLeadSource = xmlConfig.defaultLeadSource;
            commonDataPool.currentLeadConfig = xmlConfig.currentLeadConfig;
            commonDataPool.demarcateCV = xmlConfig.demarcateCV;

            //以下变量为FORMEEG中涉及到图纸显示所需要的全局变量 -- by lxl
			commonDataPool.speed = xmlConfig.speed;
            commonDataPool.secondsPerPage = xmlConfig.secondsPerPage;
            commonDataPool.pixelPerCM = xmlConfig.pixelPerCM;
            commonDataPool.sensitivity = xmlConfig.sensitivity;
            commonDataPool.chartColor[0] = xmlConfig.chartColor0;
            commonDataPool.chartColor[1] = xmlConfig.chartColor1;
            commonDataPool.chartColor[2] = xmlConfig.chartColor2;
            commonDataPool.chartColor[3] = xmlConfig.chartColor3;
            commonDataPool.chartColor[4] = xmlConfig.chartColor4;
            commonDataPool.chartColor[5] = xmlConfig.chartColor5;
            commonDataPool.chartColor[6] = xmlConfig.chartColor6;
            commonDataPool.chartColor[7] = xmlConfig.chartColor7;
            commonDataPool.chartColor[8] = xmlConfig.chartColor8;
            commonDataPool.chartColor[9] = xmlConfig.chartColor9;
            commonDataPool.chartColor[10] = xmlConfig.chartColor10;
            commonDataPool.chartColor[11] = xmlConfig.chartColor11;
            commonDataPool.chartColor[12] = xmlConfig.chartColor12;
            commonDataPool.chartColor[13] = xmlConfig.chartColor13;
            commonDataPool.chartColor[14] = xmlConfig.chartColor14;
            commonDataPool.chartColor[15] = xmlConfig.chartColor15;
            commonDataPool.chartColor[16] = xmlConfig.chartColor16;
            commonDataPool.chartColor[17] = xmlConfig.chartColor17;
            commonDataPool.chartColor[18] = xmlConfig.chartColor18;
            commonDataPool.chartColor[19] = xmlConfig.chartColor19;
            commonDataPool.chartColor[20] = xmlConfig.chartColor20;
            commonDataPool.chartColor[21] = xmlConfig.chartColor21;
            commonDataPool.chartColor[22] = xmlConfig.chartColor22;
            commonDataPool.chartColor[23] = xmlConfig.chartColor23;
            commonDataPool.chartColor[24] = xmlConfig.chartColor24;
            commonDataPool.chartColor[25] = xmlConfig.chartColor25;
            commonDataPool.signalDisplay[0] = xmlConfig.signalDisplay0;
            commonDataPool.signalDisplay[1] = xmlConfig.signalDisplay1;
            commonDataPool.signalDisplay[2] = xmlConfig.signalDisplay2;
            commonDataPool.signalDisplay[3] = xmlConfig.signalDisplay3;
            commonDataPool.signalDisplay[4] = xmlConfig.signalDisplay4;
            commonDataPool.signalDisplay[5] = xmlConfig.signalDisplay5;
            commonDataPool.signalDisplay[6] = xmlConfig.signalDisplay6;
            commonDataPool.signalDisplay[7] = xmlConfig.signalDisplay7;
            commonDataPool.signalDisplay[8] = xmlConfig.signalDisplay8;
            commonDataPool.signalDisplay[9] = xmlConfig.signalDisplay9;
            commonDataPool.signalDisplay[10] = xmlConfig.signalDisplay10;
            commonDataPool.signalDisplay[11] = xmlConfig.signalDisplay11;
            commonDataPool.signalDisplay[12] = xmlConfig.signalDisplay12;
            commonDataPool.signalDisplay[13] = xmlConfig.signalDisplay13;
            commonDataPool.signalDisplay[14] = xmlConfig.signalDisplay14;
            commonDataPool.signalDisplay[15] = xmlConfig.signalDisplay15;
            commonDataPool.signalDisplay[16] = xmlConfig.signalDisplay16;
            commonDataPool.signalDisplay[17] = xmlConfig.signalDisplay17;
            commonDataPool.signalDisplay[18] = xmlConfig.signalDisplay18;
            commonDataPool.signalDisplay[19] = xmlConfig.signalDisplay19;
            commonDataPool.signalDisplay[20] = xmlConfig.signalDisplay20;
            commonDataPool.signalDisplay[21] = xmlConfig.signalDisplay21;
            commonDataPool.signalDisplay[22] = xmlConfig.signalDisplay22;
            commonDataPool.signalDisplay[23] = xmlConfig.signalDisplay23;
            commonDataPool.signalDisplay[24] = xmlConfig.signalDisplay24;
        }

        /// <summary>
        /// 保存配置信息  
        /// </summary>
        public void SaveXmlConfig()
        {
            if (xmlConfig == null)
                return;
            //开机自检参数 --by wdp
            xmlConfig.hardware = commonDataPool.hardware;
            xmlConfig.firmware = commonDataPool.firmware;
            xmlConfig.software = commonDataPool.software;
            xmlConfig.registerConfig3_BiasSignal = commonDataPool.registerConfig3_BiasSignal;
            xmlConfig.registerLoff_ElectrodeDroppedOff = commonDataPool.registerLoff_ElectrodeDroppedOff;
            xmlConfig.channelConfig1 = commonDataPool.channelConfig[0];
            xmlConfig.channelConfig2 = commonDataPool.channelConfig[1];
            xmlConfig.channelConfig3 = commonDataPool.channelConfig[2];
            xmlConfig.channelConfig4 = commonDataPool.channelConfig[3];
            xmlConfig.channelConfig5 = commonDataPool.channelConfig[4];
            xmlConfig.channelConfig6 = commonDataPool.channelConfig[5];
            xmlConfig.channelConfig7 = commonDataPool.channelConfig[6];
            xmlConfig.channelConfig8 = commonDataPool.channelConfig[7];
            xmlConfig.loff_Sensp = commonDataPool.loff_Sensp;
            xmlConfig.loff_Senen = commonDataPool.loff_Senen;
            xmlConfig.mm = commonDataPool.mm;
            xmlConfig.nn = commonDataPool.nn;
            xmlConfig.jj = commonDataPool.jj;

            #region 导联配置参数  --by zt
            xmlConfig.leadList = commonDataPool.leadList;
            xmlConfig.leadCodeList = commonDataPool.leadCodeList;
            xmlConfig.leadSource = commonDataPool.leadSource;
            xmlConfig.defaultLeadSource = commonDataPool.defaultLeadSource;
			xmlConfig.currentLeadConfig = commonDataPool.currentLeadConfig;
            xmlConfig.demarcateCV = commonDataPool.demarcateCV;
            #endregion  

            //画图配置  --by lxl
            xmlConfig.chartColor0 = commonDataPool.chartColor[0];
            xmlConfig.chartColor1 = commonDataPool.chartColor[1];
            xmlConfig.chartColor2 = commonDataPool.chartColor[2];
            xmlConfig.chartColor3 = commonDataPool.chartColor[3];
            xmlConfig.chartColor4 = commonDataPool.chartColor[4];
            xmlConfig.chartColor5 = commonDataPool.chartColor[5];
            xmlConfig.chartColor6 = commonDataPool.chartColor[6];
            xmlConfig.chartColor7 = commonDataPool.chartColor[7];
            xmlConfig.chartColor8 = commonDataPool.chartColor[8];
            xmlConfig.chartColor9 = commonDataPool.chartColor[9];
            xmlConfig.chartColor10 = commonDataPool.chartColor[10];
            xmlConfig.chartColor11 = commonDataPool.chartColor[11];
            xmlConfig.chartColor12 = commonDataPool.chartColor[12];
            xmlConfig.chartColor13 = commonDataPool.chartColor[13];
            xmlConfig.chartColor14 = commonDataPool.chartColor[14];
            xmlConfig.chartColor15 = commonDataPool.chartColor[15];
            xmlConfig.chartColor16 = commonDataPool.chartColor[16];
            xmlConfig.chartColor17 = commonDataPool.chartColor[17];
            xmlConfig.chartColor18 = commonDataPool.chartColor[18];
            xmlConfig.chartColor19 = commonDataPool.chartColor[19];
            xmlConfig.chartColor20 = commonDataPool.chartColor[20];
            xmlConfig.chartColor21 = commonDataPool.chartColor[21];
            xmlConfig.chartColor22 = commonDataPool.chartColor[22];
            xmlConfig.chartColor23 = commonDataPool.chartColor[23];
            xmlConfig.chartColor24 = commonDataPool.chartColor[24];
            xmlConfig.chartColor25 = commonDataPool.chartColor[25];
            xmlConfig.signalDisplay0 = commonDataPool.signalDisplay[0];
            xmlConfig.signalDisplay1 = commonDataPool.signalDisplay[1];
            xmlConfig.signalDisplay2 = commonDataPool.signalDisplay[2];
            xmlConfig.signalDisplay3 = commonDataPool.signalDisplay[3];
            xmlConfig.signalDisplay4 = commonDataPool.signalDisplay[4];
            xmlConfig.signalDisplay5 = commonDataPool.signalDisplay[5];
            xmlConfig.signalDisplay6 = commonDataPool.signalDisplay[6];
            xmlConfig.signalDisplay7 = commonDataPool.signalDisplay[7];
            xmlConfig.signalDisplay8 = commonDataPool.signalDisplay[8];
            xmlConfig.signalDisplay9 = commonDataPool.signalDisplay[9];
            xmlConfig.signalDisplay10 = commonDataPool.signalDisplay[10];
            xmlConfig.signalDisplay11 = commonDataPool.signalDisplay[11];
            xmlConfig.signalDisplay12 = commonDataPool.signalDisplay[12];
            xmlConfig.signalDisplay13 = commonDataPool.signalDisplay[13];
            xmlConfig.signalDisplay14 = commonDataPool.signalDisplay[14];
            xmlConfig.signalDisplay15 = commonDataPool.signalDisplay[15];
            xmlConfig.signalDisplay16 = commonDataPool.signalDisplay[16];
            xmlConfig.signalDisplay17 = commonDataPool.signalDisplay[17];
            xmlConfig.signalDisplay18 = commonDataPool.signalDisplay[18];
            xmlConfig.signalDisplay19 = commonDataPool.signalDisplay[19];
            xmlConfig.signalDisplay20 = commonDataPool.signalDisplay[20];
            xmlConfig.signalDisplay21 = commonDataPool.signalDisplay[21];
            xmlConfig.signalDisplay22 = commonDataPool.signalDisplay[22];
            xmlConfig.signalDisplay23 = commonDataPool.signalDisplay[23];
            xmlConfig.signalDisplay24 = commonDataPool.signalDisplay[24];
            xmlConfig.speed = commonDataPool.speed;
            xmlConfig.sensitivity = commonDataPool.sensitivity;
            xmlConfig.secondsPerPage = commonDataPool.secondsPerPage;
            xmlConfig.pixelPerCM = commonDataPool.pixelPerCM;

            if (configManage.SaveToFile("config", xmlConfig) == false) 
            { 
               // System.Windows.Forms.MessageBox.Show("写XML配置文件失败！");
            }
               
        }
    }
}
