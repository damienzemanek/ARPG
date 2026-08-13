using NUnit.Framework;
using ProArchitecture.Logic;


public class LogicOperationTestSuite
{
    /// <summary>
    /// This test has to pass because data is initialized and handle is assigned
    /// </summary>
    [Test]
    public void Test1_InitializeDataAndHandle()
    {
        var data = new ExampleLogic.ExampleData { x = 2f };
        var operation = ExampleLogic.Operation;

        Assert.AreEqual(2f, data.x);
        Assert.IsNotNull(operation);
    }

    /// <summary>
    /// This test has to pass because ShouldRun() returns true
    /// </summary>
    [Test]
    public void Test2_ShouldRun()
    {
        var data = new ExampleLogic.ExampleData { x = 2f };
        var operation = ExampleLogic.Operation;

        var shouldRun = operation.ShouldRun(in data);
        Assert.IsTrue(shouldRun);
    }

    /// <summary>
    /// This test has to pass because TryRun() increments data from 1 to 2
    /// </summary>
    [Test]
    public void Test3_Runs()
    {
        var data = new ExampleLogic.ExampleData { x = 2f };
        var operation = ExampleLogic.Operation;

        operation.Run(ref data);    

        Assert.AreEqual(3f, data.x);
    }

    /// <summary>
    /// This test has to pass because ShouldRun() returns true,
    /// so TryRun() is executed and increments data from 1 to 2
    /// </summary>
    [Test]
    public void Test4_ShouldRunPassesRun()
    {
        var data = new ExampleLogic.ExampleData { x = 2f };
        var operation = ExampleLogic.Operation;

        var shouldRun = operation.ShouldRun(in data);
        Assert.IsTrue(shouldRun);

        operation.Run(ref data);    

        Assert.AreEqual(3f, data.x);
    }

    /// <summary>
    /// This test has to pass because TryRun() should NOT execute,
    /// since ShouldRun() returns false when x == 0
    /// </summary>
    [Test]
    public void Test5_ShouldRunFailsRun()
    {
        var data = new ExampleLogic.ExampleData { x = 0f };
        var operation = ExampleLogic.Operation;

        var shouldRun = operation.ShouldRun(in data);

        if (shouldRun)
            operation.Run(ref data);

        Assert.AreEqual(0f, data.x);
    }
}