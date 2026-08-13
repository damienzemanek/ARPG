using System;
using System.Collections;
using EMILtools.Systems;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MonoFacadeTests
{
    [Serializable] public class TestBlackboard : Blackboard { }
    public class TestConfig : Config { }
    
    public interface ITestContextView : IViewableCtx
{
        // Readonly properties
        public float SomeInt { get; }
    }

    public class TestContextData : ContextData, ITestContextView
    {
        // Mutable state
        public float SomeInt { get; set; }
    }
    
    public class TestController : MonoFacade<
        TestFunctionality,
        TestConfig,
        TestStructure,
        TestController.ActionMap>
    {
        public class ActionMap : IActionMap
        {
            // Add Available Actions here as PersistentActions
            // Actions are separate from InputActions
            public IContextViewImmutable ctx { get; }
        }

        // If using InputAuthority, put InitializeFacade in InitSubordinate
        protected void Awake()
        {
            var config = ScriptableObject.CreateInstance<TestConfig>();
            InitializeFacade(config);
        }
    }
    
    public class TestFunctionality : Functionalities<
        TestController,
        TestContextData>
    {
        
        protected override IState AddModulesHere(TestController f)
        {
            return null;
        }

        protected override void SetupTransitionsForFSM(StateMachine<TestContextData> fsm, TestContextData ctx)
        {
            return;
        }
    }
    
    public class TestStructure : MonoStructure<
        TestBlackboard,
        TestContextData,
        ITestContextView>
    {

    }
    
    TestConfig config;
    TestController fcd;
    
    
    [SetUp]
    public void Setup()
    {
        config = ScriptableObject.CreateInstance<TestConfig>();
        fcd = new GameObject("TestFacade").AddComponent<TestController>();
    }
    
    
    
    //
    // [UnityTest]
    // public IEnumerator Test1_Initializes()
    // {
    //     
    //     yield return new WaitForSeconds(1);
    //     
    //     Assert.IsNotNull(fcd, "The FACADE was not created");
    //     Assert.IsNotNull(fcd.API_Config<TestConfig>(), "The CONFIG was not created");
    //     Assert.IsNotNull(fcd.API_Structure(), "The STRUCTURE was not created");
    //     Assert.IsNotNull(fcd.API_Blackboard<TestBlackboard>(), "The BLACKBOARD was not created");
    //     Assert.IsNotNull(fcd.API_Functionality<TestFunctionality>(), "The FUNCTIONALITY was not created");
    // }
    
    
    
    // // A Test behaves as an ordinary method
    // [Test]
    // public void MonoFacadeTestsSimplePasses()
    // {
    //     // Use the Assert class to test conditions
    // }
    //
    // // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // // `yield return null;` to skip a frame.
    // [UnityTest]
    // public IEnumerator MonoFacadeTestsWithEnumeratorPasses()
    // {
    //     // Use the Assert class to test conditions.
    //     // Use yield to skip a frame.
    //     yield return null;
    // }
}
