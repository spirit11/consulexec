using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace ConsulExec.View
{
    [ContentProperty(nameof(Mappings))]
    public class ObjectToObjectConverter : IValueConverter
    {
        public List<MapValues> Mappings { get; } = new List<MapValues>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = System.Convert.ToString(value);
            return Mappings.FirstOrDefault(
                m => m.Source == value
                     || stringValue == System.Convert.ToString(m.Source))?.Target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Mappings.FirstOrDefault();
        }
    }

    public class MapValues
    {
        public object Source { get; set; }
        public object Target { get; set; }
    }
}