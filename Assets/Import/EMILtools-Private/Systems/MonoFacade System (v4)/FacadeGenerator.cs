#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EMILtools.Systems
{
    public class FacadeGenerator : EditorWindow
    {
        string facadeName = "";
        string path = "Assets/EMILtools/Facades/";
        bool generateInputSubordinate = false;

        [MenuItem("Tools/EMILTools/MonoFacade")]
        public static void ShowWindow() => GetWindow<FacadeGenerator>("EMILtools MonoFacade Generator");

        private void OnGUI()
        {
            facadeName = EditorGUILayout.TextField("MonoFacade Name", facadeName);
            path = EditorGUILayout.TextField("Path", path);

            GUILayout.Space(5);
            generateInputSubordinate = EditorGUILayout.Toggle("Generate Input Subordinate", generateInputSubordinate);

            if (GUILayout.Button("Generate MonoFacade Scripts"))
                GenerateScripts();
        }

        void GenerateScripts()
        {
            if (string.IsNullOrWhiteSpace(facadeName))
            {
                Debug.LogError("Facade Name cannot be empty.");
                return;
            }

            string createdPath = Path.Combine(path, facadeName);
            if (!Directory.Exists(createdPath))
                Directory.CreateDirectory(createdPath);

            GenerateCoreScripts(createdPath);

            if (generateInputSubordinate)
                GenerateInputAuthorityScripts(createdPath);

            AssetDatabase.Refresh();
            Debug.Log($"Generated MonoFacade: {facadeName} at {createdPath}");
        }

        void GenerateCoreScripts(string createdPath)
        {
            // Config
            CreateScript(createdPath, $"{facadeName}Config",
$@"using UnityEngine;
using EMILtools.Systems;

[CreateAssetMenu(fileName = ""{facadeName}Config"", menuName = ""EMILtools/ScriptableObjects/Configs/{facadeName}"")]
public class {facadeName}Config : Config
{{
}}");

            // Blackboard
            CreateScript(createdPath, $"{facadeName}Blackboard",
$@"using System;
using EMILtools.Systems;

[Serializable]
public class {facadeName}Blackboard : Blackboard
{{
}}");

            // Context
            CreateScript(createdPath, $"{facadeName}Context",
$@"using EMILtools.Systems;

public interface I{facadeName}ContextView : IContextViewImmutable
{{
    public float SomeInt {{ get; }}
}}

public class {facadeName}ContextData : ContextData, I{facadeName}ContextView
{{
    public float SomeInt {{ get; set; }}
}}");

            // Structure
            CreateScript(createdPath, $"{facadeName}Structure",
$@"using System;
using EMILtools.Systems;

[Serializable]
public class {facadeName}Structure : MonoStructure<
    {facadeName}Blackboard,
    {facadeName}ContextData,
    I{facadeName}ContextView>
{{
}}");

            // Functionality
            CreateScript(createdPath, $"{facadeName}Functionality",
$@"using EMILtools.Systems;

public class {facadeName}Functionality : Functionalities<
    {facadeName}Controller,
    I{facadeName}ContextView>
{{
    protected override IState AddModulesHere()
    {{
        // Add Modules like this
        // AddModule(new ExampleModule(...));

        // Return the module that is the starting state, ex:
        // return AddModule(new Idle(...))
        return null;
    }}

    protected override void SetupTransitionsForFSM(StateMachine<I{facadeName}ContextView> fsm, I{facadeName}ContextView ctx)
    {{
        // Add State Transitions here
        // fsm.AddAnyTransition<Jump>(new FuncPredicate(() => ctx.isJumping), ""Jumping"");
    }}
}}");

            if (!generateInputSubordinate)
                GenerateStandardController(createdPath);
        }

        void GenerateStandardController(string createdPath)
        {
            CreateScript(createdPath, $"{facadeName}Controller",
$@"using UnityEngine;
using EMILtools.Systems;

public class {facadeName}Controller : MonoFacade<
    {facadeName}Functionality,
    {facadeName}Config,
    {facadeName}Structure,
    {facadeName}Controller.ActionMap>
{{
    public class ActionMap : IActionMap
    {{
    }}

    protected void Awake()
    {{
        InitializeFacade();
    }}

    // If you change to an InputSubordinate switch these out with OnAuthorityReceived() and OnAuthorityLost()
    public void OnEnable() => Functionality.Bind();
    public void OnDisable() => Functionality.Unbind();

}}
}}");
        }

        void GenerateInputAuthorityScripts(string createdPath)
        {
            // Generate InputSubordinate Controller
            GenerateInputSubordinateController(createdPath);

            // Generate InputReader
            GenerateInputReader(createdPath);

            // Generate InputAuthority only if not present
            if (!InputAuthorityExists())
            {
                CreateScript(createdPath, $"PrimaryInputAuthority",
$@"using EMILtools.Systems;
using static {facadeName}Controller;

public class PrimaryInputAuthority : InputAuthority<
    {facadeName}InputReader,
    {facadeName}InputMap,
    PrimaryInputAuthority.Subordinates>
{{
    public enum Subordinates
    {{
        {facadeName}
    }}
}}");
            }
        }

        void GenerateInputSubordinateController(string createdPath)
        {
            CreateScript(createdPath, $"{facadeName}Controller",
$@"using EMILtools.Systems;
using UnityEngine;
using Sirenix.OdinInspector;
using static PrimaryInputAuthority;
using static EMILtools.Systems.IInputSubordinate<{facadeName}Controller.{facadeName}InputMap,PrimaryInputAuthority.Subordinates>;


public class {facadeName}Controller : MonoFacade<
    {facadeName}Functionality,
    {facadeName}Config,
    {facadeName}Structure,
    {facadeName}Controller.ActionMap>,
    IInputSubordinate<{facadeName}Controller.{facadeName}InputMap, Subordinates>
{{
    public class ActionMap : IActionMap
    {{
    }}

    public class {facadeName}InputMap : InputMap
    {{
        public readonly Publisher<(bool, Vector2)> Move = new();
    }}

    public {facadeName}InputMap Input {{ get; set; }}

    [field: SerializeField] [field: PropertyOrder(-1)]
    public SubordinateContext inputSubordinateContext {{ get; set; }}

    public {facadeName}InputMap InjectInputMap() => new {facadeName}InputMap();

    public void InitSubordinate()
    {{
        InitializeFacade();
    }}

    public void OnAuthorityReceived()
    {{
        Functionality.Bind();
    }}

    public void OnAuthorityLost()
    {{
        Functionality.Unbind();
    }}
}}");
        }

        void GenerateInputReader(string createdPath)
        {
            CreateScript(createdPath, $"{facadeName}InputReader",
$@"using EMILtools.Systems;
using UnityEngine.InputSystem;
using static PrimaryInputAuthority;
using static {facadeName}Controller;

public class {facadeName}InputReader :
    IPlayerActions,
    IInputReaderSubordinate<{facadeName}InputMap, Subordinates>
{{
    public IA_Player ia;

    public void Init()
    {{
        if(ia == null) ia = new IA_Player();
        ia.Player.Disable();
        ia.Player.SetCallbacks(this);
        ia.Player.Enable();
    }}

    public {facadeName}InputMap Input => subordinate.Input;
    public IInputSubordinate<{facadeName}InputMap, Subordinates> subordinate {{ get; set; }}
    public void OnAuthorityChange() {{ }}

    public void OnMove(InputAction.CallbackContext context)
    {{
        if(ia.Player.Move.IsPressed())
            Input.Move.Invoke((true, context.ReadValue<UnityEngine.Vector2>()));
        else
            Input.Move.Invoke((false, UnityEngine.Vector2.zero));
    }}
}}");
        }

        bool InputAuthorityExists()
        {
            // Scan loaded assemblies for InputAuthority-derived types
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Any(t => t.IsSubclassOf(typeof(InputAuthority<,,>)));
        }

        void CreateScript(string path, string fileName, string content)
            => File.WriteAllText(Path.Combine(path, fileName + ".cs"), content);
    }
}
#endif
#endif