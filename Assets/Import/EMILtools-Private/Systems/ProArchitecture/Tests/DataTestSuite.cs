using System;
using NUnit.Framework;
using ProArchitecture.Data;
using Unity.Collections;
using static ProArchitecture.Logic.ExampleLogic;


public class DataTestSuite
{

    [Test]
    public void Test1_Initalizes()
    {
        int capacity = 10;
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(capacity, Allocator.Persistent, out _);
        Assert.IsNotNull(data);
        
        data.Dispose();
    }
    
    [Test]
    public void Test2_Allocates()
    {
        int capacity = 10;
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(capacity, Allocator.Persistent, out _);
        var exampleData = new ExampleData() { x = 1f };
        data.Allocate(ref exampleData, out int id);
        Assert.AreEqual(0, id);

        var allocatedData = data[id];
        Assert.IsNotNull(allocatedData);
        
        var dataInside = data[id];
        Assert.AreEqual(1f, dataInside.x);
        
        data.Dispose();
    }
    
    [Test]
    public void Test3_GetOutsideCapacity_Throws()
    {
        int capacity = 10;
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(capacity, Allocator.Persistent, out _);
        var exampleData = new ExampleData() { x = 1f };
        data.Allocate(ref exampleData, out int id);
        Assert.AreEqual(0, id);

        var allocatedData = data[id];
        Assert.IsNotNull(allocatedData);
        
        Assert.Throws<IndexOutOfRangeException>(() => { var dataInside = data[capacity]; });
        
        data.Dispose();
    }
    
    [Test]
    public void Test4_GetOutsideAllocatedButStillWithinCapacity_Throws()
    {
        int capacity = 10;
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(capacity, Allocator.Persistent, out _);
        var exampleData = new ExampleData() { x = 1f };
        data.Allocate(ref exampleData, out int id);
        Assert.AreEqual(0, id);

        var allocatedData = data[id];
        Assert.IsNotNull(allocatedData);
        
        Assert.Throws<IndexOutOfRangeException>(() => { var dataInside = data[1]; });
        
        data.Dispose();
    }
    
    [Test]
    public void Test5_ExpandsWhenFull()
    {
        int initialCapacity = 2;
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(initialCapacity, Allocator.Persistent, out _);
        var exampleData1 = new ExampleData() { x = 1f };
        var exampleData2 = new ExampleData() { x = 2f };
        var exampleData3 = new ExampleData() { x = 3f };
        data.Allocate(ref exampleData1, out _);
        data.Allocate(ref exampleData2, out _);;
        // This should trigger SetCapacity(4)
        data.Allocate(ref exampleData3, out int id); 
    
        Assert.AreEqual(2, id);
        Assert.AreEqual(3f, data[2].x);
        Assert.AreEqual(1f, data[0].x); // Verify old data is still there
    
        data.Dispose();
    }
    
    [Test]
    public void Test6_SupportsReferenceMutation()
    {
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(10, Allocator.Persistent, out _);
        var exampleData = new ExampleData() { x = 10f };
        data.Allocate(ref exampleData, out int id);
    
        // Mutate via ref
        ref var storedData = ref data[id];
        storedData.x = 20f;
    
        Assert.AreEqual(20f, data[id].x);
    
        data.Dispose();
    }
    
    [Test]
    public void Test8_MultipleAllocations_TracksSize()
    {
        Data<ExampleData, ExampleMetaData> data = new Data<ExampleData, ExampleMetaData>(1, Allocator.Persistent, out _);
        for(int i = 0; i < 100; i++)
        {
            var exampleData = new ExampleData() { x = (float)i };
            data.Allocate(ref exampleData, out _);
        }
        Assert.AreEqual(100, data.currentSize);
        Assert.AreEqual(99f, data[99].x);
        data.Dispose();
    }
}
