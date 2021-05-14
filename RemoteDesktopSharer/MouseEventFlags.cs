using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopSharer
{
    public class MouseEventFlags
    {
        /// <summary>
        /// 在 DX和 DY参数包含归绝对坐标。如果未设置，则这些参数包含相对数据：自上次报告位置以来的位置变化。无论将哪种鼠标或类似鼠标的设备连接到系统，都可以设置或不设置此标志。有关鼠标相对运动的更多信息，请参见以下“备注”部分。
        /// </summary>
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        /// <summary>
        /// 左按钮按下
        /// </summary>
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        /// <summary>
        /// 左按钮向上
        /// </summary>
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        /// <summary>
        /// 中间按钮按下
        /// </summary>
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        /// <summary>
        /// 中间按钮向上
        /// </summary>
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        /// <summary>
        /// 运动发生了
        /// </summary>
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        /// <summary>
        /// 右按钮按下
        /// </summary>
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        /// <summary>
        /// 右侧按钮向上
        /// </summary>
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        /// <summary>
        /// 按下了X按钮
        /// </summary>
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        /// <summary>
        /// X按钮被释放
        /// </summary>
        public const uint MOUSEEVENTF_XUP = 0x0100;
        /// <summary>
        /// 滚轮按钮被旋转
        /// </summary>
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        /// <summary>
        /// 滚轮按钮倾斜
        /// </summary>
        public const uint MOUSEEVENTF_HWHEEL = 0x01000;
    }
}
