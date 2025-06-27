using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KinectApp
{
    /// <summary>
    /// Kinect 骨骼数据采集器
    /// </summary>
    public class BodyCapturer : BaseCapturer
    {
        /// <summary>
        /// (已修改) 只包含指定关节的过滤后骨骼数据帧到达事件
        /// </summary>
        public event Action<List<FilteredBody>> FilteredFrameArrived;

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

        private BodyFrameReader bodyReader;

        public override void Initialize()
        {
            base.Initialize();
            try
            {
                bodyReader = Sensor.BodyFrameSource.OpenReader();
                bodyReader.FrameArrived += OnBodyFrameArrived;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

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
                        }
                        filteredBodies.Add(filteredBody);
                    }
                }
                
                if (filteredBodies.Any())
                {
                    FilteredFrameArrived?.Invoke(filteredBodies);
                }
            }
        }

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
