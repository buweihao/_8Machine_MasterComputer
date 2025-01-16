using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8Machine_MasterComputer.ViewModel
{
    public class MasterComputerVM: INotifyPropertyChanged
    {
        #region  属性
        // 按钮状态属性
        private bool _test;
        public bool Test
        {
            get => _test;
            set
            {
                if (_test != value)
                {
                    _test = value;
                    OnPropertyChanged(nameof(Test));
                }
            }
        }

        private bool _start;
        public bool Start
        {
            get => _start;
            set
            {
                if (_start != value)
                {
                    _start = value;
                    OnPropertyChanged(nameof(Start));
                }
            }
        }

        private bool _pause;
        public bool Pause
        {
            get => _pause;
            set
            {
                if (_pause != value)
                {
                    _pause = value;
                    OnPropertyChanged(nameof(Pause));
                }
            }
        }

        private bool _stop;
        public bool Stop
        {
            get => _stop;
            set
            {
                if (_stop != value)
                {
                    _stop = value;
                    OnPropertyChanged(nameof(Stop));
                }
            }
        }

        private bool _restart;
        public bool Restart
        {
            get => _restart;
            set
            {
                if (_restart != value)
                {
                    _restart = value;
                    OnPropertyChanged(nameof(Restart));
                }
            }
        }

        private bool _sysReset;
        public bool SysReset
        {
            get => _sysReset;
            set
            {
                if (_sysReset != value)
                {
                    _sysReset = value;
                    OnPropertyChanged(nameof(SysReset));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public readonly TextWriter _textBoxWriter;

        #endregion

        public MasterComputerVM(TextWriter textBoxWriter)
        {
            _textBoxWriter = textBoxWriter;

        }
    }
}
