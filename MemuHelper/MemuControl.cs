using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemuHelper
{
    public class MemuControl
    {
        public int index;
        public static string memuFolderPath;
        public static string adbFolderPath;

        public MemuControl(int index)
        {
            this.index = index;
        }


        #region Main Excute

        public static string CMDExecute(string command, string path, int timeout = -1)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + command.Trim();
            process.StartInfo.WorkingDirectory = path;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            StringBuilder output = new StringBuilder();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.Append(e.Data + "\n");
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            if (timeout < 0)
            {
                process.WaitForExit();
            }
            else
            {
                process.WaitForExit(timeout * 1000);
            }

            try { process.Kill(); } catch { }
            return output.ToString();
        }

        public static string ADBExecute(string adbCmd, int index = -1, int timeout = -1)
        {
            if (index < 0)
            {
                Process process = new Process();
                process.StartInfo.FileName = string.IsNullOrEmpty(adbFolderPath) ? Path.Combine(memuFolderPath, "adb.exe") : Path.Combine(adbFolderPath, "adb.exe");
                process.StartInfo.Arguments = adbCmd.Trim();
                process.StartInfo.WorkingDirectory = string.IsNullOrEmpty(adbFolderPath) ? memuFolderPath : adbFolderPath;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                StringBuilder output = new StringBuilder();

                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.Append(e.Data + "\n");
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                if (timeout < 0)
                {
                    process.WaitForExit();
                }
                else
                {
                    process.WaitForExit(timeout * 1000);
                }

                try { process.Kill(); } catch { }
                return output.ToString();
            }
            return MemucExecute($"-i {index} adb \"{adbCmd}\"", timeout);
        }


        private static int countUsingMemuConsole = -1;
        private static object _lockCleanMemuConsole = new object();
        public static string MemucExecute(string command, int timeout = -1)
        {
            Process process = new Process();
            process.StartInfo.FileName = memuFolderPath + "\\memuc.exe";
            process.StartInfo.Arguments = command.Trim();
            process.StartInfo.WorkingDirectory = memuFolderPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            StringBuilder output = new StringBuilder();

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.Append(e.Data + "\n");
                }
            };
            process.Start();
            process.BeginOutputReadLine();

            if (timeout < 0)
            {
                process.WaitForExit();
            }
            else
            {
                process.WaitForExit(timeout * 1000);
            }

            lock (_lockCleanMemuConsole)
            {
                if (countUsingMemuConsole > 30 || countUsingMemuConsole == -1)
                {
                    countUsingMemuConsole = 0;
                    CloseAllMemuConsole();
                }
            }
            countUsingMemuConsole++;

            try { process.Kill(); } catch { }
            return output.ToString();
        }


        /// <summary>
        /// Kill all MemuConsole programs running > 60s
        /// </summary>
        public static void CloseAllMemuConsole()
        {
            foreach (var item in Process.GetProcessesByName("memuconsole"))
            {
                try
                {
                    if (item.StartTime.AddSeconds(60) < DateTime.Now)
                    {
                        item.Kill();
                    }
                }
                catch { }
            }
        }


        #endregion


        #region MemuConsole Static Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeVersion">44, 51, 71, 76</param>
        /// <returns></returns>

        private static async Task<string> CreateVM(int codeVersion)
        {
            return await Task.FromResult(MemucExecute(("create" + codeVersion).Trim()));
        }

        private static async Task<string> DeleteVM(int index)
        {
            return await Task.FromResult(MemucExecute($"remove -i {index}"));
        }

        private static async Task<string> CloneVM(int index)
        {
            await new MemuControl(index).StopVM(true);
            return await Task.FromResult(MemucExecute($"clone -i {index}"));
        }

        private static async Task<string> Export(int index, string filePath)
        {
            return await Task.FromResult(MemucExecute($"memuc export -i {index} {filePath}.ova"));
        }

        public static async Task<string> Import(string filePath)
        {
            return await Task.FromResult(MemucExecute($"memuc import -i {filePath}"));
        }

        private static async Task<string> OpenVM(int index, bool WaitBootCompled)
        {
            if (WaitBootCompled)
            {
                return await Task.FromResult(MemucExecute($"start -i {index}"));
            }
            return await Task.FromResult(MemucExecute($"start -i {index} -t"));
        }

        private static async Task<string> StopVM(int index, bool WaitStopCompled)
        {
            if (WaitStopCompled)
            {
                return await Task.FromResult(MemucExecute($"stop -i {index}"));
            }
            return await Task.FromResult(MemucExecute($"stop -i {index} -t"));
        }

        public static async Task<string> StopAllVMs()
        {
            return await Task.FromResult(MemucExecute("stopall"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="showOnlyRunningVMs"></param>
        /// <param name="isShowDiskInfo"></param>
        /// <returns>List the simulator index, title, top-level window handle, whether to start the simulator, process PID information, simulator disk usage</returns>
        public static async Task<List<InfoVMs>> InfoVMs()
        {
            List<InfoVMs> result = new List<InfoVMs>();
            string[] listInfo = MemucExecute("listvms -s").Replace("\r", "").Split('\n');
            foreach (string info in listInfo)
            {
                if (string.IsNullOrEmpty(info))
                {
                    continue;
                }
                string[] arrInfo = info.Split(',');
                InfoVMs objInfo = new InfoVMs();

                objInfo.Index = int.Parse(arrInfo[0]);
                objInfo.Title = arrInfo[1];
                objInfo.TopWinHwnd = new IntPtr(int.Parse(arrInfo[2]));
                objInfo.IsRunning = arrInfo[3].Contains("1");
                objInfo.Pid = long.Parse(arrInfo[4]);
                objInfo.DiskUsage = long.Parse(arrInfo[5]);

                result.Add(objInfo);
            }
            return await Task.FromResult(result);
        }

        private static async Task<bool> isVMRunning(int index)
        {
            return await Task.FromResult(!MemucExecute($"isvmrunning -i {index}").Contains("Not Running"));
        }

        public async static Task<string> SortVMs()
        {
            return await Task.FromResult(MemucExecute($"sortwin"));
        }

        private static async Task<string> RebootVM(int index)
        {
            return await Task.FromResult(MemucExecute($"reboot -i {index}"));
        }

        private static async Task<string> RenameVM(int index, string title)
        {
            return await Task.FromResult(MemucExecute($"rename -i {index} {title}"));
        }

        public static async Task<string> TaskStatus(string taskId)
        {
            return await Task.FromResult(MemucExecute($"taskstatus {taskId}"));
        }

        private static async Task<string> GetConfig(int index, string key)
        {
            return await Task.FromResult(MemucExecute($"getconfigex -i {index} {key}"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="configStr">cpus 4 memory 1024</param>
        /// <returns></returns>
        private static async Task<string> SetConfig(int index, string configStr)
        {
            return await Task.FromResult(MemucExecute($"setconfigex -i {index} {configStr}"));
        }

        private static async Task<string> InstallApk(int index, string apkFilePath)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"installapp -i {index} {apkFilePath}"));
        }

        private static async Task<string> UninstallApk(int index, string packageName)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"uninstallapp -i {index} {packageName}"));
        }

        private static async Task<string> StartApp(int index, string packageActivity)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"installapp -i {index} {packageActivity}"));
        }

        private static async Task<string> StopApp(int index, string packageName)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"stopapp -i {index} {packageName}"));
        }

        private static async Task<string> SendKeyStroke(int index, string keyName)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"sendkey -i {index} {keyName}"));
        }

        private static async Task<string> OnShake(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"shake -i {index}"));
        }

        private static async Task<string> ConnectInternet(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"connect -i {index}"));
        }

        private static async Task<string> DisconnectInternet(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"disconnect -i {index}"));
        }

        private static async Task<string> InputText(int index, string text)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"input -i {index} \"{text}\""));
        }

        private static async Task<string> Rotate(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"rotate -i {index}"));
        }

        private static async Task<string> ExecuteCmdInAndroid(int index, string guestCmd)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"-i {index} execcmd \"{guestCmd}\""));
        }

        private static async Task<string> ChangeGPS(int index, double longitude, double latitude)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"setgps -i {index} {longitude} {latitude}"));
        }

        private static async Task<string> GetIPAdress(int index)
        {
            return await ExecuteCmdInAndroid(index, "wget -O- whatismyip.akamai.com");
        }

        private static async Task<string> ZoomIn(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"zoomin -i {index}"));
        }

        private static async Task<string> ZoomOut(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"zoomout -i {index}"));
        }

        private static async Task<string> GetAppInfoList(int index)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"getappinfolist -i {index}"));
        }

        private static async Task<string> SetAccelerometer(int index, double x, double y, double z)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"accelerometer -i {index} -x {x} -y {y} -z {z}"));
        }

        private static async Task<string> CreateShortcutToDesktop(int index, string packageName)
        {
            return await Task.FromResult(MemuControl.MemucExecute($"createshortcut -i {index} {packageName}"));
        }

        #endregion


        #region MemuConsole Object Methods

        public async Task<string> CreateVM()
        {
            return await MemuControl.CreateVM(index);
        }

        public async Task<string> DeleteVM()
        {
            return await MemuControl.DeleteVM(index);
        }

        public async Task<string> CloneVM()
        {
            return await MemuControl.CloneVM(index);
        }

        public async Task<string> Export(string filePath)
        {
            return await MemuControl.Export(index, filePath);
        }

        public async Task<string> OpenVM(bool isWaitBootCompled)
        {
            return await MemuControl.OpenVM(index, isWaitBootCompled);
        }

        public async Task<string> StopVM(bool isWaitBootCompled)
        {
            return await MemuControl.StopVM(index, isWaitBootCompled);
        }

        public async Task<bool> isVMRunning()
        {
            return await MemuControl.isVMRunning(index);
        }

        public async Task<string> RebootVM()
        {
            return await MemuControl.RebootVM(index);
        }

        public async Task<string> RenameVM(string title)
        {
            return await MemuControl.RenameVM(index, title);
        }

        public async Task<string> GetConfig(string key)
        {
            return await MemuControl.GetConfig(index, key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConfig">cpus 4 memory 1024</param>
        /// <returns></returns>
        public async void SetConfigs(int milisecondDelay = -1, params string[] strConfigs)
        {
            foreach (var config in strConfigs)
            {
                if (milisecondDelay > 0)
                {
                    await Task.Delay(milisecondDelay);
                }
                await MemuControl.SetConfig(index, config);
            }
        }

        public async Task<string> SetConfig(string strConfig)
        {
            return await MemuControl.SetConfig(index, strConfig);
        }

        public async Task<string> InstallApk(string apkFilePath)
        {
            return await MemuControl.InstallApk(index, apkFilePath);
        }

        public async Task<string> UninstallApk(string packageName)
        {
            return await MemuControl.UninstallApk(index, packageName);
        }

        public async Task<string> StartApp(string packageActivity)
        {
            return await MemuControl.StartApp(index, packageActivity);
        }

        public async Task<string> StopApp(string packageName)
        {
            return await MemuControl.StopApp(index, packageName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">home, back</param>
        /// <returns></returns>
        public async Task<string> SendKeyStroke(string keyName)
        {
            return await MemuControl.SendKeyStroke(index, keyName);
        }

        public async Task<string> OnShake()
        {
            return await MemuControl.OnShake(index);
        }

        public async Task<string> ConnectInternet()
        {
            return await MemuControl.ConnectInternet(index);
        }

        public async Task<string> DisconnectInternet()
        {
            return await MemuControl.DisconnectInternet(index);
        }

        public async Task<string> InputText(string text)
        {
            return await MemuControl.InputText(index, text);
        }

        public async Task<string> Rotate()
        {
            return await MemuControl.Rotate(index);
        }

        public async Task<string> ExecuteCmdInAndroid(string guestCmd)
        {
            return await MemuControl.ExecuteCmdInAndroid(index, guestCmd);
        }

        public async Task<string> ChangeGPS(double longitude, double latitude)
        {
            return await MemuControl.ChangeGPS(index, longitude, latitude);
        }

        public async Task<string> GetIPAdress()
        {
            return await GetIPAdress(index);
        }

        public async Task<string> ZoomIn()
        {
            return await MemuControl.ZoomIn(index);
        }

        public async Task<string> ZoomOut()
        {
            return await MemuControl.ZoomOut(index);
        }

        public async Task<string> GetAppInfoList()
        {
            return await MemuControl.GetAppInfoList(index);
        }

        public async Task<string> SetAccelerometer(double x, double y, double z)
        {
            return await MemuControl.SetAccelerometer(index, x, y, z);
        }

        public async Task<string> CreateShortcutToDesktop(string packageName)
        {
            return await MemuControl.CreateShortcutToDesktop(index, packageName);
        }

        public async Task<string> ADBExecute(string adbCmd)
        {
            return await Task.FromResult(MemuControl.ADBExecute(adbCmd, index));
        }

        #endregion

    }
}
