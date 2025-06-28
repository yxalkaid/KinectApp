using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.Impinj;
using System;
using System.Collections.Generic;
using System.IO;

namespace KinectApp
{
    /// <summary>
    /// RFID 数据采集器
    /// </summary>
    public class RFIDCapturer : IDisposable
    {
        /// <summary>
        /// RFID 数据帧到达事件
        /// </summary>
        public event Action<List<SimpleTagData>> FrameArrived;

        /// <summary>
        /// RFID 数据捕获
        /// </summary>
        private static LLRPClient reader;

        private uint messageID = 23;
        private PARAM_ROSpec rospec;
        private UInt32 modelNumber = 0;

        private bool IsClosed = false;

        /// <summary>
        /// 设备主机地址
        /// </summary>
        private string host;

        private string configPath;
        private string roSpecPath;


        /// <summary>
        /// 已记录数
        /// </summary>
        private int recordCount;

        public RFIDCapturer(string host, string configPath, string roSpecPath)
        {
            this.host = host;
            this.configPath = configPath;
            this.roSpecPath = roSpecPath;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            Connect(this.host);
            DisableAllROSpecs();
            EnableImpinjExtensions();
            FactoryDefault();
            GetReaderCapabilities();
            GetReaderConfiguration();

            SetReaderConfiguration(this.configPath);
            AddRoSpec(this.roSpecPath);
            Enable();
        }

