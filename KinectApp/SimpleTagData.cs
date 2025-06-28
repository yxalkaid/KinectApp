using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectApp
{
    /// <summary>
    /// 简单标签数据
    /// </summary>
    public class SimpleTagData
    {
        public String Time;
        public String EPC;
        public String Channel;
        public String Phase;
        public String RSSI;
        public String Antenna;

        public override string ToString()
        {
            return Time + "," + EPC + "," + Channel + "," + Phase + "," + RSSI + "," + Antenna;
        }
    }
}
