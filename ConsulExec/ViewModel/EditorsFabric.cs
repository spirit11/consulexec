using System;
using ConsulExec.Domain;


namespace ConsulExec.ViewModel
{
    public interface IEditStartupFactory
    {
        void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup);
    }


    public interface IEditConnectionFactory
    {
        void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup);
    }


    public class EditStartupFactory : IEditStartupFactory
    {
        public EditStartupFactory(IActivatingViewModel ActivatingViewModel,
            ConnectionProfilesViewModel ConnectionProfilesViewModel)
        {
            activatingViewModel = ActivatingViewModel;
            connectionProfilesViewModel = ConnectionProfilesViewModel;
        }

        public void EditStartupOptions(ProfileViewModel<StartupOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<StartupOptions>>> EditorSetup)
        {
            Activate(EditorSetup,
                new StartupOptionsEditorViewModel(Profile,
                    connectionProfilesViewModel,
                    activatingViewModel
            ));
        }

        private readonly IActivatingViewModel activatingViewModel;
        private readonly ConnectionProfilesViewModel connectionProfilesViewModel;

        private void Activate<T>(Action<IProfileEditorViewModel<ProfileViewModel<T>>> Setup,
            IProfileEditorViewModel<ProfileViewModel<T>> ProfileEditorViewModel)
        {
            Setup(ProfileEditorViewModel);
            activatingViewModel?.Activate(ProfileEditorViewModel);
        }
    }


    public class EditConnectionFactory :  IEditConnectionFactory
    {
        public EditConnectionFactory(IActivatingViewModel ActivatingViewModel,
            ConnectionOptionsFactoryDelegate ConnectionConnectionOptionsFabric)
        {
            activatingViewModel = ActivatingViewModel;
            connectionConnectionOptionsFabric = ConnectionConnectionOptionsFabric;
        }

        public void EditConnectionOptions(ProfileViewModel<ConnectionOptions> Profile, Action<IProfileEditorViewModel<ProfileViewModel<ConnectionOptions>>> EditorSetup)
        {
            Activate(EditorSetup, new ConnectionOptionsEditorViewModel(Profile, connectionConnectionOptionsFabric, activatingViewModel));
        }

        private readonly IActivatingViewModel activatingViewModel;
        private readonly ConnectionOptionsFactoryDelegate connectionConnectionOptionsFabric;

        private void Activate<T>(Action<IProfileEditorViewModel<ProfileViewModel<T>>> Setup,
            IProfileEditorViewModel<ProfileViewModel<T>> ProfileEditorViewModel)
        {
            Setup(ProfileEditorViewModel);
            activatingViewModel?.Activate(ProfileEditorViewModel);
        }
    }
}