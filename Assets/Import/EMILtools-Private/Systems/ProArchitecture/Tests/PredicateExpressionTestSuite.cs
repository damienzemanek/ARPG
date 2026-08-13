using NUnit.Framework;
using ProArchitecture.Predicates;
using UnityEngine;

public class PredicateExpressionTestSuite
{
    struct TestData
    {
        public int a;
        public int b;
    }

    static unsafe class TestPredicates
    {
        public static Predicate AIsZero() => new Predicate(&IsAZero);
        static bool IsAZero(void* ptr) => ((TestData*)ptr)->a == 0;

        public static Predicate BIsZero() => new Predicate(&IsBZero);
        static bool IsBZero(void* ptr) => ((TestData*)ptr)->b == 0;
        
        public static Predicate AIsOne() => new Predicate(&IsAOne);
        static bool IsAOne(void* ptr) => ((TestData*)ptr)->a == 1;

        public static Predicate AIsZeroVRef() => new Predicate(&IsAZeroVRef);
        static bool IsAZeroVRef(void* ptr) => ProArchitecture.Data.PtrEX.VPtrToRef<TestData>(ptr).a == 0;
    }

    [Test]
    public void Test1_RootOnly()
    {
        var data = new TestData { a = 0, b = 1 };
        var pA = TestPredicates.AIsZero();
        
        // Using AsExpr which creates a PredicateExpression
        PredicateExpression expr = pA;
        
        Assert.IsTrue(expr.Evaluate(ref data));
        
        data.a = 1;
        Assert.IsFalse(expr.Evaluate(ref data));
        
        expr.Dispose();
    }

    [Test]
    public void Test2_AndLogic()
    {
        var data = new TestData { a = 0, b = 0 };
        var pA = TestPredicates.AIsZero();
        var pB = TestPredicates.BIsZero();
        
        // pA AND pB
        var expr = pA.And(pB);
        
        Assert.IsTrue(expr.Evaluate(ref data), "0,0 should be true for A==0 AND B==0");
        
        data.a = 1;
        Assert.IsFalse(expr.Evaluate(ref data), "1,0 should be false for A==0 AND B==0");
        
        data.a = 0;
        data.b = 1;
        Assert.IsFalse(expr.Evaluate(ref data), "0,1 should be false for A==0 AND B==0");
        
        expr.Dispose();
    }

    [Test]
    public void Test3_OrLogic()
    {
        var data = new TestData { a = 1, b = 1 };
        var pA = TestPredicates.AIsZero();
        var pB = TestPredicates.BIsZero();
        
        // pA OR pB
        var expr = pA.Or(pB);
        
        Assert.IsFalse(expr.Evaluate(ref data), "1,1 should be false for A==0 OR B==0");
        
        data.a = 0;
        Assert.IsTrue(expr.Evaluate(ref data), "0,1 should be true for A==0 OR B==0");
        
        data.a = 1;
        data.b = 0;
        Assert.IsTrue(expr.Evaluate(ref data), "1,0 should be true for A==0 OR B==0");
        
        expr.Dispose();
    }

    [Test]
    public void Test4_DataResize()
    {
        // (A==0 AND B==0) OR A==1
        var data = new TestData { a = 0, b = 0 };
        var pA0 = TestPredicates.AIsZero();
        var pB0 = TestPredicates.BIsZero();
        var pA1 = TestPredicates.AIsOne();
        
        // pA0.And(pB0.StaticRef) creates expr with capacity 2.
        var expr = pA0.And(pB0);
        
        // Adding 3rd predicate triggers resize in the underlying Data structure
        expr.Or(pA1);
        
        // 0,0 -> (T AND T) OR F = T
        Assert.IsTrue(expr.Evaluate(ref data));
        
        // 0,1 -> (T AND F) OR F = F
        data.b = 1;
        Assert.IsFalse(expr.Evaluate(ref data));
        
        // 1,1 -> (F AND F) OR T = T
        data.a = 1;
        Assert.IsTrue(expr.Evaluate(ref data));
        
        expr.Dispose();
    }

    [Test]
    public void Test6_NoResize()
    {
        // Explicitly pre-allocating size 3
        var data = new TestData { a = 0, b = 0 };
        var pA0 = TestPredicates.AIsZero();
        var pB0 = TestPredicates.BIsZero();
        var pA1 = TestPredicates.AIsOne();

        // Create with capacity 3
        var expr = new PredicateExpression(3, pA0);
        expr.And(pB0);
        expr.Or(pA1);

        Assert.AreEqual(3, expr.NodeCount);
        
        // Verification of logic
        Assert.IsTrue(expr.Evaluate(ref data)); // (0==0 AND 0==0) OR 0==1 -> T
        
        data.a = 1;
        Assert.IsTrue(expr.Evaluate(ref data)); // (1==0 AND 0==0) OR 1==1 -> T
        
        expr.Dispose();
    }

    [Test]
    public void Test5_DisposeLifecycle()
    {
        var pA = TestPredicates.AIsZero();
        PredicateExpression expr = pA;
        var data = new TestData { a = 0 };
        
        Assert.IsTrue(expr.Evaluate(ref data));
        
        expr.Dispose();
        
        // After Dispose, Active is false, so Evaluate should return false and log an error
        //LogAssert.Expect(LogType.Error, "PredicateExpression.Evaluate: expression is not active, likely disposed");
        Assert.IsFalse(expr.Evaluate(ref data), "Should return false after Dispose");
    }

    [Test]
    public void Test7_PtrEXVRef()
    {
        var data = new TestData { a = 0, b = 1 };
        var pA = TestPredicates.AIsZeroVRef();
        PredicateExpression expr = pA;
        
        Assert.IsTrue(expr.Evaluate(ref data), "PtrEX.VRef should correctly access data.a == 0");
        
        data.a = 5;
        Assert.IsFalse(expr.Evaluate(ref data), "PtrEX.VRef should correctly detect data.a changed to 5");
        
        expr.Dispose();
    }
}
