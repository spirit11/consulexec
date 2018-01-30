using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{
    /// <summary>
    /// Interaction logic for StartupOptionsView.xaml
    /// </summary>
    public partial class ConnectionOptionsEditorView : IViewFor<ConnectionOptionsEditorViewModel>
    {
        public ConnectionOptionsEditorView()
        {
            InitializeComponent();
        }
        public ConnectionOptionsEditorViewModel ViewModel
        {
            get { return (ConnectionOptionsEditorViewModel)DataContext; }
            set { DataContext = value; }
        }

        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
