using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemuHelper
{
    public class InfoVMs
    {
        int index;
        string title;
        IntPtr topWinHwnd;
        bool isRunning;
        long pid;
        long diskUsage;
        string status;

        public int Index { get => index; set => index = value; }
        public string Title { get => title; set => title = value; }
        public IntPtr TopWinHwnd { get => topWinHwnd; set => topWinHwnd = value; }
        public bool IsRunning { get => isRunning; set => isRunning = value; }
        public long Pid { get => pid; set => pid = value; }
        public long DiskUsage { get => diskUsage; set => diskUsage = value; }
        public string Status { get => status; set => status = value; }

    }

}
