using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{
    /// <summary>
    /// Interaction logic for CommandStartupView.xaml
    /// </summary>
    public partial class CommandStartupView : IViewFor<CommandStartupViewModel>
    {
        public CommandStartupView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.UndoCommand, v => v.UndoButton));
            });
        }

        public CommandStartupViewModel ViewModel
        {
            get { return (CommandStartupViewModel)DataContext; }
            set { DataContext = value; }
        }


        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
