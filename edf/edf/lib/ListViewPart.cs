using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroencephalographController
{
    /// <summary>
    /// 脑电数据列表模块  --by wdp
    /// </summary>
    public partial class NeuroControl
    {
        /// <summary>
        /// ListViewForm
        /// </summary>
        public FormListView myListViewForm;

        /// <summary>
        /// 初始化
        /// </summary>
        public void FormListViewInit()
        {
            myListViewForm = new FormListView();
            myListViewForm.SetFormAsChild();
            myListViewForm.ReigisterNeuroControl(this);
            myMainForm.AddSubForm(myListViewForm);
        }

        /// <summary>
        /// ShowForm
        /// </summary>
        private void ShowListViewForm()
        {
            if (myListViewForm != null)
            {
                myListViewForm.ShowForm();
            }
        }
    }
}
