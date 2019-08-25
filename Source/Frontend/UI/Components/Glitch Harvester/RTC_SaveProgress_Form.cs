﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using RTCV.CorruptCore;
using static RTCV.UI.UI_Extensions;
using RTCV.NetCore.StaticTools;
using RTCV.NetCore;
using Newtonsoft.Json;

namespace RTCV.UI
{
    public partial class RTC_SaveProgress_Form : Form, IAutoColorize, ISubForm
    {

        public RTC_SaveProgress_Form()
        {
            InitializeComponent();

            RtcCore.ProgressBarHandler += StockpileProgressBarHandler;
            this.Shown += RTC_SaveProgress_Form_Shown;
        }

        private void RTC_SaveProgress_Form_Shown(object sender, EventArgs e)
        {
            lbCurrentAction.Text = "Waiting";
            pbSave.Value = 0;
        }

        private void StockpileProgressBarHandler(object source, ProgressBarEventArgs e)
		{
			SyncObjectSingleton.FormExecute(() =>
			{
				lbCurrentAction.Text = e.CurrentTask;
				if ((int) e.Progress > 100)
					e.Progress = 100;
				pbSave.Value = (int)e.Progress;
			});
		}

		public bool SubForm_HasLeftButton => false;
		public bool SubForm_HasRightButton => false;
        public string SubForm_LeftButtonText { get; }
		public string SubForm_RightButtonText { get; }
		public void SubForm_LeftButton_Click()
		{
			throw new NotImplementedException();
		}

		public void SubForm_RightButton_Click()
		{
			throw new NotImplementedException();
		}
	}
}
