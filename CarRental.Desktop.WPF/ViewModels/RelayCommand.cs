using System;
using System.Windows.Input;
using System.Threading.Tasks;

// Assurez-vous que ce namespace correspond à celui de votre FinancialReportViewModel
namespace CarRental.Desktop.WPF.ViewModels
{

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Mise en place de l'événement CanExecuteChanged pour rafraîchir le statut du bouton
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }

   
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;

        // Un champ pour stocker l'événement (souvent non utilisé dans cette implémentation simple)
        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<object, Task> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        // Permet l'exécution par défaut (true)
        public bool CanExecute(object parameter) => true;

        // Appel asynchrone qui permet à l'UI de rester réactive
        public async void Execute(object parameter)
        {
            await _execute(parameter);
        }
    }
}