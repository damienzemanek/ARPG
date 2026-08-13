#if UNITY_EDITOR    
using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using EMILtools.Core;
using EMILtools.Timers;
using EMILtools.Systems;
using UnityEngine.TestTools;
using static EMILtools.Systems.PipelineExecutor<PipelineTests.TestCtx>;

public class PipelineTests
{
    // Simple context for testing
    public class TestCtx : IViewableCtx
    {
        public int Value;
        public TestCtx(int _value = 0) => Value = _value;
    }

    
    [Test]
    public void Test1_PipelineBuilder_CreatesCorrectSize()
    {
        // Arrange
        var builder = new PipelineBuilder<TestCtx>();
        
        // Act
        builder.Add_ShortCircuit(new FuncPredicate(() => true));
        var pipeline = builder.InjectMainMethod(MainMethod);

        void MainMethod(TestCtx ctx) { }

        // Assert
        Assert.AreEqual(2, pipeline.Size);
    }

    
    [Test]
    public async Task Test2_StepsAllPass_FinalExecutes()
    {
        // Arrange
        var myctx = new TestCtx(3);
        bool jumpSuccessfull = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2))
            .InjectMainMethod(MainMethod);
        Debug.Log("------- Setup Complete -------");

        
        // Act
        await TryTo(jump, myctx);
        Debug.Log("------- Act Complete -------");

        
        //Assert
        Assert.AreEqual(true, jumpSuccessfull);
        Debug.Log("------- Assert Complete -------");

        void MainMethod(TestCtx ctx) 
        {
            Debug.Log("Main Method Being Called");
            jumpSuccessfull = true;
        }
    }
    
    [Test]
    public async Task Test3_StepFail_FinalDoesNotExecute()
    {
        // Arrange
        var myctx = new TestCtx(2);
        bool jumpSuccessfull = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2))
            .InjectMainMethod(Jump);

        //Act
        await TryTo(jump, myctx);
        
        // Assert
        Assert.AreEqual(jumpSuccessfull, false);
        
        void Jump(TestCtx ctx) 
        {
            Debug.Log("Main Method Being Called");
            jumpSuccessfull = true;
        }
    }
    
    [Test]
    public async Task Test4_StepFail_Executes_ShortCircuitCallback()
    {
        // Arrange
        var myctx = new TestCtx(2);
        var failedStepCallbackExecuted = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2),
                shortCircuited: new IResolvable[] { new Callback(ShortCircuitCallback) })
            .InjectMainMethod(Jump);
        void Jump(TestCtx ctx) { }
        void ShortCircuitCallback() => failedStepCallbackExecuted = true;

        
        // Act
        await TryTo(jump, myctx);
        
        
        // Assert
        Assert.IsTrue(failedStepCallbackExecuted, "Short circuit callback should have been called.");
    }

    
    [Test]
    public async Task Test5_BeforeAndAfter_AllPass_ResolveContextCalled()
    {
        // Arrange
        var myctx = new TestCtx(3);
        bool jumpCalled = false;
        bool beforecalled = false;
        bool aftercalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2),
                before: new IResolvable[]{ new Callback(CallBefore) }, 
                after: new IResolvable[]{ new Callback(CallAfter) })
            .InjectMainMethod(Jump);
        
        // Act
        await TryTo(jump, myctx);
        void Jump(TestCtx ctx) { jumpCalled = true; }
        void CallBefore() => beforecalled = true;
        void CallAfter() => aftercalled = true;
        
        // Assert
        Assert.IsTrue(beforecalled, "Before callback should have been called.");
        Assert.IsTrue(aftercalled, "After callback should have been called.");
        Assert.IsTrue(jumpCalled, "Jump should have been called.");

    }
    
    [Test]
    public async Task Test6_BeforeAndAfter_BeforePassesOnly_DueToShortCircuit_ResolveContextCalled()
    {
        // Arrange
        var myctx = new TestCtx(2);
        var jumpCalled = false;
        var beforecalled = false;
        var aftercalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2),
                before: new IResolvable[]{ new Callback(CallBefore) }, 
                after: new IResolvable[]{ new Callback(CallAfter) })
            .InjectMainMethod(Jump);
        void CallBefore() => beforecalled = true;
        void CallAfter() => aftercalled = true;
        void Jump(TestCtx ctx) { jumpCalled = true; }
        
        // Act
        await TryTo(jump, myctx);
        
        
        // Assert
        Assert.IsTrue(beforecalled, "Before callback should have been called.");
        Assert.IsFalse(aftercalled, "After callback should NOT have been called.");
        Assert.IsFalse(jumpCalled, "Jump should NOT have been called.");
    }

    
    
    
        
    [UnityTest]
    public IEnumerator Test7_TimedStepBlocking()
    {
        // Arrange
        var myctx = new TestCtx(3);
        var jumpCalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2),
                before: new IResolvable[] { new TimedGate(1, false, out var resetHandle) })
            .InjectMainMethod(Jump);
        void Jump(TestCtx ctx) { jumpCalled = true; }

        
        // Act
        TryTo(jump, myctx).Forget("Jump");
        
        // Assert
        Assert.AreEqual(jumpCalled, false, "Jump should not be called immediately.");
            // Manually tick 900ms (0.9s)
        TimerUtility.TickAllFixed(0.9f);
        TryTo(jump, myctx);
        Assert.AreEqual(jumpCalled, false, "Jump should not be called after 900ms.");
            // Manually tick another 200ms (Total 1.1s)
        TimerUtility.TickAllFixed(0.2f);
        TryTo(jump, myctx);
        Assert.AreEqual(jumpCalled, true, "Jump should be called after total 1100ms.");
    
        yield return null;

    }
    
    [UnityTest]
    public IEnumerator Test8_WaitingStepBlockingAndEventuallyCompletes()
    {
        // Arrange
        var myctx = new TestCtx(2);
        bool jumpCalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 0))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1),
                before: new IResolvable[] { new Wait(1) })
            .InjectMainMethod(Jump);
        void Jump(TestCtx ctx) { jumpCalled = true; }

        
        // Act
        var task = TryTo(jump, myctx);
        
        // Assert
        Assert.AreEqual(jumpCalled, false, "Jump should not be called immediately.");
        TimerUtility.TickAllFixed(1.2f);
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.AreEqual(jumpCalled, true, "Jump should be called after total 1100ms.");
        yield return null;
    }

    
        
    [Test]
    public async Task Test9_ShortCircuitCallbackCalled()
    {
        // Arrange
        var myctx = new TestCtx(2);
        bool jumpCalled = false;
        bool beforecalled = false;
        bool aftercalled = false;
        bool shortCircuitCalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 2),
                before: new IResolvable[]{ new Callback(CallBefore) }, 
                after: new IResolvable[]{ new Callback(CallAfter) },
                shortCircuited: new IResolvable[]{ new Callback(ShortCircuit) })
            .InjectMainMethod(Jump);
        void Jump(TestCtx ctx) { jumpCalled = true; }
        void CallBefore() => beforecalled = true;
        void CallAfter() => aftercalled = true;
        void ShortCircuit() => shortCircuitCalled = true;
        
        // Act
        await TryTo(jump, myctx);
        
        // Assert
        Assert.IsTrue(beforecalled, "Before callback should have been called.");
        Assert.IsTrue(shortCircuitCalled, "Short circuit callback should have been called.");
        Assert.IsFalse(aftercalled, "After callback should NOT have been called.");
        Assert.IsFalse(jumpCalled, "Jump should NOT have been called.");

    }
    
    [Test]
    public async Task Test10_ShortCirtcuit()
    {
        // Arrange
        var myctx = new TestCtx(2);
        var shortCircuitChecked = false;
        var shortCircuitSucceeded = false;
        var jumpCalled = false;
        var jump = new PipelineBuilder<TestCtx>()
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx => ctx.Value == 1))
            .Add_ShortCircuit(new FuncCtxPredicate<TestCtx>(ctx =>
                {
                    shortCircuitChecked = true;
                    return ctx.Value == 2;
                }), 
                shortCircuited: new IResolvable[] { new Callback(ShortCircuits) }) 
            .InjectMainMethod(Jump);
        void ShortCircuits() => shortCircuitSucceeded = true;
        void Jump(TestCtx ctx) { jumpCalled = true; }
        
        // Act
        await TryTo(jump, myctx);
        
        
        // Assert
        Assert.IsTrue(shortCircuitChecked, "Before callback should have been called.");
        Assert.IsTrue(shortCircuitSucceeded, "After callback should NOT have been called.");
        Assert.IsFalse(jumpCalled, "Jump should NOT have been called.");
    }
    
    
}
#endif