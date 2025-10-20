using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF_SQL_NET_Framework.Command
{
    public class ButtonCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<object> _act;
        private readonly Func<bool> _func;

        public ButtonCommand(Action excute, Func<bool> canExecute = null)
        {
            if (excute == null)
            {
                throw new ArgumentNullException(nameof(excute));
            }

            _act = (object obj) => excute(); // action -> action<object>로 변환
            _func = canExecute;
        }
        public ButtonCommand(Action<object> excute, Func<bool> canExecute = null)
        {
            if (excute == null)
            {
                throw new ArgumentNullException(nameof(excute));
            }
            _act = excute;
            _func = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _func == null ? true : _func();
        }

        public void Execute(object parameter)
        {
            _act(parameter);
        }
    }
}
