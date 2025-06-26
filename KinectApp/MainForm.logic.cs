using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectApp
{
    partial class MainForm
    {
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

            videoSaver = new VideoSaver(
                "D:/Downloads/Kinect",
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

            audioSaver = new AudioSaver(
                "D:/Downloads/Kinect"
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
        /// 处理视频帧
        /// </summary>
        /// <param name="frame"></param>
        private void HandleVideoFrame(Bitmap image)
        {

            var oldImage = pictureBox.Image;

            // 安全更新PictureBox
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => pictureBox.Image = image));
            }
            else
            {
                pictureBox.Image = image;
            }

            if (oldImage != null && oldImage != image)
            {
                oldImage.Dispose();
                oldImage = null;
            }
        }
    }
}
