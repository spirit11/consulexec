using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{
    /// <summary>
    /// Interaction logic for StartupOptionsView.xaml
    /// </summary>
    public partial class ProfileEditorView : IViewFor<ProfileEditorViewModel>
    {
        public ProfileEditorView()
        {
            InitializeComponent();
        }
        public ProfileEditorViewModel ViewModel
        {
            get { return (ProfileEditorViewModel)DataContext; }
            set { DataContext = value; }
        }

        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
