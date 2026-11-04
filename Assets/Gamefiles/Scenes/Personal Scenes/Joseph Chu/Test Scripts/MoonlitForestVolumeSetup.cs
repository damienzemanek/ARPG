#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public static class MoonlitForestVolumeSetup
{
    [MenuItem("Tools/Moonlit Forest/Create Cinematic Global Volume")]
    public static void CreateVolume()
    {
        const string folder = "Assets/MoonlitForest";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "MoonlitForest");

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Moonlit_Forest_Profile";
        string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/Moonlit_Forest_Profile.asset");
        AssetDatabase.CreateAsset(profile, path);

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);

        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.15f);
        color.contrast.Override(22f);
        color.saturation.Override(-5f);
        color.colorFilter.Override(new Color(0.863f, 0.910f, 1f, 1f));

        var white = profile.Add<WhiteBalance>(true);
        white.temperature.Override(-12f);
        white.tint.Override(2f);

        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.0f);
        bloom.intensity.Override(0.55f);
        bloom.scatter.Override(0.75f);

        var vignette = profile.Add<Vignette>(true);
        vignette.color.Override(Color.black);
        vignette.intensity.Override(0.22f);
        vignette.smoothness.Override(0.45f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        var go = new GameObject("Moonlit_Forest_Global_Volume");
        Undo.RegisterCreatedObjectUndo(go, "Create Moonlit Forest Volume");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.sharedProfile = profile;
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
        Debug.Log("Moonlit Forest Global Volume created. Enable Post Processing on your camera and make sure its Volume Mask includes this object's layer.");
    }
}
#endif
