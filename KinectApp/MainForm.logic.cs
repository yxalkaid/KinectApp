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
        private string audioDir="D:/Downloads/Kinect";

        /// <summary>
        /// 骨骼数据保存目录
        /// </summary>
        private string bodyDir="D:/Downloads/Kinect";

        private void InitializeVideoCapturer()
        {
            videoCapturer = new VideoCapturer();
            videoCapturer.FrameArrived += HandleVideoFrame;
            videoCapturer.Initialize();
            videoCapturer.Start();
        }

        private void InitializeAudioCapturer()
        {
            audioCapturer = new AudioCapturer();
            //audioCapturer.AudioFrameArrived += HandleAudioFrame;
            audioCapturer.Initialize();
            audioCapturer.Start();
        }

        private void InitializeBodyCapturer()
        {
            bodyCapturer = new BodyCapturer();
            bodyCapturer.FrameArrived += HandleBodyFrame;
            bodyCapturer.Initialize();
            bodyCapturer.Start();
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
                return true;
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
    }
}
