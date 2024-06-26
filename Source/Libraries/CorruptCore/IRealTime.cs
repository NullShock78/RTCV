namespace RTCV.CorruptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public interface IRealTime
    {
        event EventHandler<RealTimeEventArgs> StepHandler;
        event EventHandler GameLoaded;
        event EventHandler GameClosed;

        public bool SupportsRewind { get; set; }
        public bool SupportsForwarding { get; set; }
        public bool SupportsFastForwarding { get; set; }
        public object GetDisplayForm { get; }

        public bool OverrideBackgroundInput { get; set; }
    }

    public class RealTimeEventArgs : EventArgs
    {
        public bool isForwarding { get; set; } = false;
        public bool isRewinding { get; set; } = false;
        public bool isFastForwarding { get; set; } = false;
    }
}
