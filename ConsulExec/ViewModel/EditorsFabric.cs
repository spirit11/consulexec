using System;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public interface IEditorsFactory
    {
        void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup);
        void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup);
    }

    public class EditorsFactory : IEditorsFactory
    {
        public EditorsFactory(IActivatingViewModel ActivatingViewModel,
            ReactiveList<ProfileViewModel<ConnectionOptions>> ConnectionsList,
            UndoListViewModel UndoList,
            ConnectionOptionsFactoryDelegate ConnectionConnectionOptionsFabric)
        {
            activatingViewModel = ActivatingViewModel;
            connectionsList = ConnectionsList;
            undoList = UndoList;
            connectionConnectionOptionsFabric = ConnectionConnectionOptionsFabric;
        }

        public void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup)
        {
            Activate(EditorSetup,
                new StartupOptionsEditorViewModel(Profile,
                    new ConnectionProfilesViewModel(EditConnectionOptions, undoList, connectionsList, connectionConnectionOptionsFabric),
                    activatingViewModel
            ));
        }

        public void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup)
        {
            Activate(EditorSetup, new ConnectionOptionsEditorViewModel(Profile, connectionConnectionOptionsFabric, activatingViewModel));
        }

        private readonly IActivatingViewModel activatingViewModel;
        private readonly ReactiveList<ProfileViewModel<ConnectionOptions>> connectionsList;
        private readonly UndoListViewModel undoList;
        private readonly ConnectionOptionsFactoryDelegate connectionConnectionOptionsFabric;

        private void Activate<T>(Action<IProfileEditorViewModel<ProfileViewModel<T>>> Setup,
            IProfileEditorViewModel<ProfileViewModel<T>> ProfileEditorViewModel)
        {
            Setup(ProfileEditorViewModel);
            activatingViewModel?.Activate(ProfileEditorViewModel);
        }
    }
}