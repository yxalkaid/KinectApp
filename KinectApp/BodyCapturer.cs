using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KinectApp
{
    /// <summary>
    /// Kinect 骨骼数据采集器
    /// </summary>
    public class BodyCapturer : KinectCapturer
    {

        /// <summary>
        /// 骨骼数据捕获
        /// </summary>
        private BodyFrameReader bodyReader;

        /// <summary>
        /// (已修改) 只包含指定关节的过滤后骨骼数据帧到达事件
        /// </summary>
        public event Action<List<FilteredBody>> FrameArrived;

        /// <summary>
        /// 坐标映射器
        /// </summary>
        private CoordinateMapper coordinateMapper;

        /// <summary>
        /// 我们希望追踪的关节点列表
        /// </summary>
        private readonly List<JointType> _requiredJoints = new List<JointType>
        {
            JointType.SpineBase,
            JointType.Neck,
            JointType.HipLeft,
            JointType.KneeLeft,
            JointType.HipRight,
            JointType.KneeRight,
            JointType.ShoulderLeft,
            JointType.ElbowLeft,
            JointType.WristLeft,
            JointType.ShoulderRight,
            JointType.ElbowRight,
            JointType.WristRight
        };
        
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            try
            {
                coordinateMapper = Sensor.CoordinateMapper;
                bodyReader = Sensor.BodyFrameSource.OpenReader();
                bodyReader.FrameArrived += OnBodyFrameArrived;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        /// <summary>
        /// 骨骼数据帧到达事件处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;

                Body[] bodies = new Body[frame.BodyCount];
                frame.GetAndRefreshBodyData(bodies);


                var filteredBodies = new List<FilteredBody>();

                foreach (var body in bodies)
                {
                    if (body != null && body.IsTracked)
                    {
                        var filteredBody = new FilteredBody
                        {
                            TrackingId = body.TrackingId,
                            Timestamp = DateTime.Now
                        }; 

                        foreach (var jointType in _requiredJoints)
                        {
                            filteredBody.Joints[jointType] = body.Joints[jointType];

                            //Joint originalJoint = body.Joints[jointType];
                            //CameraSpacePoint cameraPoint = originalJoint.Position;
                            //ColorSpacePoint colorPoint = coordinateMapper.MapCameraPointToColorSpace(cameraPoint);
                            //originalJoint.Position.X = colorPoint.X;
                            //originalJoint.Position.Y = colorPoint.Y;
                            //originalJoint.Position.Z = 0;
                            //filteredBody.Joints[jointType] = originalJoint;
                        }
                        filteredBodies.Add(filteredBody);
                    }
                }
                
                if (filteredBodies.Any())
                {
                    FrameArrived?.Invoke(filteredBodies);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (bodyReader != null)
            {
                bodyReader.FrameArrived -= OnBodyFrameArrived;
                bodyReader.Dispose();
                bodyReader = null;
            }
            base.Dispose();
        }
    }
}
