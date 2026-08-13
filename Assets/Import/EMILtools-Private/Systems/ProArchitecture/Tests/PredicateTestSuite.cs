using System;
using NUnit.Framework;
using ProArchitecture.Predicates;
using UnityEngine;

public class PredicateTestSuite
{
    struct ExampleData
    {
        public int value;
    }

    static unsafe class ExamplePredicate
    {   
        public static Predicate Create() => new Predicate(&IsZero);
        static bool IsZero(void* ptr)
        {
            ExampleData* data = (ExampleData*)ptr;
            return data->value == 0;
        }

        public static Predicate CreateNegated() => new Predicate(&IsZero, true);
    }

    [Test]
    public void Test1_Initalizes()
    {
        Predicate predicate = ExamplePredicate.Create();
        Assert.IsNotNull(predicate);
    }

    [Test]
    public void Test2_EvaluatesTrue()
    {
        ExampleData data = new ExampleData() { value = 0 };
        Predicate predicate = ExamplePredicate.Create();
        bool result = predicate.Evaluate(ref data);
        Assert.IsTrue(result);
    }

    [Test]
    public void Test3_EvaluatesFalse()
    {
        ExampleData data = new ExampleData() { value = 10 };
        Predicate predicate = ExamplePredicate.Create();
        bool result = predicate.Evaluate(ref data);
        Assert.IsFalse(result);
    }

    [Test]
    public void Test4_Negation()
    {
        ExampleData data = new ExampleData() { value = 0 };
        Predicate predicate = ExamplePredicate.CreateNegated();
        bool result = predicate.Evaluate(ref data);
        Assert.IsFalse(result, "Negated IsZero(0) should be false");

        data.value = 1;
        result = predicate.Evaluate(ref data);
        Assert.IsTrue(result, "Negated IsZero(1) should be true");
    }

    struct HealthData { public float hp; }
    static unsafe class HealthPredicate {
        public static Predicate Create() => new Predicate(&IsDead);
        static bool IsDead(void* ptr) => ((HealthData*)ptr)->hp <= 0;
    }

    [Test]
    public void Test5_MultipleDataTypes()
    {
        HealthData hData = new HealthData() { hp = -1f };
        Predicate hPredicate = HealthPredicate.Create();
        Assert.IsTrue(hPredicate.Evaluate(ref hData));
        
        hData.hp = 100f;
        Assert.IsFalse(hPredicate.Evaluate(ref hData));
    }

    [Test]
    public void Test6_UninitializedPredicateThrows()
    {
        Predicate emptyPredicate = default;
        var data = new ExampleData() { value = 0 };
        Assert.Throws<NullReferenceException>(() => emptyPredicate.Evaluate(ref data));
    }

    [Test]
    public void Test7_DataNotModified()
    {
        ExampleData data = new ExampleData() { value = 0 };
        Predicate predicate = ExamplePredicate.Create();
        predicate.Evaluate(ref data);
        Assert.AreEqual(0, data.value);
    }
}
