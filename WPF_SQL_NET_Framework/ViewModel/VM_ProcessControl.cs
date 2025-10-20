using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.Info;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.View;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public class VM_ProcessControl : NotifyProperty
    {
        #region [Instance]
        private M_ProcessControl _MPControl = new M_ProcessControl();
        private DispatcherTimer _Timer;

        #endregion


        #region [Variable]
        //private ObservableCollection<Process_Data> _Process_List = new ObservableCollection<Process_Data>();
        //public ObservableCollection<Process_Data> Process_List
        //{
        //    get { return _Process_List; }
        //    set { _Process_List = value; OnPropertyChanged(nameof(Process_List)); }
        //}
        public ObservableCollection<Process_Data> Process_List { get; } = new ObservableCollection<Process_Data>();

        #endregion

        #region [Property]
        private bool _BTN_CheckProcess = false;
        public bool BTN_CheckProcess
        {
            get { return _BTN_CheckProcess; }
            set
            {
                if (_BTN_CheckProcess != value)
                {
                    _BTN_CheckProcess = value;
                    OnPropertyChanged(nameof(BTN_CheckProcess));

                    if (BTN_CheckProcess == true)
                    {
                        Tick_Process();
                    }
                    else
                    {
                        Tick_Stop();
                    }
                }
            }
        }

        private string _TB_LimitCPU = string.Empty;
        public string TB_LimitCPU
        {
            get { return _TB_LimitCPU; }
            set { _TB_LimitCPU = value; OnPropertyChanged(TB_LimitCPU); }
        }

        private string _TB_LimitMemory = string.Empty;
        public string TB_LimitMemory
        {
            get { return _TB_LimitMemory; }
            set { _TB_LimitMemory = value; OnPropertyChanged(TB_LimitMemory); }
        }

        private PlotModel _ProcessChart = new PlotModel();
        public PlotModel ProcessChart
        {
            get { return _ProcessChart; }
            set { _ProcessChart = value; OnPropertyChanged(nameof(ProcessChart)); }
        }

        //public PlotModel ProcessChart { get; set; }

        #endregion [Property]


        #region [ButtonCommand]
        // btn command 등록
        public ButtonCommand BTN_Process { get; }
        public ButtonCommand BTN_GetProcess { get; }

        // 동작 등록
        public VM_ProcessControl()
        {
            BTN_Process = new ButtonCommand(Act_BTN_Process);
            BTN_GetProcess = new ButtonCommand(Act_BTN_GetProcess);

            SetPlotmodel();
        }

        // 동작 구현
        private void Act_BTN_Process()
        {
            //_MPControl.GetProcessList();
        }

        private void Act_BTN_GetProcess()
        {
            Process_List.Clear();
            _MPControl.GetProcessList().ForEach(plist => Process_List.Add(plist));
        }


        public void Tick_Process()
        {
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(10);
            _Timer.Tick += Check_Process;

            _Timer.Start();
        }

        private void Check_Process(object sender, EventArgs args)
        {
            if (_MPControl.CheckLimit(Process_List, TB_LimitCPU, TB_LimitMemory))
            {
                Task.Run(() =>
                {
                    _MPControl.GetPorcessCpuTime(Process_List);
                    SetPlotmodel();
                });
            }
            else
            {
                BTN_CheckProcess = false;
            }
        }

        private void Tick_Stop()
        {
            _Timer.Stop();
            _MPControl.Clear_ProcessList(Process_List);
        }

        #endregion [ButtonCommand List]

        public void SetPlotmodel()
        {
            try
            {
                ProcessChart = new PlotModel() { Title = "CPU Times" };

                ProcessChart.Axes.Add(new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "시간",
                    StringFormat = "HH:mm:ss",
                    MajorGridlineStyle = LineStyle.Solid,
                });

                ProcessChart.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "사용량",
                    Minimum = 0,
                    Maximum = 5,
                });


                var selectProcess = Process_List.Where(x => x.IsCheck.Equals(true)).ToList();

                selectProcess.ForEach(y =>
                {
                    var plotmodel_line = new LineSeries
                    {
                        Title = y.PName,
                        Color = OxyColors.Black,
                        StrokeThickness = 2,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 4,
                    };

                    y.PResource.ToList().ForEach(x =>
                    {
                        plotmodel_line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(x.PDateTime), x.PCpuTime));
                    });

                    ProcessChart.Series.Add(plotmodel_line);
                });

                //ProcessChart.InvalidatePlot(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
