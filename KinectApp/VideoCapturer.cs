using System;
using System.Drawing;
using Microsoft.Kinect;

namespace KinectApp
{
    /// <summary>
    /// kinect 视频采集器
    /// </summary>
    public class VideoCapturer : KinectCapturer
    {
        /// <summary>
        /// 视频帧到达事件
        /// </summary>
        public event Action<Bitmap> FrameArrived;

        /// <summary>
        /// 视频帧捕获
        /// </summary>
        private ColorFrameReader frameReader;

        /// <summary>
        /// 位图
        /// </summary>
        private Bitmap colorBitmap;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            try
            {
                // 配置彩色帧源
                var colorSource = this.Sensor.ColorFrameSource;

                // 创建帧读取器
                this.frameReader = colorSource.OpenReader();
                frameReader.FrameArrived += OnColorFrameArrived;

                // 初始化位图
                colorBitmap = new Bitmap(
                    colorSource.FrameDescription.Width,
                    colorSource.FrameDescription.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb
                );
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        /// <summary>
        /// 视频帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;

                try
                {
                    // 锁定位图数据
                    var bitmapData = colorBitmap.LockBits(
                        new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        colorBitmap.PixelFormat
                    );

                    try
                    {
                        // 复制帧数据到位图
                        frame.CopyConvertedFrameDataToIntPtr(
                            bitmapData.Scan0,
                            (uint)(colorBitmap.Width * colorBitmap.Height * 4),
                            ColorImageFormat.Bgra
                        );

                        colorBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        // 触发帧到达事件
                        FrameArrived?.Invoke((Bitmap)colorBitmap.Clone());
                    }
                    finally
                    {
                        colorBitmap.UnlockBits(bitmapData);
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(ex);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            // 释放帧读取器
            if (frameReader != null)
            {
                frameReader.FrameArrived -= OnColorFrameArrived;
                frameReader.Dispose();
                frameReader = null;
            }

            // 释放位图
            if (colorBitmap != null)
            {
                colorBitmap.Dispose();
                colorBitmap = null;
            }

            base.Dispose();
        }
    }
}