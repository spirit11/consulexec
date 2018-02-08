using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ConsulExec.Infrastructure
{
    public class MultiCheck : DependencyObject
    {
        public static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
            "Group", typeof(string), typeof(MultiCheck), new PropertyMetadata(default(string)) { PropertyChangedCallback = SetGroupChangedCallback });

        public static void SetGroup(DependencyObject element, string value)
        {
            element.SetValue(GroupProperty, value);
        }

        public static string GetGroup(DependencyObject element)
        {
            return (string)element.GetValue(GroupProperty);
        }

        private static readonly Dictionary<string, List<ToggleButton>> groups = new Dictionary<string, List<ToggleButton>>();

        private static void SetGroupChangedCallback(DependencyObject D, DependencyPropertyChangedEventArgs E)
        {
            var chk0 = D as ToggleButton;
            if (chk0 == null)
                return;
            var group = groups.GetOrAdd((string)E.NewValue, _ => new List<ToggleButton>());
            group.Add(chk0);
            chk0.Checked += HandleCheck;
            chk0.Unchecked += HandleCheck;
            chk0.Unloaded += delegate { group.Remove(chk0); };
        }

        private static bool PreventChecking;

        private static void HandleCheck(object Sender, RoutedEventArgs E)
        {
            if (PreventChecking)
                return;
            var button = (ToggleButton)Sender;
            var isChecked = button.IsChecked;
            PreventChecking = true;
            try
            {
                foreach (var toggleButton in groups[GetGroup(button)].Where(IsSelectedInList))
                    toggleButton.IsChecked = isChecked;
            }
            finally
            {
                PreventChecking = false;
            }
        }

        private static bool IsSelectedInList(FrameworkElement Button)
        {
            var lbitem = Button;
            while (lbitem != null && !(lbitem is ListBoxItem))
                lbitem = lbitem.TemplatedParent as FrameworkElement;
            return (lbitem as ListBoxItem)?.IsSelected ?? false;
        }
    }
}
