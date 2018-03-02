using ConsulExec.ViewModel;
using ReactiveUI;

namespace ConsulExec.View
{
    /// <summary>
    /// Interaction logic for NodeRunView.xaml
    /// </summary>
    public partial class NodeRunView : IViewFor<NodeRunViewModel>
    {
        public NodeRunView()
        {
            InitializeComponent();
        }

        public NodeRunViewModel ViewModel
        {
            get { return (NodeRunViewModel)DataContext; }
            set { DataContext = value; }
        }


        object IViewFor.ViewModel
        {
            get { return DataContext; }
            set { DataContext = value; }
        }
    }
}
