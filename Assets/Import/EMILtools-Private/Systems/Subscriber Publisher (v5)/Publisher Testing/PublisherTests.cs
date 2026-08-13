using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using EMILtools.Systems;
using NUnit.Framework;
using EMILtools.Systems;
using EMILtools.Timers;

public class PublisherTests
{
    [Test]
    public void Test1_Initializes()
    {
        var sub = new SubResolvable(Call);
        var publisher = new Publisher();

        bool Call() => true;
        Assert.IsNotNull(sub);
        Assert.IsNotNull(publisher);
    }
    
    [Test]
    public async Task Test2_CanPublish()
    {
        var called = false;
        var sub = new SubResolvable(Call);
        var publisher = new Publisher();
        publisher.Add(sub);
        await publisher.Publish();
        
        bool Call() => called = true;
        Assert.IsTrue(called);
    }
    
        [Test]
    public async Task Test3_InactiveSubscriberNotCalled()
    {
        bool called = false;
        bool Call() => called = true;

        var sub = new SubResolvable(Call, isActive: false);
        var publisher = new Publisher();
        publisher.Add(sub);

        await publisher.Publish();

        Assert.IsFalse(called);
    }

    [Test]
    public async Task Test4_MultipleSubscribers()
    {
        bool called1 = false;
        bool called2 = false;
        bool Call1() => called1 = true;
        bool Call2() => called2 = true;

        var sub1 = new SubResolvable(Call1);
        var sub2 = new SubResolvable(Call2);

        var publisher = new Publisher();
        publisher.Add(sub1);
        publisher.Add(sub2);

        await publisher.Publish();

        Assert.IsTrue(called1 && called2);
    }

    [Test]
    public async Task Test5_PublishWithResolveContainerBeforeExecution()
    {
        bool beforeResolved = false;
        bool mainCalled = false;

        var before = new Callback(() => beforeResolved = true);
        var container = new Resolves(true, beforeExe: new IResolvable[] { before });

        bool MainCall() => mainCalled = true;

        var sub = new SubResolvable(MainCall, container);
        var publisher = new Publisher();
        publisher.Add(sub);

        await publisher.Publish();

        Assert.IsTrue(beforeResolved);
        Assert.IsTrue(mainCalled);
    }
    
    
    public class MyContext : IContext
    {
        public int value = 0;
    }
    
    [Test]
    public async Task Test6_CustomContextSubscriberExecutes()
    {
        var ctx = new MyContext();
        bool wasCalled = false;
        bool actionCalled = false;
        bool action2Called = false;

        bool Call(MyContext c) => wasCalled = true;
        void ActionCall() => actionCalled = true;
        void Action2Call(MyContext c) => action2Called = true;
        var sub = new SubResolvableCtx<MyContext>(
            Call,
            new Resolves(true,
                beforeExe: new IResolvable[] { new Callback(ActionCall) },
                afterExe: new IResolvable[] { new CallbackCtx<MyContext>(Action2Call) })
            
        );

        var publisher = new Publisher<MyContext>();
        publisher.Add(sub);

        await publisher.Publish(ctx);

        Assert.IsTrue(wasCalled);
        Assert.IsTrue(actionCalled);
        Assert.IsTrue(action2Called);
    }

    [Test]
    public async Task Test7_CustomContextMultipleSubscribers()
    {
        var ctx = new MyContext();
        bool oneCalled = false;
        bool twoCalled = false;
        bool Call1(MyContext c) => oneCalled = true;
        bool Call2(MyContext c) => twoCalled = true;

        var sub1 = new SubResolvableCtx<MyContext>(Call1);
        var sub2 = new SubResolvableCtx<MyContext>(Call2);

        var publisher = new Publisher<MyContext>();
        publisher.Add(sub1);
        publisher.Add(sub2);

        await publisher.Publish(ctx);

        Assert.IsTrue(oneCalled);
        Assert.IsTrue(twoCalled);
    }
    
    
    [Test]
    public async Task Test8_PredicateShortCircuit()
    {
        bool mainCalled = false;
        Func<bool> predicate = () => true; // returns true = short circuit
        void MainCall() => mainCalled = true;

        var sub = new SubResolvable(predicate, canShortCircuit: true);
        var publisher = new Publisher();
        publisher.Add(sub);

        await publisher.Publish();

        // The main subscriber callback (not predicate) should not run because predicate short-circuits
        Assert.IsFalse(mainCalled);
    }

