using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

namespace HDFSTools
{
    static class Program
    {

        #region API
        //禁止多个进程运行,并当重复运行时激活以前的进程
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int WS_SHOWNORMAL = 1;
        #endregion

        #region Application Enterance
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Get the running instance.
            Process instance = RunningInstance();
            if (instance == null)
            {
                //Show win7 style
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //There is not another instance, show the form
                Application.Run(new frm_Main());
            }
            else
                //There is another instance of this process
                HandleRunningInstance(instance);
        }
        #endregion

        #region Function
        /// <summary>
        /// 获取进程实例是否已启动
        /// </summary>
        /// <returns></returns>
        private static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            //Loop through the running processes in with the same name   
            foreach (Process process in processes)
            {
                //Ignore the current process   
                if (process.Id != current.Id)
                {
                    //Make sure that the process is running from the exe file.   
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        //Return   the   other   process   instance.   
                        return process;
                    }
                }
            }
            //No other instance was found, return null. 
            return null;
        }

        /// <summary>
        /// 已有实例启动时激活实例
        /// </summary>
        /// <param name="instance"></param>
        private static void HandleRunningInstance(Process instance)
        {
            //Make sure the window is not minimized or maximized   
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            //Set the real intance to foreground window
            SetForegroundWindow(instance.MainWindowHandle);
        }
        #endregion

    }
}
