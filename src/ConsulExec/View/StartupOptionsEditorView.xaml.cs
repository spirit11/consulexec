using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{
    /// <summary>
    /// Interaction logic for StartupOptionsView.xaml
    /// </summary>
    public partial class StartupOptionsEditorView : IViewFor<StartupOptionsEditorViewModel>
    {
        public StartupOptionsEditorView()
        {
            InitializeComponent();
        }
        public StartupOptionsEditorViewModel ViewModel
        {
            get { return (StartupOptionsEditorViewModel)DataContext; }
            set { DataContext = value; }
        }

        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
