using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ConsulExec.Infrastructure
{
    public class CollectionSyncronizer
    {
        public static CollectionSyncronizer<TM, TVM, ObservableCollection<TVM>> Bind<TM, TVM>(
            ObservableCollection<TVM> ViewModelCollection,
            IList<TM> ModelCollection,
            Func<TM, TVM> ViewModelFactory,
            Expression<Func<TVM, TM>> ModelProperty)
            where TVM : INotifyPropertyChanged
        {
            return new CollectionSyncronizer<TM, TVM, ObservableCollection<TVM>>(ViewModelCollection, ModelCollection, ViewModelFactory, ModelProperty);
        }
    }


    public class CollectionSyncronizer<TM, TVM, TVMC>
        where TVM : INotifyPropertyChanged
        where TVMC : ICollection<TVM>, INotifyCollectionChanged
    {
        public CollectionSyncronizer(TVMC ViewModelCollection, IList<TM> ModelCollection,
            Func<TM, TVM> ViewModelFactory, Expression<Func<TVM, TM>> ModelProperty)
        {
            viewModelCollection = ViewModelCollection;
            viewModelFactory = ViewModelFactory;
            modelCollection = ModelCollection;
            modelProperty = ModelProperty.Compile();
            modelPropertyName = ((MemberExpression)ModelProperty.Body).Member.Name;
            FullSync();
            ViewModelCollection.CollectionChanged += CollectionChanged;
        }


        private readonly ICollection<TVM> viewModelCollection;
        private readonly IList<TM> modelCollection;
        private readonly Func<TVM, TM> modelProperty;
        private readonly string modelPropertyName;
        private readonly Func<TM, TVM> viewModelFactory;

        private void FullSync()
        {
            foreach (var vm in modelCollection.Select(viewModelFactory))
            {
                SubscribeToModelPropertyChanged(vm);
                viewModelCollection.Add(vm);
            }
        }

        private void CollectionChanged(object Sender, NotifyCollectionChangedEventArgs E)
        {
            switch (E.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItems = E.NewItems.Cast<TVM>();
                    foreach (var vm in newItems)
                        SubscribeToModelPropertyChanged(vm);
                    InsertRange(E.NewStartingIndex, newItems.Select(modelProperty));
                    break;
                case NotifyCollectionChangedAction.Move:
                    var l = PopRange(E.OldStartingIndex, E.OldItems.Count);
                    InsertRange(E.NewStartingIndex, l);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TVM vm in E.OldItems)
                        UnsubscribeToModelPropertyChanged(vm);
                    PopRange(E.OldStartingIndex, E.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (viewModelCollection.Count == 0)
                        modelCollection.Clear();
                    else
                        throw new NotImplementedException();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void InsertRange(int Index, IEnumerable<TM> Values)
        {
            var i = Index;
            foreach (var m in Values)
                modelCollection.Insert(i++, m);
        }

        private List<TM> PopRange(int Index, int Count)
        {
            var result = new List<TM>(Count);
            for (int i = 0; i < Count; i++)
            {
                result.Add(modelCollection[Index]);
                modelCollection.RemoveAt(Index);
            }
            return result;
        }

        private void SubscribeToModelPropertyChanged(TVM ViewModel)
        {
            UnsubscribeToModelPropertyChanged(ViewModel);
            ViewModel.PropertyChanged += ModelPropertyChanged;
        }

        private void UnsubscribeToModelPropertyChanged(TVM ViewModel) =>
            ViewModel.PropertyChanged -= ModelPropertyChanged;

        private void ModelPropertyChanged(object Sender, PropertyChangedEventArgs Args)
        {
            if (string.IsNullOrEmpty(Args.PropertyName)
                || Args.PropertyName == modelPropertyName)
            {
                var vm = (TVM)Sender;
                var newModel = modelProperty(vm);
                modelCollection[viewModelCollection.TakeWhile(vc => !vc.Equals(vm)).Count()] = newModel;
            };
        }
    }
}