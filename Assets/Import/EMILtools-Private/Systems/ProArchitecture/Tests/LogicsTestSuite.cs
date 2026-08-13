// using System;
// using NUnit.Framework;
// using ProArchitecture.Data;
// using ProArchitecture.Logic;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using UnityEngine;
// using static ProArchitecture.Logic.ExampleLogic;
//
// public class LogicTestSuite
// {
//     
//     [Test]
//     public void Test1_Initializes()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         var logic = Operation;
//         
//         Assert.IsNotNull(exampleData);
//         Assert.IsNotNull(logic);
//         Assert.AreEqual(2f, exampleData.x);
//     }
//     
//     [Test]
//     public void Test2_RunThroughSlotCall()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         var logic = Operation;
//         
//         logic.Run(ref exampleData);
//         
//         Assert.AreEqual(3f, exampleData.x);
//     }
//     
//     [Test]
//     public void Test3_MultiSlot_Execution()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//     
//         var instanceLogic = LogicBuilder<ExampleData>.Instance
//             .Add(ref Operation)
//             .Add(ref Operation)
//             .Build();
//
//         // run multi-slot (should run both handles: 1 + 1 + 1)
//         instanceLogic.TryRunAllSequentially(ref exampleData);
//
//         Assert.AreEqual(4f, exampleData.x);
//         
//         instanceLogic.Dispose();
//     }
//
//     
//     
//     static Logic<ExampleData> staticLogic = LogicBuilder<ExampleData>.Static.Add(ref Operation).Build();
//     [Test]
//     public void Test4_StaticLogic_Execution()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         
//         // Static builder uses persistent memory, typically initialized once.
//         
//         staticLogic.TryRunAllSequentially(ref exampleData);
//         Assert.AreEqual(3f, exampleData.x);
//     }
//
//     
//     
//     static LogicGroup<ExampleData> staticGroup = LogicGroupBuilder<ExampleData>.Static.Add(staticLogic).Build(out _);
//     [Test]
//     public void Test5_LogicGroup_Static_Execution()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         
//         staticGroup.RunAll(ref exampleData);
//         Assert.AreEqual(3f, exampleData.x);
//     }
//
//     
//     [Test]
//     public void Test6_LogicGroup_Dynamic_Lifecycle()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         
//         // Dynamic logics are "instanced" and own their Malloc'd memory
//         var dynamicLogic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         
//         // LogicGroupBuilder.Dynamic sets isDynamic = true
//         var instanceGroup = LogicGroupBuilder<ExampleData>.Instance.Add(dynamicLogic).Build(out _);
//         
//         instanceGroup.RunAll(ref exampleData);
//         Assert.AreEqual(3f, exampleData.x);
//         
//         // Dispose should free the dynamicLogic's ops array via the metadata check
//         instanceGroup.Dispose();
//         
//         // Subsequent access would be an access violation in real use, but here we just confirm it finished.
//     }
//
//     [Test]
//     public void Test7_ShouldRun_Predicate_Filtering()
//     {
//         var exampleData = new ExampleData() { x = 0.5f }; // ShouldRun checks x > 1
//         
//         var logic = LogicBuilder<ExampleData>.Static.Add(ref Operation).Build();
//         
//         logic.TryRunAllSequentially(ref exampleData);
//         
//         // Should not have incremented because x <= 1
//         Assert.AreEqual(0.5f, exampleData.x);
//         
//         exampleData.x = 2f;
//         logic.TryRunAllSequentially(ref exampleData);
//         
//         // Should have incremented now
//         Assert.AreEqual(3f, exampleData.x);
//     }
//
//     [Test]
//     public void Test8_LogicGroupDynamic_ActiveToggle()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         
//         var logic1 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         var logic2 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         
//         var group = new LogicGroup<ExampleData>(2, out _);
//         group.Add(logic1);
//         group.Add(logic2);
//         
//         // Both active: 2 + 1 + 1 = 4
//         group.RunAll(ref exampleData);
//         Assert.AreEqual(4f, exampleData.x);
//         
//         // Deactivate one slot via the internal Data collection
//         group.LogicsData.GetWrapper(0).Active.Set(false);
//         
//         exampleData.x = 2f;
//         group.RunAll(ref exampleData);
//         // Only one should run: 2 + 1 = 3
//         Assert.AreEqual(3f, exampleData.x);
//         
//         group.Dispose();
//     }
//
//     [Test]
//     public void Test9_Stackalloc_Span_Initialization()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//         
//         var logic1 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         var logic2 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         //
//         // // Testing the ReadOnlySpan constructor (simulating stackalloc/params)
//         // Span<(Ref<Logic<ExampleData>> logics, bool dynamic)> span = stackalloc (Ref<Logic<ExampleData>>, bool)[2];
//         // span[0] = (logic1);
//         // span[1] = (logic2);
//         //
//         // // Use the ReadOnlySpan constructor explicitly
//         // var group = new LogicGroup<ExampleData>((ReadOnlySpan<(Ref<Logic<ExampleData>>, bool)>)span, span.Length, out _);
//         //
//         // group.RunAll(ref exampleData);
//         // Assert.AreEqual(4f, exampleData.x);
//         //
//         // group.Dispose();
//     }
//
//     [Test]
//     public void Test10_StaticLogic_And_DynamicLogic_InSameLogicGroup()
//     {
//         var exampleData = new ExampleData() { x = 2f };
//
//         var instanceLogic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         Debug.Log("[Instanced] is Static: " + instanceLogic.RefInstance.GetRef.isStatic.active);
//         Debug.Log("[Static] is Static: " + staticLogic.isStatic.active);
//         var mixGroup = LogicGroupBuilder<ExampleData>.Instance.Add(instanceLogic).Add(staticLogic).Build(out _);
//         
//         mixGroup.RunAll(ref exampleData);
//         Assert.AreEqual(4f, exampleData.x);
//         
//         mixGroup.Dispose();
//     }
//     
//     
//     [Test]
//     public void Test_ADDITIONAL_EmptyLogic_DoesNotCrash()
//     {
//         var logic = LogicBuilder<ExampleData>.Instance.Build();
//         var data = new ExampleData { x = 2f };
//
//         logic.TryRunAllSequentially(ref data);
//
//         Assert.AreEqual(2f, data.x);
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_EmptyLogic_ToGroup()
//     {
//         var empty = Logic<ExampleData>.EmptyStatic;
//
//         var group = LogicGroupBuilder<ExampleData>.Instance.Add(empty).Build(out _);
//
//         var data = new ExampleData { x = 2f };
//         group.RunAll(ref data);
//
//         Assert.AreEqual(2f, data.x);
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_Builder_Reuse_DoesNotLeakState()
//     {
//         var l1 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         var l2 = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//
//         var data = new ExampleData { x = 2f };
//
//         l1.TryRunAllSequentially(ref data);
//         Assert.AreEqual(3f, data.x);
//
//         data.x = 2f;
//
//         l2.TryRunAllSequentially(ref data);
//         Assert.AreEqual(3f, data.x);
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_Pointer_Stability_After_GC()
//     {
//         var logic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//
//         GC.Collect();
//         GC.WaitForPendingFinalizers();
//
//         var data = new ExampleData { x = 2f };
//
//         logic.TryRunAllSequentially(ref data);
//
//         Assert.AreEqual(3f, data.x);
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_Dispose_InvalidatesLogic()
//     {
//         var dyn = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//
//         dyn.Dispose();
//
//         var data = new ExampleData { x = 2f };
//
//         // Should NOT crash even if ops was freed
//         Assert.DoesNotThrow(() =>
//         {
//             dyn.TryRunAllSequentially(ref data);
//         });
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_Group_With_UninitializedSlots()
//     {
//         var group = new LogicGroup<ExampleData>(2, out _);
//
//         var data = new ExampleData { x = 2f };
//
//         Assert.DoesNotThrow(() =>
//         {
//             group.RunAll(ref data);
//         });
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_Ref_Stability()
//     {
//         var logic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//
//         var r1 = logic.RefInstance;
//         var r2 = logic.RefInstance;
//
//         Assert.IsTrue(r1.Equals(r2));
//     }
//     
//     [Test]
//     public void Test_ADDITIONAL_MixedGroup_AfterDisposeOfInstanceLogicGroup()
//     {
//         var instanceLogic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//
//         var group = LogicGroupBuilder<ExampleData>.Instance
//             .Add(staticLogic)
//             .Add(staticLogic)
//             .Build(out _);
//
//         instanceLogic.Dispose();
//
//         var data = new ExampleData { x = 2f };
//
//         // depending on design, this should either:
//         // - safely ignore dead logic OR
//         // - throw predictable error
//
//         Assert.DoesNotThrow(() =>
//         {
//             group.RunAll(ref data);
//         });
//     }
//     
//     [Test]
//     public void Test_DynamicAddition_WithinCapacity()
//     {
//         var instanceLogic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         
//         // Create with capacity 2, add only 1
//         var group = LogicGroupBuilder<ExampleData>.Instance.Add(staticLogic).Build(out _);
//         
//         Assert.AreEqual(2, group.capacity);
//         Assert.AreEqual(1, group.currentSize);
//         
//         group.Add(instanceLogic);
//         Assert.AreEqual(2, group.currentSize);
//     }
//     
//     [Test]
//     public void Test_LogicGroup_AddCapacityExceeded_Throws()
//     {
//         var instanceLogic = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         
//         var group = LogicGroupBuilder<ExampleData>.Instance.Add(staticLogic).Build(out int remainingCapacitySpace);
//         var data = new ExampleData { x = 2f };
//         
//         
//         group.RunAll(ref data);
//         Assert.AreEqual(3f, data.x);
//         
//         // 1 -> 2 (UnsafeList capacity)
//         group.Add(instanceLogic);
//
//         Assert.AreEqual(0, remainingCapacitySpace);
//         // THIS should throw
//         Assert.Throws<InvalidOperationException>(() => 
//         {
//             group.Add(instanceLogic);
//         });
//         Debug.Log("group.currentSize: " + group.currentSize + " group.capacity: " + group.capacity);
//
//     }
//
//     [Test]
//     public void Test_LogicGroup_AdditionCapacity_Success()
//     {
//         
//         // Init with capacity 1, means UnsafeList goes to capacity 2
//         var group = LogicGroupBuilder<ExampleData>.Instance.Add(staticLogic).Build(out _);
//         
//         Assert.AreEqual(2, group.capacity, "capacity should be 2");
//         Assert.AreEqual(1, group.currentSize, "currentSize should be 1");
//         
//         // Should succeed, now current size = 2
//         Assert.DoesNotThrow(() => 
//         {
//             group.Add(staticLogic);
//         });
//         Assert.AreEqual(2, group.currentSize, "currentSize should be 2");
//
//         
//         var data = new ExampleData { x = 2f };
//         group.RunAll(ref data);
//         Assert.AreEqual(4f, data.x, "data.x should be 4 (2 + 1 + 1)");
//     }
//     [Test]
//     public void Test_AutoDisable_On_Dispose()
//     {
//         var data = new ExampleData { x = 2f };
//         var dyn = LogicBuilder<ExampleData>.Instance.Add(ref Operation).Build();
//         
//         var group = new LogicGroup<ExampleData>(1, out _);
//         group.Add(dyn);
//         
//         Assert.IsTrue(group.LogicsData.GetWrapper(0).Active);
//         
//         group.RunAll(ref data);
//         Assert.AreEqual(3f, data.x);
//         
//         // Dispose dynamic logic
//         dyn.Dispose();
//         
//         // Should have auto-disabled in group
//         Assert.IsFalse(group.LogicsData.GetWrapper(0).Active);
//         
//         data.x = 2f;
//         group.RunAll(ref data);
//         // Should NOT have run because it's inactive
//         Assert.AreEqual(2f, data.x);
//         
//         group.Dispose();
//     }
// }
//