    [Test]
    public async Task Test9_PredicateNoShortCircuit()
    {
        bool mainCalled = false;
        Func<bool> predicate = () => false; // returns false = do not short circuit
        bool MainCall() => mainCalled = true;

        var sub = new SubResolvable(predicate, canShortCircuit: true);
        var publisher = new Publisher();
        publisher.Add(sub);

        // Execute a second subscriber after predicate to simulate normal execution
        var mainSub = new SubResolvable(MainCall);
        publisher.Add(mainSub);

        await publisher.Publish();

        // Since predicate returned false, the second subscriber should run
        Assert.IsTrue(mainCalled);
    }
    
    [Test]
    public async Task Test10_AfterResolveDoesNotRunOnShortCircuit_ManualTick()
    {
        bool afterCalled = false;

        bool ShortCircuitPredicate() => false; // triggers short circuit
        void After() => afterCalled = true;

        var container = new Resolves(true,
            afterExe: new IResolvable[] { new Callback(After) }
        );

        var sub = new SubResolvable(ShortCircuitPredicate, container, canShortCircuit: true);
        var publisher = new Publisher();
        publisher.Add(sub);

        var task = publisher.Publish();

        // Manual tick (simulate a frame, though After should NOT run)
        TimerUtility.TickAllFixed(1f);
        await task;

        Assert.IsFalse(afterCalled, "AfterExecution should NOT run when short-circuited.");
    }


    [Test]
    public async Task Test11_BeforeResolveRunsEvenIfShortCircuited_ManualTick()
    {
        bool beforeCalled = false;

        void Before() => beforeCalled = true;
        bool Predicate() => true; // short circuit

        var container = new Resolves(true,
            beforeExe: new IResolvable[] { new Callback(Before) }
        );

        var sub = new SubResolvable(Predicate, container, canShortCircuit: true);
        var publisher = new Publisher();
        publisher.Add(sub);

        var task = publisher.Publish();

        // Manual tick (simulate passage of time / timer execution)
        TimerUtility.TickAllFixed(1f);
        await task;

        Assert.IsTrue(beforeCalled, "BeforeExecution should run even if short-circuited.");
    }

    [Test]
    public async Task Test12_WaitResolverBlocksExecutionUntilFinished_ManualTick()
    {
        bool mainCalled = false;

        var wait = new Wait(0.1f); // 100ms

        bool Main()
        {
            mainCalled = true;
            return false;
        }

        var container = new Resolves(true,
            beforeExe: new IResolvable[] { wait }
        );

        var sub = new SubResolvable(Main, container);
        var publisher = new Publisher();
        publisher.Add(sub);

        var publishTask = publisher.Publish();

        // Immediately after publish, main should not have executed yet
        Assert.IsFalse(mainCalled, "Main should not run immediately due to Wait resolver.");

        // Manually tick the timer 90ms -> main should still not run
        TimerUtility.TickAllFixed(0.09f);
        Assert.IsFalse(mainCalled, "Main should not run before Wait finishes.");

        // Tick another 20ms (total 110ms)
        TimerUtility.TickAllFixed(0.02f);
        await publishTask;

        Assert.IsTrue(mainCalled, "Main should run after Wait resolver finishes.");
    }

    [Test]
    public async Task Test13_SubscriberCanAddSubscriberDuringPublish_ManualTick()
    {
        bool firstCalled = false;
        bool secondCalled = false;

        var publisher = new Publisher();

        bool First()
        {
            firstCalled = true;

            publisher.Add(new SubResolvable(() =>
            {
                secondCalled = true;
                return false;
            }));

            return false;
        }

        publisher.Add(new SubResolvable(First));

        var task1 = publisher.Publish();

        // Manual tick to simulate frame/update
        TimerUtility.TickAllFixed(0.1f);
        await task1;

        Assert.IsTrue(firstCalled, "First subscriber should have been called.");
        Assert.IsFalse(secondCalled, "Second subscriber should NOT run in the same publish cycle.");

        var task2 = publisher.Publish();
        TimerUtility.TickAllFixed(0.1f);
        await task2;

        Assert.IsTrue(secondCalled, "Second subscriber should run after next publish cycle.");
    }
}
