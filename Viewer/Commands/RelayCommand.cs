using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Viewer.Commands {
    class RelayCommand : ICommand {
        // reference: https://stackoverflow.com/questions/1468791
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null) {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter) => this.execute(parameter);
        public bool CanExecute(object parameter) => this.canExecute?.Invoke(parameter) ?? true;
    }
}
