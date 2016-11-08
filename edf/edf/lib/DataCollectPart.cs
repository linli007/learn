using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDF;

namespace ElectroencephalographController
{
    /// <summary>
    /// 数据采集模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        #region 变量
        /// <summary>
        /// 数据采集配置Form
        /// </summary>
        FormConfigDataCollect myDataCollectConfigForm;

        /// <summary>
        /// 实时采集Form
        /// </summary>
        FormDataCollect myDataCollectForm;

        public FormDataCollect MyDataCollectForm
        {
            get { return myDataCollectForm; }
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        private void FormDataCollectInit()
        {
            try
            {
                //xmlConfig = configManage.GetFromFile("config");
                //ImportXmlData();
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("读配置文件信息出错！");
                // if (xmlConfig == null)
                //{
                //   xmlConfig = new GSMRConfig();
                //   xmlConfig.SetDefaultConfig();
                //   ImportXmlData();
                //   configManage.SaveToFile("config", xmlConfig);
            }
            
            myDataCollectForm = new FormDataCollect();
            myDataCollectForm.SetFormAsChild();
            myDataCollectForm.ReigisterNeuroControl(this);
            myDataCollectForm.showPlayBackParams();

            myMainForm.AddSubForm(myDataCollectForm);
        }

        /// <summary>
        /// Show实时采集配置Form
        /// </summary>
        private void ShowDataCollectConfigForm()
        {
            if (myDataCollectConfigForm != null)
            {
                myDataCollectConfigForm.ShowForm();
            }
        }

        /// <summary>
        /// Show实时采集Form
        /// </summary>
        private void ShowDataCollectForm()
        {
            if (myDataCollectForm != null)
            {
                myDataCollectForm.ShowForm();
            }
        }
        /// <summary>
        /// 解析EEGData  
        /// </summary>
        /// <param name="edfFile"></param>
        private void DealWithEEGData(EDFFile edfFile)
        {
            myDataCollectForm.DealWithEEGData(edfFile);
        }

        ///<summary>
        ///设置btn_Play为可用
        ///-- by lxl
        ///</summary>
        public void EnableBtn_Play()
        {
            myDataCollectForm.EnableBtn_Play();
        }
    }
}
