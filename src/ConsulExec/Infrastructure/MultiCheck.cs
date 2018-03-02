using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MahApps.Metro.Controls;

namespace ConsulExec.Infrastructure
{
    public class MultiCheck : DependencyObject
    {
        #region Selector Property

        public static readonly DependencyProperty SelectorProperty =
            DependencyProperty.RegisterAttached(
            "Selector", typeof(Selector), typeof(MultiCheck), new PropertyMetadata(default(Selector)) { PropertyChangedCallback = SelectorChangedCallback });

        /// <summary>
        /// When set to Selector then a group of selected items in it is checked or unchecked simultaneously.
        /// </summary>
        public static void SetSelector(DependencyObject Element, Selector Value)
        {
            Element.SetValue(SelectorProperty, Value);
        }

        public static Selector GetSelector(DependencyObject Element)
        {
            return (Selector)Element.GetValue(SelectorProperty);
        }

        #endregion

        #region Apply property

        public static readonly DependencyProperty ApplyProperty =
            DependencyProperty.RegisterAttached(
            "Apply", typeof(bool), typeof(MultiCheck), new PropertyMetadata(default(bool)) { PropertyChangedCallback = ApplyChangedCallback });


        /// <summary>
        /// When set to True then a group of selected items in parent selector is checked or unchecked simultaneously.
        /// </summary>
        public static void SetApply(DependencyObject Element, bool Value)
        {
            Element.SetValue(ApplyProperty, Value);
        }

        public static bool GetApply(DependencyObject Element)
        {
            return (bool)Element.GetValue(ApplyProperty);
        }

        #endregion

        private static readonly Dictionary<Selector, HashSet<ToggleButton>> Groups = new Dictionary<Selector, HashSet<ToggleButton>>();
        private static bool PreventChecking;

        private static void ApplyChangedCallback(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var toggleButton = D as ToggleButton;
            var selector = D.TryFindParent<Selector>();

            if (E.NewValue.Equals(true) && toggleButton != null && selector != null)
                SetSelector(toggleButton, selector);
        }

        private static void SelectorChangedCallback(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var chk0 = D as ToggleButton;
            if (chk0 == null || E.NewValue == null)
                return;
            AddToGroup(chk0, (Selector)E.NewValue);
        }

        private static void AddToGroup(ToggleButton Button, Selector Selector)
        {
            var group = Groups.GetOrAdd(Selector, _ => new HashSet<ToggleButton>());
            group.Add(Button);

            Button.Checked += HandleCheck;
            Button.Unchecked += HandleCheck;
            RoutedEventHandler detach = null;
            detach = delegate
            {
                Button.Unloaded -= detach;
                group.Remove(Button);
            };
            Button.Unloaded += detach;
        }

        private static void HandleCheck(object Sender, RoutedEventArgs E)
        {
            var button = (ToggleButton)Sender;
            if (PreventChecking || !IsSelectedInList(button))
                return;
            var isChecked = button.IsChecked;
            PreventChecking = true;
            try
            {
                foreach (var toggleButton in Groups[GetSelector(button)].Where(IsSelectedInList))
                    toggleButton.IsChecked = isChecked;
            }
            finally
            {
                PreventChecking = false;
            }
        }

        private static bool IsSelectedInList(FrameworkElement Button)
        {
            var lbitem = Button.TryFindParent<ListBoxItem>();
            return lbitem?.IsSelected ?? false;
        }
    }
}
