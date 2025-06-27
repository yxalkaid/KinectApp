using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace KinectApp
{
    /// <summary>
    /// kinect 骨骼数据保存器
    /// </summary>
    public class BodySaver : IDisposable
    {
        /// <summary>
        /// 骨骼数据写入器
        /// </summary>
        private StreamWriter bodyWriter;

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 录制开始事件
        /// </summary>
        public event Action RecordingStarted;

        /// <summary>
        /// 录制结束事件
        /// </summary>
        public event Action RecordingStopped;


        public BodySaver(string parentDir)
        {
            // 创建目录
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            // 生成文件路径
            this.FilePath = Path.Combine(parentDir, $"kinect_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            // 初始化数据写入器
            FileStream fileStream= new FileStream(FilePath, FileMode.CreateNew,FileAccess.Write);
            this.bodyWriter = new StreamWriter(fileStream) { AutoFlush=false };

            if (this.bodyWriter==null)
            {
                throw new IOException("无法打开骨骼数据写入器，可能权限不足或路径无效。");
            }
        }


        /// <summary>
        /// 写入一帧数据到文件
        /// </summary>
        public void WriteFrame(Body[] bodies)
        {
            if(!IsRecording||bodies.Length==0)
                return;

            try
            {
                foreach (var body in bodies)
                {
                    if (body.IsTracked)
                    {
                        List<string> row = new List<string>();

                        // 用户唯一标识
                        row.Add(body.TrackingId.ToString());

                        // 添加所有关节坐标
                        foreach (JointType joint in Enum.GetValues(typeof(JointType)))
                        {
                            var position = body.Joints[joint].Position;
                            row.Add(position.X.ToString("F6"));
                            row.Add(position.Y.ToString("F6"));
                            row.Add(position.Z.ToString("F6"));
                        }


                        string line = string.Join(",", row);
                        bodyWriter.WriteLine(line);
                    }
                }

                if (bodyWriter.BaseStream.Length > 1024*1024)
                {
                    bodyWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入 CSV 数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void Start()
        {
            if (!IsRecording)
            {
                this.IsRecording = true;
                this.RecordingStarted?.Invoke();
            }
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void Stop()
        {
            if (IsRecording)
            {
                this.IsRecording = false;
                this.RecordingStopped?.Invoke();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            if (this.bodyWriter != null)
            {
                this.bodyWriter.Flush();
                this.bodyWriter.Dispose(); // 释放对象
                this.bodyWriter = null;
            }
        }
    }
}
