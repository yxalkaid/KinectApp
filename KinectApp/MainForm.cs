using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KinectApp
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class MainForm: Form
    {
        /// <summary>
        /// 是否正在录制
        /// </summary>
        private bool IsRecording=false;

        /// <summary>
        /// 是否已连接
        /// </summary>
        private bool IsConnected=false;

        /// <summary>
        /// 视频采集器
        /// </summary>
        private VideoCapturer videoCapturer;

        /// <summary>
        /// 音频采集器
        /// </summary>
        private AudioCapturer audioCapturer;

        /// <summary>
        /// 骨骼数据采集器
        /// </summary>
        private BodyCapturer bodyCapturer;

        /// <summary>
        /// RFID数据采集器
        /// </summary>
        private RFIDCapturer rfidCapturer;

        /// <summary>
        /// 视频保存器
        /// </summary>
        private VideoSaver videoSaver;

        /// <summary>
        /// 音频保存器
        /// </summary>
        private AudioSaver audioSaver;

        /// <summary>
        /// 骨骼数据保存器
        /// </summary>
        private BodySaver bodySaver;

        /// <summary>
        /// RFID 数据保存器
        /// </summary>
        private RFIDSaver rfidSaver;

        public MainForm()
        {
            InitializeComponent();

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
            this.DisposeAll();

            // 调用基类方法
            base.OnFormClosed(e);
        }

        private void DisposeAll()
        {
            // 释放视频采集资源
            videoSaver?.Dispose();
            videoSaver = null;
            videoCapturer?.Dispose();
            videoCapturer = null;

            // 释放音频采集资源
            audioSaver?.Dispose();
            audioSaver = null;
            audioCapturer?.Dispose();
            audioCapturer = null;

            // 释放骨骼数据采集资源
            bodySaver?.Dispose();
            bodySaver = null;
            bodyCapturer?.Dispose();
            bodyCapturer = null;

            // 释放RFID数据采集资源
            rfidSaver?.Dispose();
            rfidSaver = null;
            rfidCapturer?.Dispose();
            rfidCapturer = null;
        }

        private void initButton_Click(object sender, EventArgs e)
        {
            if (this.IsConnected == false)
            {
                this.InitializeVideoCapturer();
                this.InitializeAudioCapturer();
                this.InitializeBodyCapturer();
                //this.InitializeRFIDCapturer();
            }
            else
            {
                this.DisposeAll();
            }

            this.IsConnected = !this.IsConnected;
            this.initButton.Text = this.IsConnected ? "Disconnect" : "Connect";
            string info = this.IsConnected ? "已连接设备" : "已断开连接";
            DialogResult result = MessageBox.Show(
                info,
                "确认",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (this.IsRecording == false)
            {
                this.StartVideoSaver();
                this.StartAudioSaver();
                this.StartBodySaver();
                this.StartRFIDSaver();
            }
            else
            {
                this.StopVideoSaver();
                this.StopAudioSaver();
                this.StopBodySaver();
                this.StopRFIDSaver();
            }

            this.IsRecording=!this.IsRecording;
            this.startButton.Text = this.IsRecording ? "Stop" : "Start";

            string info=this.IsRecording ? "已开始录制" : "已停止录制";
            DialogResult result = MessageBox.Show(
                info,
                "确认",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
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

        /// <summary>
        /// 处理骨骼数据帧
        /// </summary>
        /// <param name="filteredBodies"></param>
        private void HandleBodyFrame(List<FilteredBody> filteredBodies)
        {
            
        }

        /// <summary>
        /// 处理 RFID 数据帧
        /// </summary>
        private void HandleRFIDFrame(List<SimpleTagData> dataList)
        {
            foreach (SimpleTagData data in dataList)
            {
                if (listView.InvokeRequired)
                {
                    this.listView.Invoke(new Action(() => this.listView.Items.Add(data.ToString())));
                }
                else
                {
                    this.listView.Items.Add(data.ToString());
                }
            }
            MessageBox.Show("Tag Data Received");
        }
    }
}
