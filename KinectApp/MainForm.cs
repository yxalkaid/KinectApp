using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KinectApp
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class MainForm: Form
    {
        /// <summary>
        /// 视频采集器
        /// </summary>
        private VideoCapturer videoCapturer;

        /// <summary>
        /// 音频采集器
        /// </summary>
        private AudioCapturer audioCapturer;

        /// <summary>
        /// 视频保存器
        /// </summary>
        private VideoSaver videoSaver;

        /// <summary>
        /// 音频保存器
        /// </summary>
        private AudioSaver audioSaver;

        public MainForm()
        {
            InitializeComponent();

            InitializeVideoCapturer();
            InitializeAudioCapturer();
        }


        /// <summary>
        /// 重写窗体关闭前方法
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 显示确认对话框
            DialogResult result = MessageBox.Show(
                "确定要退出程序吗？",
                "确认退出",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }

            // 调用基类方法
            base.OnFormClosing(e);
        }

        /// <summary>
        /// 重写窗体关闭后方法
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 释放资源
            videoCapturer?.Dispose();

            // 调用基类方法
            base.OnFormClosed(e);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (this.videoSaver == null)
            {
                this.StartVideoSaver();
                this.StartAudioSaver();

                DialogResult result = MessageBox.Show(
                    "已开始录制",
                    "确认",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                DialogResult result = MessageBox.Show(
                    "正在录制中",
                    "确认",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (this.videoSaver != null)
            {
                this.StopVideoSaver();
                this.StopAudioSaver();

                DialogResult result = MessageBox.Show(
                    "已停止录制",
                    "确认",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                DialogResult result = MessageBox.Show(
                    "未进行录制",
                    "确认",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
    }
}
