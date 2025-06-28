using Impinj.OctaneSdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KinectApp
{
    /// <summary>
    /// RFID 数据采集器
    /// </summary>
    public class RFIDCapturer :IDisposable
    {
        /// <summary>
        /// RFID 数据帧到达事件
        /// </summary>
        public event Action<List<SimpleTagData>> FrameArrived;

        /// <summary>
        /// RFID 数据采集
        /// </summary>
        private ImpinjReader RFIDReader;

        /// <summary>
        /// 设备主机地址
        /// </summary>
        private string host;

        public RFIDCapturer(string host)
        {
            this.host = host;
            RFIDReader = new ImpinjReader();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (RFIDReader != null)
            {
                this.ConnectToReader();

                RFIDReader.Stop();
                Settings settings = RFIDReader.QueryDefaultSettings();
                settings.AutoStart.Mode = AutoStartMode.None;
                settings.AutoStop.Mode = AutoStopMode.None;

                // Set the reader mode, search mode and session
                settings.ReaderMode = ReaderMode.MaxThroughput;
                settings.SearchMode = SearchMode.DualTarget;
                settings.Session = 2;
                settings.TagPopulationEstimate = 32;

                settings.Antennas.DisableAll();
                settings.Antennas.GetAntenna(1).IsEnabled = true;
                settings.Antennas.GetAntenna(1).MaxTxPower = true;
                settings.Antennas.GetAntenna(1).MaxRxSensitivity = true;

                settings.Report.Mode = ReportMode.Individual;

                settings.Report.IncludeFirstSeenTime = true;
                settings.Report.IncludeChannel= true;
                settings.Report.IncludePhaseAngle = true;
                settings.Report.IncludePeakRssi = true;
                settings.Report.IncludeAntennaPortNumber = true;

                RFIDReader.ApplySettings(settings);
                RFIDReader.SaveSettings();


                RFIDReader.TagsReported += RFIDFrameArrived;
            }
        }

        /// <summary>
        /// RFID数据帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="report"></param>
        public void RFIDFrameArrived(ImpinjReader sender, TagReport report)
        {

            List<SimpleTagData> simpleTags = new List<SimpleTagData>();
            foreach(Tag tag in report)
            {
                SimpleTagData data = new SimpleTagData();

                // 获取时间
                data.Time = tag.FirstSeenTime.LocalDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff");

                // 获取EPC
                data.EPC = tag.Epc.ToHexString();

                // 获取信道
                int channel= 1+((int)((tag.ChannelInMhz-902.75)*2));
                data.Channel = channel.ToString();

                // 获取相位
                int phase = (int)(tag.PhaseAngleInRadians / Math.PI * 2048);
                data.Phase = phase.ToString();

                // 获取RSSI
                data.RSSI = tag.PeakRssiInDbm.ToString();

                // 获取天线号
                data.Antenna = tag.AntennaPortNumber.ToString();

                simpleTags.Add(data);
            }

            FrameArrived?.Invoke(simpleTags);
        }

        private void ConnectToReader()
        {
            try
            {
                Console.WriteLine(
                    "Attempting to connect to {0} ({1}).",
                    RFIDReader.Name, this.host
                );

                RFIDReader.ConnectTimeout = 6000;
                RFIDReader.Connect(this.host);
                RFIDReader.ResumeEventsAndReports();

                Console.WriteLine("Successfully connected.");
            }
            catch (OctaneSdkException e)
            {
                Console.WriteLine("Failed to connect.");
                throw e;
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public void Start()
        {
            RFIDReader.Start();
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public void Stop()
        {
            RFIDReader.Stop();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if ( RFIDReader != null )
            {
                Stop();
                RFIDReader.Disconnect();
                RFIDReader = null;
            }
        }
    }
}
