using System;
using System.Threading.Tasks;
using EMILtools.Systems;
using NUnit.Framework;
using UnityEngine;

public class SubsctiberTests : MonoBehaviour
{

    public class TestFailResolvable : IResolvable
    {
        public bool wasCalled;
        public bool consumed { get; }
        public void ResetWait()
        {
            // No op
        }

        public Func<bool> Resolve { get; }

        public TestFailResolvable()
        {
            Resolve = () =>
            {
                wasCalled = true;
                return false;
            };
        }

    }
    public class TestResolvable : IResolvable
    {
        public bool wasCalled;
        public bool consumed { get; }
        public void ResetWait()
        {
            // No op
        }

        public Func<bool> Resolve { get; }

        public TestResolvable(bool resolveResult = true)
        {
            Resolve = () =>
            {
                wasCalled = true;
                return resolveResult;
            };
        }
    }
    
    [Test]
    public async Task Test1_ActionSubscriberExecutes()
    {
        bool executed = false;

        var sub = new SubResolvable(
            () => executed = true,
            new Resolves(true)
        );

        await sub.Execute();

        Assert.IsTrue(executed);
    }

    [Test]
    public async Task Test2_BeforeResolversExecute()
    {
        var before = new TestResolvable();

        var container = new Resolves(true,
            beforeExe: new IResolvable[] { before }
        );

        var sub = new SubResolvable(
            () => false,
            container
        );

        await sub.Execute();

        Assert.IsTrue(before.wasCalled);
    }
    
    [Test]
    public async Task Test3_AfterResolversExecute()
    {
        var after = new TestResolvable();

        var container = new Resolves(true,
            afterExe: new IResolvable[] { after }
        );

        var sub = new SubResolvable(
            () => false,
            container
        );

        await sub.Execute();

        Assert.IsTrue(after.wasCalled);
    }
    
    [Test]
    public async Task Test4_ShortCircuitStopsExecution()
    {
        bool executed = false;

        var beforeFail = new TestFailResolvable();

        var container = new Resolves(true,
            beforeExe: new IResolvable[] { beforeFail }
        );

        var sub = new SubResolvable(
            () => executed = true,
            container,
            canShortCircuit: true
        );

        await sub.Execute();

        Assert.IsFalse(executed);
        Assert.IsTrue(beforeFail.wasCalled);
    }
    
    [Test]
    public async Task Test5_PredicateShortCircuitTriggersFailedExecution()
    {
        bool failedResolved = false;

        var failed = new TestResolvable();
        bool DoesContinue = false;

        var container = new Resolves(true,
            failExe: new IResolvable[] { failed }
        );

        var sub = new SubResolvable(
            () => DoesContinue,
            container,
            canShortCircuit: true
        );

        await sub.Execute();

        failedResolved = failed.wasCalled;

        Assert.IsTrue(failedResolved, "Failed execution should have been called.");
    }
    
    [Test]
    public async Task Test6_BeforeFailureStopsAllExecution()
    {
        var failPointBefore = new TestFailResolvable();
        var after = new TestResolvable();

        var container = new Resolves(true,
            beforeExe: new IResolvable[] { failPointBefore },
            afterExe: new IResolvable[] { after }
        );

        var sub = new SubResolvable(
            () => false,
            container,
            canShortCircuit: true
        );

        await sub.Execute();

        Assert.IsTrue(failPointBefore.wasCalled);
        Assert.IsFalse(after.wasCalled);
    }
        

}
