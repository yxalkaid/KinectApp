using Microsoft.Kinect;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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
        /// 骨骼数据采集器
        /// </summary>
        private BodyCapturer bodyCapturer;

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

        public MainForm()
        {
            InitializeComponent();

            InitializeVideoCapturer();
            InitializeAudioCapturer();
            InitializeBodyCapturer();

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
            videoSaver?.Dispose();
            videoCapturer?.Dispose();

            // 释放资源
            audioSaver?.Dispose();
            audioCapturer?.Dispose();

            // 调用基类方法
            base.OnFormClosed(e);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (this.videoSaver == null)
            {
                this.StartVideoSaver();
                this.StartAudioSaver();
                this.StartBodySaver();

               /* DialogResult result = MessageBox.Show(
                    "已开始录制",
                    "确认",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );*/
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
                this.StopBodySaver();

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

        private void HandleBodyFrame(List<FilteredBody> filteredBodies)
        {

            //var headJoint = filteredBodies[0].Joints[JointType.Head];

            //// 创建位图（如果尚未创建）
            //if (pictureBox.Image == null)
            //{
            //    pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            //}

            //using (Graphics g = Graphics.FromImage(pictureBox.Image))
            //{
            //    // 清除之前的绘制
            //    g.Clear(Color.Black);

            //    // 绘制红色圆点（头部位置）
            //    int radius = 10;
            //    Rectangle rect = new Rectangle(
            //        (int)(colorPoint.X - radius),
            //        (int)(colorPoint.Y - radius),
            //        radius * 2,
            //        radius * 2
            //    );

            //    g.FillEllipse(Brushes.Red, rect);
            //}

            //// 刷新 PictureBox
            //pictureBox.Refresh();
        }


    }
}
