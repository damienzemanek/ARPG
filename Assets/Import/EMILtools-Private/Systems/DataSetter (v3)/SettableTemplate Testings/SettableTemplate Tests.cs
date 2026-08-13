using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EMILtools.Systems;
using EMILtools.Core;

public class DataSetterTests
{
    // Test implementation of SettableTemplate
    class TestSettable : DataSetter<int>
    {
        public int setCallCount = 0;
        public int lastValue;

        protected override void LocalOnSet(int val)
        {
            setCallCount++;
            lastValue = val;
        }
    }

    // -----------------------------------------------------

    [Test]
    public void Test1_InitializesTest()
    {
        var settable = new TestSettable();

        Assert.NotNull(settable);
        Assert.NotNull(settable.Subscriber);
        Assert.NotNull(settable.OnSetEvent);
    }

    // -----------------------------------------------------

    [Test]
    public void Test2_SetStoresData()
    {
        var settable = new TestSettable();
        var publisher = new Publisher<int>();

        publisher.Add(settable.Subscriber);
        publisher.Publish(5).Forget("Test2_SetStoresData");

        Assert.AreEqual(5, settable.Get);
    }

    // -----------------------------------------------------

    [Test]
    public void Test3_SetCallsSetMethod()
    {
        var settable = new TestSettable();
        var publisher = new Publisher<int>();

        publisher.Add(settable.Subscriber);
        publisher.Publish(10).Wait();

        Assert.AreEqual(1, settable.setCallCount);
        Assert.AreEqual(10, settable.lastValue);
    }

    // -----------------------------------------------------

    [Test]
    public void Test4_OnSetInvoked()
    {
        var settable = new TestSettable();
        var publisher = new Publisher<int>();
        bool called = false;

        settable.OnSetEvent.Add(() => called = true);

        publisher.Add(settable.Subscriber);
        publisher.Publish(7).Wait();

        Assert.IsTrue(called);
    }

    // -----------------------------------------------------

    [Test]
    public async Task Test5_MultipleSetsUpdateValue()
    {
        var settable = new TestSettable();
        var publisher = new Publisher<int>();

        publisher.Add(settable.Subscriber);

        await publisher.Publish(3);
        await publisher.Publish(9);

        Assert.AreEqual(9, settable.Get);
        Assert.AreEqual(2, settable.setCallCount);
    }
}