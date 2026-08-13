// using System;
// using ProArchitecture.Logic;
// using ProArchitecture.Predicates;
// using NUnit.Framework;
// using ProSM;
// using ProTimers;
// using UnityEngine;
// using static ProArchitecture.Logic.ExampleLogic;
//
//
// public class ProSMIntegrationTestSuite : MonoBehaviour
// {
//
//     [Test]
//     public void Test1_TimedDirectTransition_Initalizes()
//     {
//         TimerStack.Reset();
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(2);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         fsm.InitLayer<TestLayerTwo, ExampleData>(1);
//         var data = new ExampleData() { x = 2 };
//         fsm.AddDirectTimedTransition(0, TestLayerOne.L1S1, TestLayerOne.L1S2, ref data, 1);
//         
//         Assert.IsTrue(fsm.layers[0].states[0].transitions.Active);
//         Assert.AreEqual(1, fsm.layers[0].states[0].transitions.currentSize);
//
//         fsm.Dispose();
//     }
//     
//     [Test]
//     public void Test2_TimedDirectTransition_Executes()
//     {
//         TimerStack.Reset();
//         TimerStackLogics.isTesting = true;
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var data = new ExampleData() { x = 1 };
//         fsm.Entry(ref data);
//
//         fsm.AddDirectTimedTransition(0, TestLayerOne.L1S1,TestLayerOne.L1S2, ref data, 1);
//         TimerStack.TickActivesDebug(0.5f);
//
//         Assert.IsFalse(fsm.layers[0].states[0].transitions[0].durationMet.active);
//         Assert.IsTrue(TimerStack.timers.GetWrapper(0).Active.active);
//         
//         TimerStack.TickActivesDebug(0.6f);
//         
//         Assert.IsTrue(fsm.layers[0].states[0].transitions[0].durationMet.active);
//         Assert.IsFalse(TimerStack.timers.GetWrapper(0).Active.active);
//         
//         fsm.Dispose();
//         TimerStackLogics.isTesting = false;
//     }
//     
//     // next test is timeddirecttransition executues, then CHANGES the state !
//
//     [Test]
//     public void Test3_TimedDirectTransition_Executes_Then_ChangesState()
//     {
//         TimerStack.Reset();
//         TimerStackLogics.isTesting = true;
//         ProSM<ExampleData> fsm = new ProSM<ExampleData>();
//
//         fsm.Initialize(1);
//         fsm.InitLayer<TestLayerOne, ExampleData>(0);
//         var data = new ExampleData() { x = 1 };
//         fsm.Entry(ref data);
//
//         fsm.AddDirectTimedTransition(0, TestLayerOne.L1S1,TestLayerOne.L1S2, ref data,1);
//         TimerStack.TickActivesDebug(0.5f);
//         
//         Assert.IsTrue(fsm.layers[0].currentState == (int)TestLayerOne.L1S1);
//         
//         TimerStack.TickActivesDebug(0.6f);
//         
//         
//         Assert.IsTrue(fsm.layers[0].currentState == (int)TestLayerOne.L1S2);
//         
//         fsm.Dispose();
//         TimerStackLogics.isTesting = false;
//     }
// }