        /// <summary>
        /// 启动采集
        /// </summary>
        public void Start()
        {
            var start = new MSG_START_ROSPEC
            {
                MSG_ID = GetUniqueMessageID(),
                ROSpecID = rospec.ROSpecID
            };

            try
            {
                var response = reader.START_ROSPEC(start, out _, 12000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("START_ROSPEC successful");
                }
                else
                {
                    Console.WriteLine("START_ROSPEC failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting ROSpec: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public void Stop()
        {
            var stop = new MSG_STOP_ROSPEC
            {
                MSG_ID = GetUniqueMessageID(),
                ROSpecID = rospec.ROSpecID
            };

            try
            {
                var response = reader.STOP_ROSPEC(stop, out _, 12000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("STOP_ROSPEC successful");
                }
                else
                {
                    Console.WriteLine("STOP_ROSPEC failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping ROSpec: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (IsClosed) return;

            DisableAllROSpecs();

            Disconnect();
        }



        #region Private Methods

        /// <summary>
        /// 连接读写器
        /// </summary>
        /// <param name="host"></param>
        private void Connect(string host)
        {
            Console.WriteLine("Connecting to " + host);
            reader = new LLRPClient();
            Impinj_Installer.Install(); // 安装扩展

            Console.WriteLine("Adding Event Handlers");
            reader.OnReaderEventNotification += new delegateReaderEventNotification(OnReaderEventNotification);
            reader.OnRoAccessReportReceived += new delegateRoAccessReport(OnRoAccessReportReceived);

            ENUM_ConnectionAttemptStatusType status;
            bool success = reader.Open(host, 5000, out status);

            if (!success || status != ENUM_ConnectionAttemptStatusType.Success)
            {
                Console.WriteLine("Failed to connect to reader.");
                Environment.Exit(1);
            }
            IsClosed = false;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        private void Disconnect()
        {
            if (IsClosed) return;

            try
            {
                reader.Close();
                reader.OnReaderEventNotification -= new delegateReaderEventNotification(OnReaderEventNotification);
                reader.OnRoAccessReportReceived -= new delegateRoAccessReport(OnRoAccessReportReceived);

                IsClosed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error closing connection: " + ex.Message);
            }
        }

        /// <summary>
        /// 停止所有ROSpec
        /// </summary>
        private void DisableAllROSpecs()
        {
            var disable = new MSG_DISABLE_ROSPEC
            {
                MSG_ID = GetUniqueMessageID(),
                ROSpecID = 0 // All ROSpecs
            };

            try
            {
                var response = reader.DISABLE_ROSPEC(disable, out _, 10000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("DISABLE_ROSPEC successful");
                }
                else
                {
                    Console.WriteLine("Failed to disable all ROSpecs");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error disabling ROSpecs: " + ex.Message);
            }
        }

        /// <summary>
        /// 启动扩展功能
        /// </summary>
        private void EnableImpinjExtensions()
        {
            var enableExt = new MSG_IMPINJ_ENABLE_EXTENSIONS { MSG_ID = GetUniqueMessageID() };
            try
            {
                var response = reader.CUSTOM_MESSAGE(enableExt, out var error, 8000);
                var extResponse = response as MSG_IMPINJ_ENABLE_EXTENSIONS_RESPONSE;

                if (extResponse?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("IMPINJ_ENABLE_EXTENSIONS successful");
                }
                else
                {
                    Console.WriteLine("Failed to enable extensions: " + error?.ToString());
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Timeout waiting for IMPINJ_ENABLE_EXTENSIONS response: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 恢复出厂设置
        /// </summary>
        private void FactoryDefault()
        {
            var setConfig = new MSG_SET_READER_CONFIG
            {
                MSG_ID = GetUniqueMessageID(),
                ResetToFactoryDefault = true
            };

            try
            {
                var response = reader.SET_READER_CONFIG(setConfig, out _, 12000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("Factory default applied successfully");
                }
                else
                {
                    Console.WriteLine("Failed to apply factory default");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying factory default: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 获取读写器能力信息
        /// </summary>
        private void GetReaderCapabilities()
        {
            var getCap = new MSG_GET_READER_CAPABILITIES
            {
                MSG_ID = GetUniqueMessageID(),
                RequestedData = ENUM_GetReaderCapabilitiesRequestedData.All
            };

            try
            {
                var response = reader.GET_READER_CAPABILITIES(getCap, out _, 8000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    var devCap = response.GeneralDeviceCapabilities;
                    if (devCap != null && devCap.DeviceManufacturerName == 25882)
                    {
                        modelNumber = devCap.ModelName;
                        Console.WriteLine($"Found Impinj model {modelNumber}");
                    }
                    else
                    {
                        Console.WriteLine("Not an Impinj device");
                        Environment.Exit(1);
                    }
                }
                else
                {
                    Console.WriteLine("GET_READER_CAPABILITIES failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting capabilities: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 获取读写器配置信息
        /// </summary>
        private void GetReaderConfiguration()
        {
            var getConfig = new MSG_GET_READER_CONFIG
            {
                MSG_ID = GetUniqueMessageID(),
                RequestedData = ENUM_GetReaderConfigRequestedData.All,
                AntennaID = 0,
                GPIPortNum = 0,
                GPOPortNum = 0
            };

            try
            {
                var response = reader.GET_READER_CONFIG(getConfig, out _, 10000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("GET_READER_CONFIG successful");
                    // 可选地解析天线配置等
                }
                else
                {
                    Console.WriteLine("GET_READER_CONFIG failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting configuration: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置Reader配置
        /// </summary>
        /// <param name="path"></param>
        private void SetReaderConfiguration(string path)
        {
            try
            {
                Org.LLRP.LTK.LLRPV1.DataType.Message msgObj;
                ENUM_LLRP_MSG_TYPE msgType;


                StreamReader sr = new StreamReader(
                    new FileStream(path, FileMode.Open)
                );
                string s = sr.ReadToEnd();
                sr.Close();

                LLRPXmlParser.ParseXMLToLLRPMessage(s, out msgObj, out msgType);

                if (msgObj == null || msgType != ENUM_LLRP_MSG_TYPE.SET_READER_CONFIG)
                {
                    Console.WriteLine("Invalid SET_READER_CONFIG XML");
                    Environment.Exit(1);
                }

                var setConfigMsg = (MSG_SET_READER_CONFIG)msgObj;
                var response = reader.SET_READER_CONFIG(setConfigMsg, out _, 12000);

                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("SET_READER_CONFIG successful");
                }
                else
                {
                    Console.WriteLine("SET_READER_CONFIG failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting configuration: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 添加ROSpec
        /// </summary>
        /// <param name="path"></param>
        private void AddRoSpec(string path)
        {
            try
            {
                Org.LLRP.LTK.LLRPV1.DataType.Message msgObj;
                ENUM_LLRP_MSG_TYPE msgType;


                StreamReader sr = new StreamReader(
                    new FileStream(path, FileMode.Open)
                );
                string s = sr.ReadToEnd();
                sr.Close();

                LLRPXmlParser.ParseXMLToLLRPMessage(s, out msgObj, out msgType);

                if (msgObj == null || msgType != ENUM_LLRP_MSG_TYPE.ADD_ROSPEC)
                {
                    Console.WriteLine("Invalid ADD_ROSPEC XML");
                    Environment.Exit(1);
                }

                var addRoSpec = (MSG_ADD_ROSPEC)msgObj;
                addRoSpec.MSG_ID = GetUniqueMessageID();
                rospec = addRoSpec.ROSpec;

                var response = reader.ADD_ROSPEC(addRoSpec, out _, 12000);

                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("ADD_ROSPEC successful");
                }
                else
                {
                    Console.WriteLine("ADD_ROSPEC failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding ROSpec: " + ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 启用ROSpec
        /// </summary>
        private void Enable()
        {
            var enable = new MSG_ENABLE_ROSPEC
            {
                MSG_ID = GetUniqueMessageID(),
                ROSpecID = rospec.ROSpecID
            };

            try
            {
                var response = reader.ENABLE_ROSPEC(enable, out _, 12000);
                if (response?.LLRPStatus?.StatusCode == ENUM_StatusCode.M_Success)
                {
                    Console.WriteLine("ENABLE_ROSPEC successful");
                }
                else
                {
                    Console.WriteLine("ENABLE_ROSPEC failed");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enabling ROSpec: " + ex.Message);
                Environment.Exit(1);
            }
        }


        /// <summary>
        /// 获取一个唯一消息ID
        /// </summary>
        /// <returns></returns>
        private uint GetUniqueMessageID()
        {
            return messageID++;
        }

        #endregion


        public void OnRoAccessReportReceived(MSG_RO_ACCESS_REPORT msg)
        {
            if(msg.TagReportData == null)
            {
                return;
            }

            List<SimpleTagData> dataList = new List<SimpleTagData>();
            foreach (PARAM_TagReportData tag in msg.TagReportData)
            {
                recordCount++;
                SimpleTagData data = new SimpleTagData();

                // 获取EPC
                if (tag.EPCParameter[0].GetType() == typeof(PARAM_EPC_96))
                {
                     data.EPC = ((PARAM_EPC_96)(tag.EPCParameter[0])).EPC.ToHexString();
                }
                else
                {
                    data.EPC = ((PARAM_EPCData)(tag.EPCParameter[0])).EPC.ToHexString();
                }

                // 获取AntennaID
                if (tag.AntennaID != null)
                {
                    data.Antenna = tag.AntennaID.AntennaID.ToString();
                }

                // 获取ChannelIndex
                if (tag.ChannelIndex != null)
                {
                    data.Channel = tag.ChannelIndex.ChannelIndex.ToString();
                }

                if (tag.FirstSeenTimestampUTC != null)
                {
                    data.Time = tag.FirstSeenTimestampUTC.Microseconds.ToString();
                }

                if (tag.Custom != null)
                {
                    for (int x = 0; x < tag.Custom.Length; x++)
                    {
                        if (tag.Custom[x].GetType() == typeof(PARAM_ImpinjRFPhaseAngle))
                        {
                            PARAM_ImpinjRFPhaseAngle rfPhase = (PARAM_ImpinjRFPhaseAngle)tag.Custom[x];
                            data.Phase = rfPhase.PhaseAngle.ToString();
                        }
                        else if (tag.Custom[x].GetType() == typeof(PARAM_ImpinjPeakRSSI))
                        {
                            PARAM_ImpinjPeakRSSI rfRssi = (PARAM_ImpinjPeakRSSI)tag.Custom[x];
                            data.RSSI = rfRssi.RSSI.ToString();
                        }
                    }
                }

                dataList.Add(data);
            }

            FrameArrived?.Invoke(dataList);
        }

        public void OnReaderEventNotification(MSG_READER_EVENT_NOTIFICATION msg)
        {
            if (msg.ReaderEventNotificationData == null) return;

            var data = msg.ReaderEventNotificationData;
            if (data.AISpecEvent != null) Console.WriteLine(data.AISpecEvent.ToString());
            if (data.AntennaEvent != null) Console.WriteLine(data.AntennaEvent.ToString());
            if (data.ConnectionAttemptEvent != null) Console.WriteLine(data.ConnectionAttemptEvent.ToString());
            if (data.ConnectionCloseEvent != null) Console.WriteLine(data.ConnectionCloseEvent.ToString());
            if (data.GPIEvent != null) Console.WriteLine(data.GPIEvent.ToString());
            if (data.HoppingEvent != null) Console.WriteLine(data.HoppingEvent.ToString());
            if (data.ReaderExceptionEvent != null) Console.WriteLine(data.ReaderExceptionEvent.ToString());
            if (data.ReportBufferLevelWarningEvent != null) Console.WriteLine(data.ReportBufferLevelWarningEvent.ToString());
            if (data.ReportBufferOverflowErrorEvent != null) Console.WriteLine(data.ReportBufferOverflowErrorEvent.ToString());
            if (data.ROSpecEvent != null) Console.WriteLine(data.ROSpecEvent.ToString());
        }
    }
}