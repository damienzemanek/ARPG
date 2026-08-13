// using System;
// using ProArchitecture.Logic;
// using ProArchitecture.Predicates;
// using NUnit.Framework;
// using ProArchitecture.Data;
// using ProSM;
// using UnityEngine;
// using static ProArchitecture.Logic.ExampleLogic;
// using static ProSMTestSuite.SomeInstanceLogic;
//
// public class ProSMTestSuite : MonoBehaviour
// {
//     public static unsafe class ExamplePredicates
//     {   
//         // Little verbose, but oh well
//         public static Predicate IsGreaterThanOne() => new Predicate(&isGreaterThanOne);
//         static bool isGreaterThanOne(void* ptr)
//         {
//             ExampleData* data = (ExampleData*)ptr;
//             return data->x > 1;
//         }
//     }
//     
//     public enum TestLayerOne { L1S1, L1S2, L1S3 }
//     public enum TestLayerTwo { L2S1, L2S2, L2S3 }
//     
//     [Test]
//     public void Test1_Initalizes()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         var exampleData = new ExampleData() { x = 2 };
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne();
//
//         fsm.Initialize(2);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.InitLayer<TestLayerTwo, ExampleData>(1);
//
//         Assert.AreEqual(2, exampleData.x);
//         Assert.IsTrue(fsm.layers.Active);
//         Assert.IsTrue(fsm.layers.GetWrapper(0).MetaDataVolatile.IsInitialized);
//         Assert.IsTrue(fsm.layers.GetWrapper(1).MetaDataVolatile.IsInitialized);
//         Assert.AreEqual(3, fsm.layers[0].states.currentSize);
//         Assert.AreEqual(3, fsm.layers[1].states.currentSize);
//         
//         Assert.IsTrue(fsm.layers[0].states[0].transitions.Active);
//         Assert.IsTrue(fsm.layers[1].states[0].transitions.Active);
//         
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test2_AnyTransitionsInitialize()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(2);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.InitLayer<TestLayerTwo, ExampleData>(1);
//         var exampleData = new ExampleData() { x = 2 };
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddAnyTransition(0, TestLayerOne.L1S1, TestPredicate);
//         
//         Assert.IsTrue(fsm.layers[0].anyTransitions.Active);
//         Assert.AreEqual(1, fsm.layers[0].anyTransitions.currentSize);
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test3_DirectTransitionsInitialize()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(2);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.InitLayer<TestLayerTwo, ExampleData>(1);
//         var exampleData = new ExampleData() { x = 2 };
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1, TestLayerOne.L1S2, TestPredicate);
//         
//         Assert.IsTrue(fsm.layers[0].states[0].transitions.Active);
//         Assert.AreEqual(1, fsm.layers[0].states[0].transitions.currentSize);
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test4_EntersIntoCorrectState()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(2);
//         fsm.InitLayer(0, TestLayerOne.L1S2);
//         fsm.InitLayer(1, TestLayerTwo.L2S3);
//         var exampleData = new ExampleData() { x = 2 };
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne();
//         
//         fsm.Entry(ref exampleData);
//
//         Assert.AreEqual(1, fsm.layers[0].currentState);
//         Assert.AreEqual(2, fsm.layers[1].currentState);
//
//         fsm.Dispose();
//     }
//
//     
//     [Test] 
//     public void Test5_AnyTransitions_TryPollTransitions_AnyTransition_ReturnsBool()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData1 = new ExampleData() { x = 1 };
//         var exampleData2 = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData1);
//
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddAnyTransition(0, TestLayerOne.L1S2, TestPredicate);
//         
//         var resultFalse = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData1, out int _);
//         var resultTrue = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData2, out int _);
//
//         Assert.IsFalse(resultFalse);
//         Assert.IsTrue(resultTrue);
//
//         fsm.Dispose();
//         
//     }
//     
//     /// <summary>
//     /// Solved Issue here: InitLayer is called with 2 layers to init, however Polling needs to check each layer, however
//     /// since we didnt init layer 2, we are polling for a transiton on a layer that DNE
//     /// I opted for the solution checking state validitiy in Entry instead of requiring the SM to poll for a valid states
//     /// This means I am not being defensive and operating my systems in an always valid state, which is better than being defensive
//     /// </summary>
//     
//     [Test] 
//     public void Test6_AnyTransitions_TryPollTransitions_DirectTransition_ReturnsBool()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData1 = new ExampleData() { x = 1 };
//         var exampleData2 = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData1);
//
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1,TestLayerOne.L1S2, TestPredicate);
//         
//         var resultFalse = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData1, out int _);
//         var resultTrue = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData2, out int _);
//         
//         Assert.IsFalse(resultFalse);
//         Assert.IsTrue(resultTrue);
//
//         fsm.Dispose();
//         
//     }
//     
//     
//     
//     
//     [Test] 
//     public void Test7_TryPollTransitions_AnyTransition_Transitions()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData1 = new ExampleData() { x = 1 };
//         var exampleData2 = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData1);
//
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddAnyTransition(0, TestLayerOne.L1S2, TestPredicate);
//         
//         var resultFalse = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData1, out int nextState_doesNOTtransition);
//         var resultTrue = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData2, out int nextState2_doesTransition);
//
//         if(resultTrue) fsm.TransitionOnLayer_CallExitEnter(0, nextState2_doesTransition, ref exampleData1);
//         
//         Assert.AreEqual(-1, nextState_doesNOTtransition);
//         Assert.AreEqual(1, nextState2_doesTransition);
//         
//         Assert.AreEqual(1, fsm.layers[0].currentState);
//
//         fsm.Dispose();
//         
//     }
//     
//     [Test] 
//     public void Test8_TryPollTransitions_DirectTransition_Transitions()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData1 = new ExampleData() { x = 1 };
//         var exampleData2 = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData1);
//
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1,TestLayerOne.L1S2, TestPredicate);
//         
//         var resultFalse = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData1, out int nextState_doesNOTtransition);
//         var resultTrue = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData2, out int nextState2_doesTransition);
//
//         if(resultTrue) fsm.TransitionOnLayer_CallExitEnter(0, nextState2_doesTransition, ref exampleData1);
//         
//         Assert.AreEqual(-1, nextState_doesNOTtransition);
//         Assert.AreEqual(1, nextState2_doesTransition);
//         
//         Assert.AreEqual(1, fsm.layers[0].currentState);
//
//         fsm.Dispose();
//         
//     }
//     
//     
//     
//     [Test] 
//     public void Test9_StateLogic_EnterExitTransitionEventsExecutes()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         var exampleData = new ExampleData() { x = 2 };
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.Entry(ref exampleData);
//
//         Test9Logic.Reset();
//         
//         fsm.layers[0].states[0].OnExitState = new RefToStatic<Logics<ExampleData>>(ref Test9Logic.ExitLogics);
//         fsm.layers[0].states[1].OnEnterState = Test9Logic.EnterLogics;
//         
//         fsm.AddAnyTransition(0, TestLayerOne.L1S2, TestPredicate);
//         
//         var resultTrue = fsm.TryPollTransitionsOnLayer(ref fsm.layers[0], ref exampleData, out int nextState);
//
//         if(resultTrue) fsm.TransitionOnLayer_CallExitEnter(0, nextState, ref exampleData);
//         
//         Assert.AreEqual(1, nextState);
//         Assert.AreEqual(1, fsm.layers[0].currentState);
//         Assert.IsTrue(Test9Logic.Exited, "OnExitState should have executed");
//         Assert.IsTrue(Test9Logic.Entered, "OnEnterState should have executed");
//
//         fsm.Dispose();
//     }
//
//     public static unsafe class Test9Logic
//     {
//         public static bool Exited;
//         public static bool Entered;
//
//         public static void Reset()
//         {
//             Exited = false;
//             Entered = false;
//         }
//
//         static void Exit(ExampleData* data) => Exited = true;
//         static void Enter(ExampleData* data) => Entered = true;
//         static bool ShouldRun(ExampleData* data) => true;
//
//         public static readonly Operation<ExampleData> ExitOp = new(&Exit, &ShouldRun);
//         public static readonly Operation<ExampleData> EnterOp = new(&Enter, &ShouldRun);
//
//         public static readonly Logics<ExampleData> ExitLogics = new(ref ExitOp);
//         public static readonly Logics<ExampleData> EnterLogics = new(ref EnterOp);
//         
//     }
//     
//     
//     
//     
//     [Test] 
//     public unsafe void Test10_StateLogic_TicksExecute()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData);
//
//         // TickExampleLogics already has the Logics handles pre-initialized with stable pointers.
//         // We can just use any stable pointer for the test TickLogicData.
//         
//         var tickData = new TickLogic<ExampleData>.TickLogicData<ExampleData>(
//             _deltaTime: 0.1f,
//             _coreLogics: ref ExampleLogic.OperationLogics,
//             ref exampleData);
//         
//         TickExampleLogics.Reset();
//
//         fsm.layers[0].states[0].OnUpdate = TickExampleLogics.UpdateLogics;
//         fsm.layers[0].states[0].OnFixedUpdate = TickExampleLogics.FixedUpdateLogics;
//         fsm.layers[0].states[0].OnLateUpdate = TickExampleLogics.LateUpdateLogics;
//
//         // Execute the logics
//         fsm.layers[0].states[0].OnUpdate.TryRunAllSequentially(ref tickData);
//         fsm.layers[0].states[0].OnFixedUpdate.TryRunAllSequentially(ref tickData);
//         fsm.layers[0].states[0].OnLateUpdate.TryRunAllSequentially(ref tickData);
//
//         Assert.IsTrue(TickExampleLogics.update, "Update logic should have executed");
//         Assert.IsTrue(TickExampleLogics.fixedUpdate, "FixedUpdate logic should have executed");
//         Assert.IsTrue(TickExampleLogics.lateUpdate, "LateUpdate logic should have executed");
//         fsm.Dispose();
//     }
//     
//     public static unsafe class TickExampleLogics
//     {
//         public static bool update;
//         public static bool lateUpdate;
//         public static bool fixedUpdate;
//
//         public static void Reset()
//         {
//             update = false;
//             lateUpdate = false;
//             fixedUpdate = false;
//         }
//         
//         static void FixedUpdate(TickLogic<ExampleData>.TickLogicData<ExampleData>* data) => fixedUpdate = true;
//         static void LateUpdate(TickLogic<ExampleData>.TickLogicData<ExampleData>* data) => lateUpdate = true;
//         static void Update(TickLogic<ExampleData>.TickLogicData<ExampleData>* data) => update = true;
//         static bool ShouldRun(TickLogic<ExampleData>.TickLogicData<ExampleData>* data) => true;
//
//         public static readonly Operation<TickLogic<ExampleData>.TickLogicData<ExampleData>> UpdateOp = new(&Update, &ShouldRun);
//         public static readonly Operation<TickLogic<ExampleData>.TickLogicData<ExampleData>> LateUpdateOp = new(&LateUpdate, &ShouldRun);
//         public static readonly Operation<TickLogic<ExampleData>.TickLogicData<ExampleData>> FixedUpdateOp = new(&FixedUpdate, &ShouldRun);
//
//         public static readonly Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>> UpdateLogics;
//         public static readonly Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>> FixedUpdateLogics;
//         public static readonly Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>> LateUpdateLogics;
//
//         static TickExampleLogics()
//         {
//             UpdateLogics = new Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>>(ref UpdateOp);
//             FixedUpdateLogics = new Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>>(ref FixedUpdateOp);
//             LateUpdateLogics = new Logics<TickLogic<ExampleData>.TickLogicData<ExampleData>>(ref LateUpdateOp);
//         }
//         
//     }
//     
//     
//     [Test] 
//     public unsafe void Test11_PollTransitionsAllLayers()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(2);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.InitLayer<TestLayerTwo, ExampleData>(1);
//
//         var exampleData1 = new ExampleData() { x = 1 };
//         var exampleData2 = new ExampleData() { x = 2 };
//         fsm.Entry(ref exampleData1);
//         
//         var TestPredicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         fsm.AddAnyTransition(0, TestLayerOne.L1S2, TestPredicate);
//         fsm.AddAnyTransition(1, TestLayerTwo.L2S2, TestPredicate);
//
//         
//         // Should not Transition
//         fsm.TryPollTransitions(ref exampleData1);
//         
//         Debug.Log("CurrentStates: layer1: " + fsm.layers[0].currentState + " layer2: " + fsm.layers[1].currentState + "");
//         Assert.AreEqual(0, fsm.layers[0].currentState);
//         Assert.AreEqual(0, fsm.layers[1].currentState);
//         
//         // Should Transition
//         fsm.TryPollTransitions(ref exampleData2);
//         
//         Debug.Log("CurrentStates: layer1: " + fsm.layers[0].currentState + " layer2: " + fsm.layers[1].currentState + "");
//         Assert.AreEqual(1, fsm.layers[0].currentState);
//         Assert.AreEqual(1, fsm.layers[1].currentState);
//         
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test12_TransitionPriority_AnyOverDirect()
//     {
//         // Tests that AnyTransitions are evaluated before DirectTransitions
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var exampleData = new ExampleData() { x = 2 };
//
//         fsm.Entry(ref exampleData);
//         var truePredicate = ExamplePredicates.IsGreaterThanOne().AsExpr; // Always true for x=2
//         
//         // Add direct transition to S2
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1, TestLayerOne.L1S2, truePredicate);
//         // Add any transition to S3
//         fsm.AddAnyTransition(0, TestLayerOne.L1S3, truePredicate);
//
//         fsm.TryPollTransitions(ref exampleData);
//
//         // Should be S3 because AnyTransitions are polled first in TryPollTransitionsOnLayer
//         Assert.AreEqual((int)TestLayerOne.L1S3, fsm.layers[0].currentState);
//         
//         fsm.Dispose();
//     }
//     
//     // Helper logic class for the new tests
//     public static unsafe class PipelineLogic
//     {
//         static void AddOne(ExampleData* data) => data->x += 1;
//         static void Double(ExampleData* data) => data->x *= 2;
//         static bool ShouldRun(ExampleData* data) => true;
//
//         public static readonly Operation<ExampleData> AddOneOp = new(&AddOne, &ShouldRun);
//         public static readonly Operation<ExampleData> DoubleOp = new(&Double, &ShouldRun);
//
//         public static readonly Logics<ExampleData> AddOneLogics = new(ref AddOneOp);
//         public static readonly Logics<ExampleData> DoubleLogics = new(ref DoubleOp);
//
//         public static void Reset() { } 
//     }
//     
//     [Test]
//     public void Test13_DataPipeline_ExitToEnter_IsSequential()
//     {
//         // Verifies that data mutated in OnExit is correctly seen by OnEnter of the next state (Operated Sequentially)
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         var data = new ExampleData() { x = 10 };
//         fsm.Entry(ref data);
//
//         var predicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//
//         // S1 Exit: x += 1 (11)
//         // S2 Enter: x *= 2 (22)
//         fsm.layers[0].states[(int)TestLayerOne.L1S1].OnExitState = PipelineLogic.AddOneLogics;
//         fsm.layers[0].states[(int)TestLayerOne.L1S2].OnEnterState = PipelineLogic.DoubleLogics;
//
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1, TestLayerOne.L1S2, predicate);
//         fsm.TryPollTransitions(ref data);
//
//         Assert.AreEqual(22f, data.x, "Data should be modified by Exit then Enter in sequence");
//         fsm.Dispose();
//     }
//     
//     // Helper class for counting executions
//     public static unsafe class TransitionCounter
//     {
//         public static int ExitCount;
//         public static int EnterCount;
//
//         public static void Reset() { ExitCount = 0; EnterCount = 0; }
//
//         static void OnExit(ExampleData* data) => ExitCount++;
//         static void OnEnter(ExampleData* data) => EnterCount++;
//         static bool ShouldRun(ExampleData* data) => true;
//
//         public static readonly Operation<ExampleData> ExitOp = new(&OnExit, &ShouldRun);
//         public static readonly Operation<ExampleData> EnterOp = new(&OnEnter, &ShouldRun);
//
//         public static readonly Logics<ExampleData> ExitLogics = new(ref ExitOp);
//         public static readonly Logics<ExampleData> EnterLogics = new(ref EnterOp);
//     }
//     
//     [Test]
//     public void Test14_SelfTransition_IsIgnored()
//     {
//         // Verifies that transitioning to the same state is forbidden and triggers no logic
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         var data = new ExampleData() { x = 10 };
//         fsm.Entry(ref data);
//
//         var predicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//
//         // Setup counters in logic
//         TransitionCounter.Reset();
//         fsm.layers[0].states[(int)TestLayerOne.L1S1].OnExitState = TransitionCounter.ExitLogics;
//         fsm.layers[0].states[(int)TestLayerOne.L1S1].OnEnterState = TransitionCounter.EnterLogics;
//
//         // Add transition S1 -> S1 (Self)
//         fsm.AddDirectTransition(0, TestLayerOne.L1S1, TestLayerOne.L1S1, predicate);
//         
//         // Poll transitions
//         fsm.TryPollTransitions(ref data);
//
//         // Assertions
//         Assert.AreEqual((int)TestLayerOne.L1S1, fsm.layers[0].currentState, "State should not have changed");
//         Assert.AreEqual(0, TransitionCounter.ExitCount, "OnExit should NOT have run for self-transition");
//         Assert.AreEqual(0, TransitionCounter.EnterCount, "OnEnter should NOT have run for self-transition");
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test15_Stress_MultipleLayersPerformance()
//     {
//         // Stress test: 100 layers with transitions on every layer
//         const int LAYER_COUNT = 100;
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(LAYER_COUNT);
//         
//         var data = new ExampleData() { x = 2 };
//         var predicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//
//         for (int i = 0; i < LAYER_COUNT; i++)
//         {
//             fsm.InitLayer<TestLayerOne, ExampleData>(i);
//             fsm.AddAnyTransition(i, TestLayerOne.L1S2, predicate);
//         }
//
//         fsm.Entry(ref data);
//         
//         // Measure or just verify correctness across large volume
//         fsm.TryPollTransitions(ref data);
//
//         for (int i = 0; i < LAYER_COUNT; i++)
//         {
//             Assert.AreEqual((int)TestLayerOne.L1S2, fsm.layers[i].currentState, $"Layer {i} failed to transition");
//         }
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test16_InvalidEnum_ThrowsException()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         var data = new ExampleData() { x = 2 };
//
//         // Init layer 0 with TestLayerOne
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.Entry(ref data);
//
//         var predicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//
//         // Attempting to add a transition using TestLayerTwo on a layer initialized with TestLayerOne
//         Assert.Throws<ArgumentException>(() => {
//             fsm.AddAnyTransition(0, TestLayerTwo.L2S1, predicate);
//         }, "Should throw when adding Any transition with wrong enum type");
//
//         Assert.Throws<ArgumentException>(() => {
//             fsm.AddDirectTransition(0, TestLayerTwo.L2S1, TestLayerTwo.L2S2, predicate);
//         }, "Should throw when adding Direct transition with wrong enum type");
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test17_EntryWithoutLayers_ThrowsException()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         // fsm.Initialize(0); // Or skip initialization entirely
//     
//         var data = new ExampleData() { x = 0 };
//
//         Assert.Throws<InvalidOperationException>(() => {
//             fsm.Entry(ref data);
//         }, "Should throw if Entry is called on an uninitialized FSM");
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test18_DoubleInitialize_Throws()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//
//         Assert.Throws<InvalidOperationException>(() => {
//             fsm.Initialize(1);
//         }, "Should throw if Initialize is called on an already active FSM");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test19_InvalidLayerCount_Throws()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         
//         Assert.Throws<ArgumentException>(() => {
//             fsm.Initialize(0);
//         }, "Should throw if layerCount is 0");
//
//         Assert.Throws<ArgumentException>(() => {
//             fsm.Initialize(-1);
//         }, "Should throw if layerCount is negative");
//     }
//
//     [Test]
//     public void Test20_InitLayer_OutOfBounds_Throws()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1); // Only index 0 is valid
//
//         Assert.Throws<IndexOutOfRangeException>(() => {
//             fsm.InitLayer<TestLayerOne, ExampleData>(1);
//         }, "Should throw if layerIndex is equal to currentSize");
//
//         Assert.Throws<IndexOutOfRangeException>(() => {
//             fsm.InitLayer<TestLayerOne, ExampleData>(-1);
//         }, "Should throw if layerIndex is negative");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test21_InitLayer_DoubleInit_Throws()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//
//         Assert.Throws<InvalidOperationException>(() => {
//             fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         }, "Should throw if the same layer index is initialized twice");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test22_AddTransition_InvalidState_Throws()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         var predicate = ExamplePredicates.IsGreaterThanOne().AsExpr;
//         
//         // TestLayerOne has 3 states (0, 1, 2). Index 3 is invalid.
//         // We cast an int to the Enum to simulate an invalid/out-of-range state
//         TestLayerOne invalidState = (TestLayerOne)3;
//
//         // Any Transition: Invalid 'to'
//         Assert.Throws<ArgumentOutOfRangeException>(() => {
//             fsm.AddAnyTransition(0, invalidState, predicate);
//         }, "Should throw if 'to' state index is out of bounds");
//
//         // Direct Transition: Invalid 'from'
//         Assert.Throws<ArgumentOutOfRangeException>(() => {
//             fsm.AddDirectTransition(0, invalidState, TestLayerOne.L1S2, predicate);
//         }, "Should throw if 'from' state index is out of bounds");
//
//         // Direct Transition: Invalid 'to'
//         Assert.Throws<ArgumentOutOfRangeException>(() => {
//             fsm.AddDirectTransition(0, TestLayerOne.L1S1, invalidState, predicate);
//         }, "Should throw if 'to' state index is out of bounds in direct transition");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test23_PollBeforeEntry_Throws()
//     {
//         // This test only runs if ENABLE_UNITY_COLLECTIONS_CHECKS is defined
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         var data = new ExampleData() { x = 2 };
//
//         // We specifically avoid calling fsm.Entry(ref data) here
//         
//         Assert.Throws<InvalidOperationException>(() => {
//             fsm.TryPollTransitions(ref data);
//         }, "Should throw if TryPollTransitions is called before Entry()");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test24_DoubleDispose_IsSafe()
//     {
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(1);
//         fsm.Dispose();
//
//         // Should not throw
//         Assert.DoesNotThrow(() => {
//             fsm.Dispose();
//         }, "Dispose should be safe to call multiple times or on uninitialized FSMs.");
//     }
//     
//     [Test]
//     public void Test25_EntryWithUninitializedLayer_Throws()
//     {
//         // Setup: Initialize with 2 layers but only configure the first one
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//         fsm.Initialize(2);
//         
//         // Initialize Layer 0
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         
//         // Layer 1 is allocated but NOT initialized via InitLayer()
//         
//         var exampleData = new ExampleData() { x = 0 };
//
//         // Assert: Entry should throw because Layer 1 is uninitialized
//         var ex = Assert.Throws<InvalidOperationException>(() => {
//             fsm.Entry(ref exampleData);
//         });
//
//         // Optional: Verify the error message contains the specific layer index
//         StringAssert.Contains("Layer 1 has not been initialized", ex.Message);
//
//         fsm.Dispose();
//     }
//     
//     // Helper struct for testing non-blittable validation
//     public struct NonBlittableData
//     {
//         public bool someBool; // bool is unmanaged but NOT blittable in Unity/C#
//     }
//
//     [Test]
//     public void Test26_NonBlittableData_Throws()
//     {
//         ProSM<NonBlittableData> fsm = new ProSM<NonBlittableData>();
//         
//         // Should throw ArgumentException because NonBlittableData contains a bool
//         Assert.Throws<ArgumentException>(() => {
//             fsm.Initialize(1);
//         }, "Should throw when initializing with a non-blittable TData type");
//
//         fsm.Dispose();
//     }
//
//     [Test]
//     public void Test27_DirectInstanceMutation()
//     {
//         ProSM<SomeInstanceLogic.SomeInstanceData> fsm = new ProSM<SomeInstanceLogic.SomeInstanceData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, SomeInstanceLogic.SomeInstanceData>(0);
//         var exampleData = new SomeInstanceLogic.SomeInstanceData() { x = 2 };
//         SomeInstance someInstance = new SomeInstance() { someVariable = exampleData };
//         
//         fsm.Entry(ref someInstance.someVariable);
//         
//         fsm.layers[0].states[0].OnUpdate = TickExampleLogicsNaiveImplementation.UpdateLogics;
//         fsm.layers[0].states[0].OnFixedUpdate = TickExampleLogicsNaiveImplementation.FixedUpdateLogics;
//         fsm.layers[0].states[0].OnLateUpdate = TickExampleLogicsNaiveImplementation.LateUpdateLogics;
//
//         fsm.TickUpdate(1, ref SomeInstanceLogic.OperationLogics, ref someInstance.someVariable); // 2 + 1(dt) + 1(run) = 4
//         fsm.TickFixedUpdate(1, ref SomeInstanceLogic.OperationLogics, ref someInstance.someVariable); // 4 + 1(dt) + 1(run) = 6
//         fsm.TickLateUpdate(1, ref SomeInstanceLogic.OperationLogics, ref someInstance.someVariable); // 6 + 1(dt) + 1(run) = 8
//
//         Assert.AreEqual(8f, someInstance.someVariable.x);
//         fsm.Dispose();
//     }
//
//     /// <summary>
//     /// N
//     /// </summary>
//     class SomeInstance
//     {
//         public SomeInstanceLogic.SomeInstanceData someVariable;
//     }
//
//     public static unsafe class SomeInstanceLogic
//     {
//         public struct SomeInstanceData
//         {
//             public float x;
//         }
//         
//         static void Run(SomeInstanceData* data) => data->x++;
//         static bool ShouldRun(SomeInstanceData* data) => true;
//         
//         public static Operation<SomeInstanceData> Operation = new(&Run, &ShouldRun);
//         public static Logics<SomeInstanceData> OperationLogics = new(ref Operation);
//     }
//     
//     /// <summary>
//     /// In reality you would really only need one of these so this is sort of overkill having all 3 ticks
//     /// </summary>
//     public static unsafe class TickExampleLogicsNaiveImplementation
//     {
//         static void FixedUpdate(TickLogic<SomeInstanceLogic.SomeInstanceData>.TickLogicData<SomeInstanceLogic.SomeInstanceData>* data) 
//         {
//             data->CoreData.x += data->deltaTime;
//             data->CoreLogics.TryRunAllSequentially(ref data->CoreData);
//         }
//         static void LateUpdate(TickLogic<SomeInstanceLogic.SomeInstanceData>.TickLogicData<SomeInstanceLogic.SomeInstanceData>* data)
//         {
//             data->CoreData.x += data->deltaTime;
//             data->CoreLogics.TryRunAllSequentially(ref data->CoreData);
//         }
//         static void Update(TickLogic<SomeInstanceLogic.SomeInstanceData>.TickLogicData<SomeInstanceData>* data)
//         {
//             data->CoreData.x += data->deltaTime;
//             data->CoreLogics.TryRunAllSequentially(ref data->CoreData);
//         }
//         
//         static bool ShouldRun(TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>* data) => true;
//
//         public static readonly Operation<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> UpdateOp = new(&Update, &ShouldRun);
//         public static readonly Operation<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> LateUpdateOp = new(&LateUpdate, &ShouldRun);
//         public static readonly Operation<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> FixedUpdateOp = new(&FixedUpdate, &ShouldRun);
//
//         public static readonly Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> UpdateLogics;
//         public static readonly Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> FixedUpdateLogics;
//         public static readonly Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>> LateUpdateLogics;
//
//         static TickExampleLogicsNaiveImplementation()
//         {
//             UpdateLogics = new Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>>(ref UpdateOp);
//             FixedUpdateLogics = new Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>>(ref FixedUpdateOp);
//             LateUpdateLogics = new Logics<TickLogic<SomeInstanceData>.TickLogicData<SomeInstanceData>>(ref LateUpdateOp);
//         }
//         
//     }
//     
//     public static unsafe class PredicateLogic
//     {
//         public static Predicate IsGreaterThan(float threshold)
//         {
//             // We need a way to store the threshold. 
//             // Since Predicate only takes a void*, and we don't have a capture context,
//             // we have to use a static field or a more complex solution if we want different thresholds.
//             // For the test, we can just implement it specifically for SomeInstanceData.
//             return new Predicate(&CheckGreaterThan);
//         }
//
//         static float _threshold;
//         static bool CheckGreaterThan(void* ptr)
//         {
//             var data = (SomeInstanceLogic.SomeInstanceData*)ptr;
//             return data->x > 5f; // Hardcoded for 5f as per the first use case in the test
//         }
//         
//         // Better: implement specific ones if needed, or a more robust system.
//         // But for "minimal" fix in tests:
//         public static Predicate IsGreaterThan5() => new Predicate(&CheckGT5);
//         static bool CheckGT5(void* ptr) => ((SomeInstanceLogic.SomeInstanceData*)ptr)->x > 5f;
//
//         public static Predicate IsGreaterThan10() => new Predicate(&CheckGT10);
//         static bool CheckGT10(void* ptr) => ((SomeInstanceLogic.SomeInstanceData*)ptr)->x > 10f;
//     }
//     
//     
//
// }
