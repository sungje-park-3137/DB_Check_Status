using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Parsing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Management.Automation.Language;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.SingleTon
{
    public class csLogger
    {
        private static csLogger Instance;
        public static csLogger GetInstance()
        {
            if (Instance == null)
            {
                Instance = new csLogger();
            }
            return Instance;
        }

        public readonly Microsoft.Extensions.Logging.ILogger<csLogger> _logger;
        public ObservableCollection<Logger_Data> logEvent = new ObservableCollection<Logger_Data>();

        public csLogger()
        {
            //(appsetting.json: 파일로 설정 저장 방법)

            //logEvent = new ObservableCollection<LogEvent>(); // serilog observable 저장 및 datagrid item 

            // setting 파일(json) load
            var config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsetting.json")
                .Build();

            // serilog의 설정 값 일치화
            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(config) // get section 사용 x 그냥 파일 전체 입력 o
            //    .WriteTo.Observers(_events => // nuget: serilog observable 설치 -> observablecolltion으로 반환 받기 위함
            //    {
            //        _events.Subscribe(e => // nuget: reactive 설치 -> 람다, IDispoable IOServable으로 반환 받기 위함
            //        {
            //            //lock (_logLock)
            //            //{
            //            //    logEvent.Add(e);
            //            //}
            //            logEvent.Add(e);
            //        });
            //    }) 
            //    .CreateLogger();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

            // ms logging에 serilog 연결 -> 방법:loggerfactory
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
            });

            // logging에 <type>을 사용해서 새 ILogger 인스턴스를 생성 -> createlogger: logging에 new 역할인듯? 
            _logger = loggerFactory.CreateLogger<csLogger>();


            //(직접 설정 저장 방법)
            /*// serilog : logger 내부 변수 설정 부분 (직접 설정 저장 방법
            Log.Logger = new LoggerConfiguration() // serilog 새롭게 선언
                .MinimumLevel.Debug() // 최소레벨 -> 어떤 최소레벨?
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // 파일 경로, 년월일 단위 설정
                .CreateLogger(); // 설정 값대로 생성

            // ms logging : ms logging의 logger 사용 시 serilog로 전달 
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
            });

            logger = loggerFactory.CreateLogger<MainWindow>(); // 카테고리 이름, 로그에 태그가 붙는다*/
        }

        // serilog의 interface 직접 활용
        //Log.Logger = new LoggerConfiguration().WriteTo.File("text.txt").CreateLogger();
        //Log.Logger.Information("정보: 데이터 1");

        public void CreateLog(LogLevel loglevel, string msg)//Microsoft.Extensions.Logging.LogLevel loglevel, string msg
        {
            try
            {
                Logger_Data tempLD = new Logger_Data
                {
                    LOG_DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    LOG_Level = loglevel,
                    LOG_Msg = msg
                };

                ConcurrentQueue<Logger_Data> conqueue = new ConcurrentQueue<Logger_Data>();
                conqueue.Enqueue(tempLD);

                if (conqueue.Count > 0)
                {
                    if (conqueue.TryDequeue(out Logger_Data result))
                    {
                        //var serilogLevel = Serilog.Events.LogEventLevel.Debug;

                        switch (result.LOG_Level)
                        {
                            case LogLevel.Debug:
                                _logger.LogDebug(msg);
                                //serilogLevel = LogEventLevel.Debug;
                                
                                break;

                            case LogLevel.Information:
                                _logger.LogInformation(msg);
                                //serilogLevel = LogEventLevel.Information;

                                break;

                            case LogLevel.Warning:
                                _logger.LogWarning(msg);
                                //serilogLevel = LogEventLevel.Warning;

                                break;

                            case LogLevel.Error:
                                _logger.LogError(msg);
                                //serilogLevel = LogEventLevel.Error;

                                break;

                            case LogLevel.Critical:
                                _logger.LogCritical(msg);
                                //serilogLevel = LogEventLevel.Fatal;

                                break;
                        }

                        //serilog logevent
                        //var manualLog = new LogEvent(
                        //        DateTimeOffset.Now,
                        //        serilogLevel,
                        //        exception: null,
                        //        messageTemplate: new MessageTemplate(msg, new List<MessageTemplateToken>()),
                        //        properties: new List<LogEventProperty>()
                        //        );

                        logEvent.Add(result);

                        if (logEvent.Count() > 500)
                        {
                            logEvent.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
