using System;
using System.Globalization;
using System.Linq;
using ConsulExec.View;
using NUnit.Framework;

namespace ConsulExec.Tests
{
    [TestFixture]
    public class ObjectToObjectConverterTests : AssertionHelper
    {
        [Test]
        public void ConvertEnumValuesGivenByStrings()
        {

            var target = new ObjectToObjectConverter();
            foreach (var value in Enum.GetValues(typeof(Enu)).Cast<Enu>())
                target.Mappings.Add(new MapValues { Source = value, Target = "Converted" + value });

            foreach (var enumValue in Enum.GetValues(typeof(Enu)).Cast<Enu>())
                Expect(target.Convert(enumValue, typeof(string), null, CultureInfo.CurrentCulture),
                    StartWith("Converted").And.EndsWith(enumValue.ToString()));
        }

        public enum Enu
        {
            V1, V2, V3
        }
    }
}