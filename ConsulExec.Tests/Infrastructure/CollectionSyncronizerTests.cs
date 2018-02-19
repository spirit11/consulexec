using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ConsulExec.Infrastructure;
using NUnit.Framework;

namespace ConsulExec.Tests.Infrastructure
{
    [TestFixture]
    //[TestFixtureSource(nameof(Param))]
    public class CollectionSyncronizerTests : AssertionHelper
    {
        public static object[][] Param = { new object[] { true }, new object[] { false } };

        public CollectionSyncronizerTests()
        {
            fillModelCollection = true;
        }

        public CollectionSyncronizerTests(bool FillModelCollection)
        {
            fillModelCollection = FillModelCollection;
        }

        [SetUp]
        public void SetUp()
        {
            viewModels = new ObservableCollection<ViewModel>();
            models = new List<Model>();

            AssertCollectionsInSync();
            if (fillModelCollection)
            {
                models.AddRange(Enumerable.Range(0, 5).Select(_ => new Model()));
                Expect(AssertCollectionsInSync, Throws.Exception.TypeOf<AssertionException>());
            }

            CollectionSyncronizer.Bind(
                viewModels,
                models,
                m => new ViewModel(m),
                vm => vm.Model);
        }

        [Test]
        public void ViewModelsAreFilledInitially()
        {
            AssertCollectionsInSync();
        }

        [Test]
        public void WhenViewModelsAreAddedModelsSync()
        {
            for (int i = 0; i < 5; i++)
            {
                viewModels.Add(new ViewModel());
                AssertCollectionsInSync();
            }
        }

        [Test]
        public void WhenViewModelsAreRemovedModelsSync()
        {
            ;
            foreach (var position in new[] { viewModels.Count - 1, 0, 1 }) // last, first, center
            {
                viewModels.RemoveAt(position);
                AssertCollectionsInSync();
            }
        }

        [Test]
        public void WhenViewModelsAreMovedModelsSync()
        {

            viewModels.Move(2, 4);
            AssertCollectionsInSync();
            viewModels.Move(4, 2);
            AssertCollectionsInSync();
        }

        [Test]
        public void WhenViewModelsClearModelsSync()
        {

            viewModels.Clear();
            AssertCollectionsInSync();
        }

        [Test]
        public void WhenViewModelPropertyChangedModelsSync()
        {

            viewModels[0].Model = null;
            AssertCollectionsInSync();
        }


        #region TestClasses

        private class Model
        {
            public Model()
            {
                Id = Guid.NewGuid().ToString();
            }
            public string Id { get; }
            public override string ToString()
            {
                return $"Model {Id.Substring(0, 4)}...";
            }
        }


        private class ViewModel : INotifyPropertyChanged
        {
            public ViewModel() : this(new Model())
            {
            }

            public ViewModel(Model Model)
            {
                this.Model = Model;
            }

            public Model Model
            {
                get { return model; }
                set
                {
                    if (model != value)
                    {
                        model = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
                    }
                }
            }
            private Model model;

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"ViewModel {Model?.Id.Substring(0, 4) ?? "<null>"}";
            }
        }

        #endregion

        private readonly bool fillModelCollection;

        private ObservableCollection<ViewModel> viewModels;
        private List<Model> models;

        private void AssertCollectionsInSync()
        {
            Expect(viewModels.Select(vm => vm.Model).ToList(), Is.EqualTo(models));
        }
    }
}
