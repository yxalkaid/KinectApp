using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectApp
{
    /// <summary>
    /// RFID 数据保存器
    /// </summary>
    public class RFIDSaver
    {
        /// <summary>
        /// RFID 数据写入器
        /// </summary>
        private StreamWriter rfidWriter;

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 录制开始事件
        /// </summary>
        public event Action RecordingStarted;

        /// <summary>
        /// 录制停止事件
        /// </summary>
        public event Action RecordingStopped;

        public RFIDSaver(string parentDir)
        {
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            this.FilePath = Path.Combine(parentDir, $"RFID_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            FileStream fileStream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write);
            this.rfidWriter = new StreamWriter(fileStream) { AutoFlush = false };

            if (this.rfidWriter == null)
            {
                throw new IOException("无法打开RFID数据写入器，可能权限不足或路径无效。");
            }

            // 写入CSV文件的表头
            WriteHeader();
        }

        /// <summary>
        /// 写入CSV文件的表头
        /// </summary>
        private void WriteHeader()
        {
            List<string> header = new List<string> { 
                "time", 
                "id",
                "channel",
                "phase",
                "rssi", 
                "antenna"
            };
            rfidWriter.WriteLine(string.Join(",", header));
        }

        /// <summary>
        /// 写入一帧数据到文件
        /// </summary>
        /// <param name="dataList"></param>
        public void WriteFrame(List<SimpleTagData> dataList)
        {
            if (!IsRecording)
                return;

            foreach (SimpleTagData data in dataList)
            {
                rfidWriter.WriteLine(data.ToString());
            }

            // 当缓存的数据量较大时，手动刷新到文件
            if (rfidWriter.BaseStream.Length > 1024 * 1024) // 1MB
            {
                rfidWriter.Flush();
            }
        }

        public void Start()
        {
            if (!IsRecording)
            {
                this.IsRecording = true;
                this.RecordingStarted?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRecording)
            {
                this.IsRecording = false;
                this.RecordingStopped?.Invoke();
            }
        }

        public void Dispose()
        {
            this.Stop();
            if (this.rfidWriter != null)
            {
                this.rfidWriter.Flush();
                this.rfidWriter.Dispose();
                this.rfidWriter = null;
            }
        }
    }
}
