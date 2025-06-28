using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KinectApp
{
    partial class MainForm
    {
        /// <summary>
        /// 视频保存目录
        /// </summary>
        private string videoDir = "D:/Downloads/Kinect";

        /// <summary>
        /// 音频保存目录
        /// </summary>
        private string audioDir= "D:/Downloads/Kinect";

        /// <summary>
        /// 骨骼数据保存目录
        /// </summary>
        private string bodyDir= "D:/Downloads/Kinect";

        /// <summary>
        /// RFID 数据保存目录
        /// </summary>
        private string rfidDir= "D:/Downloads/Kinect";

        /// <summary>
        /// 初始化视频采集器
        /// </summary>
        private void InitializeVideoCapturer()
        {
            if (videoCapturer == null)
            {
                videoCapturer = new VideoCapturer();
                videoCapturer.FrameArrived += HandleVideoFrame;
                videoCapturer.Initialize();
                videoCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化音频采集器
        /// </summary>
        private void InitializeAudioCapturer()
        {
            if (audioCapturer == null)
            {
                audioCapturer = new AudioCapturer();
                //audioCapturer.FrameArrived += HandleAudioFrame;
                audioCapturer.Initialize();
                audioCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化骨骼数据检测器
        /// </summary>
        private void InitializeBodyCapturer()
        {
            if(bodyCapturer == null)
            {
                bodyCapturer = new BodyCapturer();
                bodyCapturer.FrameArrived += HandleBodyFrame;
                bodyCapturer.Initialize();
                bodyCapturer.Start();
            }
        }

        /// <summary>
        /// 初始化RFID数据获取器
        /// </summary>
        private void InitializeRFIDCapturer()
        {
            if (rfidCapturer == null)
            {
                rfidCapturer = new RFIDCapturer(
                    "Speedwayr-11-25-ab.local"
                );
                //rfidCapturer.FrameArrived += HandleRFIDFrame;
                rfidCapturer.Initialize();
                //rfidCapturer.Start();
            }
        }

        /// <summary>
        /// 开始视频录制
        /// </summary>
        /// <returns></returns>
        private bool StartVideoSaver()
        {
            if (videoCapturer == null)
            {
                return false;
            }

            if (videoSaver != null)
            {
                return false;
            }

            videoSaver = new VideoSaver(
                videoDir,
                1920, 1080
            );
            videoCapturer.FrameArrived += videoSaver.WriteFrame;
            videoSaver.Start();

            return true;
        }


        /// <summary>
        /// 停止视频录制
        /// </summary>
        /// <returns></returns>
        private bool StopVideoSaver()
        {
            if (videoSaver == null)
            {
                return false;
            }

            videoSaver.Dispose();
            videoCapturer.FrameArrived -= videoSaver.WriteFrame;
            videoSaver = null;

            return true;
        }

        /// <summary>
        /// 开始音频录制
        /// </summary>
        /// <returns></returns>
        private bool StartAudioSaver()
        {
            if (audioCapturer == null)
            {
                return false;
            }

            if (audioSaver != null)
            {
                return false;
            }

            audioSaver = new AudioSaver(
                audioDir
            );
            audioCapturer.FrameArrived += audioSaver.WriteFrame;
            audioSaver.Start();

            return true;
        }

        /// <summary>
        /// 停止音频录制
        /// </summary>
        /// <returns></returns>
        private bool StopAudioSaver()
        {
            if (audioSaver == null)
            {
                return false;
            }

            audioSaver.Dispose();
            audioCapturer.FrameArrived -= audioSaver.WriteFrame;
            audioSaver = null;

            return true;
        }

        /// <summary>
        /// 开始骨骼数据录制
        /// </summary>
        private bool StartBodySaver()
        {
            if (bodyCapturer == null)
            {
                return false;
            }

            if (bodySaver != null)
            {
                return false;
            }

            bodySaver = new BodySaver(
                bodyDir
            );
            bodyCapturer.FrameArrived += bodySaver.WriteFrame;
            bodySaver.Start();

            return true;
        }

        /// <summary>
        /// 停止骨骼数据录制
        /// </summary>
        /// <returns></returns>
        private bool StopBodySaver()
        {
            if (bodySaver == null)
            {
                return false;
            }

            bodySaver.Dispose();
            bodyCapturer.FrameArrived -= bodySaver.WriteFrame;
            bodySaver = null;

            return true;
        }

        /// <summary>
        /// 开始RFID数据录制
        /// </summary>
        /// <returns></returns>
        private bool StartRFIDSaver()
        { 
            if(rfidCapturer == null)
            {
                return false;
            }

            if(rfidSaver != null)
            { 
                return false;
            }

            rfidSaver = new RFIDSaver(
                rfidDir
            );
            rfidCapturer.FrameArrived += rfidSaver.WriteFrame;
            rfidCapturer.Start();
            rfidSaver.Start();

            return true;
        }

        /// <summary>
        /// 停止RFID数据录制
        /// </summary>
        /// <returns></returns>
        private bool StopRFIDSaver()
        {
            if (rfidSaver == null)
            {
                return false;
            }

            rfidSaver.Dispose();
            rfidCapturer.Stop();
            rfidCapturer.FrameArrived -= rfidSaver.WriteFrame;
            rfidSaver = null;

            return true;
        }
    }
}
