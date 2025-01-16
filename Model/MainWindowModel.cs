using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8Machine_MasterComputer.Model
{
    class MainWindowModel
    {
        /// <summary>
        /// 测试按钮状态
        /// </summary>
        public bool Test { get; set; }

        /// <summary>
        /// 开始按钮状态
        /// </summary>
        public bool Start { get; set; }

        /// <summary>
        /// 暂停按钮状态
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// 停止按钮状态
        /// </summary>
        public bool Stop { get; set; }

        /// <summary>
        /// 重新开始按钮状态
        /// </summary>
        public bool Restart { get; set; }

        /// <summary>
        /// 复位按钮状态
        /// </summary>
        public bool SysReset { get; set; }
    }
}
