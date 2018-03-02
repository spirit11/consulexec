using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{

    /// <summary>
    /// Interaction logic for CommandRunView.xaml
    /// </summary>
    public partial class CommandRunView : IViewFor<CommandRunViewModel>
    {
        public CommandRunView()
        {
            InitializeComponent();
        }

        public CommandRunViewModel ViewModel
        {
            get { return (CommandRunViewModel)DataContext; }
            set { DataContext = value; }
        }


        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
