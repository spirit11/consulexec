using System.Collections.Generic;
using System.Linq;
using ConsulExec.Infrastructure;
using NUnit.Framework;

namespace ConsulExec.Tests.Infrastructure
{
    [TestFixture]
    public class DictionaryExtensionsTests : AssertionHelper
    {
        [SetUp]
        public void SetUp()
        {
            ctorCalled = 0;
            targetDictionary = new[] { 1, 2, 3 }.ToDictionary(v => v, v => v.ToString());
        }

        [Test]
        public void AbsentValueIsCreated()
        {
            targetDictionary.GetOrAdd(0, Ctor);

            Expect(ctorCalled, Is.EqualTo(1));
            Expect(targetDictionary, Contains(new KeyValuePair<int, string>(0, "0")));
        }

        [Test]
        public void ExistingValueIsNotCreated()
        {
            targetDictionary.GetOrAdd(1, Ctor);
            Expect(ctorCalled, Is.EqualTo(0));
        }

        private int ctorCalled;
        private Dictionary<int, string> targetDictionary;

        private string Ctor(int Value)
        {
            ctorCalled++;
            return Value.ToString();
        }
    }
}
