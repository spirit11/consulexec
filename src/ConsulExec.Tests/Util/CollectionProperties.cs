using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace ConsulExec.Tests.Util
{
    public class CollectionProperties
    {
        public static Constraint AreEqual<TI, TO>(IEnumerable<TI> Sequence, Func<TI, TO> PropertySelector) =>
            new PropConstraint<TI, TO>(Sequence, PropertySelector);
    }


    internal class PropConstraint<TI, TO> : Constraint
    {
        public PropConstraint(IEnumerable<TI> Sequence, Func<TI, TO> Selector)
        {
            sequence = Sequence.Select(Selector);
            selector = Selector;
        }

        public override string Description => new EqualConstraint(sequence).Description;

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var enumerable = actual as IEnumerable<TI>;
            if (enumerable != null)
            {
                return new ConstraintResult(this, enumerable.Select(selector),
                    enumerable.Select(selector).SequenceEqual(sequence) ?
                    ConstraintStatus.Success : ConstraintStatus.Failure);
            }
            return new ConstraintResult(this, actual, ConstraintStatus.Failure);
        }

        private readonly IEnumerable<TO> sequence;
        private readonly Func<TI, TO> selector;
    }


    [TestFixture]
    public class PropConstraintTests : AssertionHelper
    {
        class TestClass
        {
            public int Prop { get; set; }
        }

        [Test]
        public void CorrectApplyToEvaluation()
        {
            var seq1 = new[] { 1, 2, 3 }.Select(v => new TestClass { Prop = v }).ToArray();
            var seq2 = new[] { 1, 2, 3 }.Select(v => new TestClass { Prop = v }).ToArray();
            var seq3 = new[] { 1, 2, 4 }.Select(v => new TestClass { Prop = v }).ToArray();
            var cons = CollectionProperties.AreEqual(seq1, v => v.Prop);
            Expect(cons.ApplyTo(seq2).IsSuccess, True);
            Expect(cons.ApplyTo(seq3).IsSuccess, False);
        }

        [Test]
        public void CorrectApplyToEvaluation2()
        {
            var seq1 = new[] { 1, 2, 3 }.Select(v => new TestClass { Prop = v }).ToArray();
            var seq3 = new[] { 1, 2, 4 }.Select(v => new TestClass { Prop = v }).ToArray();
            var cons = CollectionProperties.AreEqual(seq1, v => v.Prop);
            Expect(() => Expect(seq3, cons), Throws.Exception);
        }
    }
}
