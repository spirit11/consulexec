using System;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public interface IEditorsFabric
    {
        void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup);
        void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup);
    }

    public class EditorsFabric : IEditorsFabric
    {
        public EditorsFabric(IActivatingViewModel ActivatingViewModel,
            ReactiveList<ProfileViewModel<ConnectionOptions>> ConnectionsList,
            UndoListViewModel UndoList)
        {
            activatingViewModel = ActivatingViewModel;
            connectionsList = ConnectionsList;
            undoList = UndoList;
        }

        public void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup)
        {
            Activate(EditorSetup, 
                new StartupOptionsEditorViewModel(Profile,
                    new ConnectionProfilesViewModel(EditConnectionOptions, undoList, connectionsList),
                    activatingViewModel
            ));
        }

        public void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup)
        {
            Activate(EditorSetup, new ConnectionOptionsEditorViewModel(Profile, activatingViewModel));
        }

        private readonly IActivatingViewModel activatingViewModel;
        private readonly ReactiveList<ProfileViewModel<ConnectionOptions>> connectionsList;
        private readonly UndoListViewModel undoList;

        private void Activate<T>(Action<IProfileEditorViewModel<ProfileViewModel<T>>> Setup,
            IProfileEditorViewModel<ProfileViewModel<T>> ProfileEditorViewModel)
        {
            Setup(ProfileEditorViewModel);
            activatingViewModel?.Activate(ProfileEditorViewModel);
        }
    }
}