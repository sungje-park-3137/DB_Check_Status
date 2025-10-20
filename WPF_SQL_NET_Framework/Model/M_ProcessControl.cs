using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using WPF_SQL_NET_Framework.Info;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WPF_SQL_NET_Framework.Model
{
    public class M_ProcessControl
    {
        #region [Method]
        public List<Process_Data> GetProcessList()
        {
            List<Process_Data> tempProcessList = new List<Process_Data>();
            try
            {
                string wmiQuery = "Select * From Win32_Process";
                ManagementObjectSearcher search = new ManagementObjectSearcher(wmiQuery);
                ManagementObjectCollection processList = search.Get();

                foreach (ManagementObject process in processList)
                {
                    string path = process["ExecutablePath"]?.ToString();
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }
                    else if (path.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    Process_Data _PData = new Process_Data
                    {
                        PID = int.Parse(process["ProcessID"]?.ToString()),
                        PName = process["Name"]?.ToString(),
                        PPath = process["ExecutablePath"]?.ToString()
                    };

                    tempProcessList.Add(_PData);
                }

                return tempProcessList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return tempProcessList;
            }
        }

        public bool CheckLimit(ObservableCollection<Process_Data> processList, string limitcpu, string limitmemory)
        {
            try
            {
                processList.ToList().ForEach(process =>
                {
                    if (process.IsCheck == true)
                    {
                        if (!string.IsNullOrEmpty(limitcpu) && Convert.ToInt64(limitcpu) < process.LastPCpuTime)
                        {
                            throw new Exception($"프로그램: {process.PName},{process.PID}\n 제한된 CPU 사용량: {limitcpu} < 프로그램 CPU 사용량: {process.LastPCpuTime}");
                        }
                        if (!string.IsNullOrEmpty(limitmemory) && Convert.ToInt64(limitmemory) < process.LastPMemory)
                        {
                            throw new Exception($"프로그램: {process.PName},{process.PID}\n 제한된 Memory 사용량: {limitmemory} < 프로그램 Memory 사용량: {process.LastPMemory}");
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
            

        public void GetPorcessCpuTime(ObservableCollection<Process_Data> processList)
        {
            List<(int, TimeSpan, DateTime)> cpucounter = new List<(int, TimeSpan, DateTime)>();

            processList.ToList().ForEach(process =>
            {
                if (process.IsCheck == true)
                {
                    // wmi 방식 : 걸리는 시간 3초 이상 걸리는듯 -> performanceCounter 사용으로 변경
                    //string query = $"SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfProc_Process WHERE Name='{Path.GetFileNameWithoutExtension(process.PName)}'";
                    //using (ManagementObjectSearcher search = new ManagementObjectSearcher(query))
                    //{
                    //    ManagementObjectCollection mobj = search.Get();
                    //    foreach (var temp in mobj)
                    //    {
                    //        //Console.WriteLine(Convert.ToDecimal(temp.Properties["PercentProcessorTime"].Value));
                    //        process.PCpuTime = Convert.ToDecimal(temp.Properties["PercentProcessorTime"].Value);
                    //    }
                    //}

                    // processCounter 방식 : 내가 설정한 시간동안(ex, 1초) CPU의 사용량을 확인한다. -> 프로세스 이름만 가지고 있으면 중복된 .exe일 경우 문제 발생 -> totalprocessortime으로 변경
                    //PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", $"{Path.GetFileNameWithoutExtension(process.PName)}", true);
                    //process.PCpuTime = cpuCounter.NextValue(); // 내부에서 한번 초기화 하는 작업이라함, 그리고 해당 작업을 0으로 거치고 계산해야함
                    //cpucounter.Add((process.PName, cpuCounter));
                    ////process.PCpuTime = cpuCounter.NextValue() / Environment.ProcessorCount;
                    ///

                    ////////

                    // cpu 사용량
                    var temppid = Process.GetProcessById(process.PID);
                    TimeSpan startcpu = temppid.TotalProcessorTime;
                    DateTime starttime = DateTime.UtcNow;

                    cpucounter.Add((process.PID, startcpu, starttime));
                }
            });

            Thread.Sleep(5000);

            processList.ToList().ForEach(process =>
            {
                if (process.IsCheck == true)
                {
                    var temp = cpucounter.FindAll(x => x.Item1 == process.PID);

                    if (temp.Count() > 0)
                    {
                        temp.ForEach(x =>
                        {
                            var temproc = Process.GetProcessById(x.Item1);
                            temproc.Refresh();

                            TimeSpan endcpu = temproc.TotalProcessorTime;
                            DateTime endtime = DateTime.UtcNow;

                            var use = (endcpu - x.Item2).TotalMilliseconds / (endtime - x.Item3).TotalMilliseconds / Environment.ProcessorCount * 100;
                            //
                            //var test = x.Item2.NextValue() / Environment.ProcessorCount;
                            //Console.WriteLine(use);
                            process.LastPCpuTime = use;

                            // 메모리 사용량
                            var temppid = Process.GetProcessById(process.PID);
                            var workmemory = temppid.WorkingSet64;
                            //var processmemory = temppid.PrivateMemorySize64;
                            //var virtualmemory = temppid.VirtualMemorySize64;

                            process.LastPMemory = workmemory;

                            process.PResource.Add(new PResource_Data
                            {
                                PCpuTime = use,
                                PMemory = workmemory,
                                PDateTime = DateTime.Now
                            });
                        });
                    }
                }
            });
        }

        public void Clear_ProcessList(ObservableCollection<Process_Data> processList)
        {
            processList.ToList().ForEach(pr =>
            {
                pr.LastPCpuTime = 0;
                pr.LastPMemory = 0;

                pr.PResource.Clear();
            });
        }

        

        #endregion
    }
}
