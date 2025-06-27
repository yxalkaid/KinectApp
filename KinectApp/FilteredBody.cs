using Microsoft.Kinect;
using System;
using System.Collections.Generic;

namespace KinectApp
{
    /// <summary>
    /// 用于存储单个被追踪者过滤后的骨骼数据
    /// </summary>
    public class FilteredBody
    {
        /// <summary>
        /// 用户的唯一追踪ID
        /// </summary>
        public ulong TrackingId { get; set; }

        /// <summary>
        /// 包含指定关节的字典
        /// </summary>
        public Dictionary<JointType, Joint> Joints { get; set; }

        /// <summary>
        /// 时间戳（UTC 时间）
        /// </summary>
        public DateTime Timestamp { get; set; }

        public FilteredBody()
        {
            Joints = new Dictionary<JointType, Joint>();
        }
    }
}
