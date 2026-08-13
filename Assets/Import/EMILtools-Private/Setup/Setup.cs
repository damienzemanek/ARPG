#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static System.IO.Path;
using static System.IO.Directory;

namespace EMILtools.Setup
{
    public static class Setup
    {

        [MenuItem("Tools/Setup/Create Default Folders")]
        public static void CreateDefaultFolders()
        {
            Folders.CreateDefault("GameFiles", 
                "Animation", "Art", "Materials", "Prefabs", 
                "ScriptableObjects", "Scripts", "Settings");

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Setup/Import Favorite Assets")]
        public static void ImportFavoriteAssets()
        {
            Assets.ImportAsset(packageName: "DOTween Pro.unitypackage", "Demigiant/Editor ExtensionsVisual Scripting");
            Assets.ImportAsset(packageName: "Odin Inspector 4.0.1.2.unitypackage", "Odin");
            Assets.ImportAsset(packageName: "Hot Reload Edit Code Without Compiling.unitypackage", "The Naughty Cult/Editor ExtensionsUtilities");
            Assets.ImportAsset(packageName: "Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem");

            AssetDatabase.Refresh();
        }


        static class Folders
        {
            public static void CreateDefault(string root, params string[] folders)
            {
                //Retrieve the path for the given root
                var fullpath = Combine(Application.dataPath, root);

                //Create the root if it doesnt already exist
                if(!Exists(fullpath)) CreateDirectory(fullpath);

                //Create all the new given folder names in the root, if they dont already exist
                foreach (var folder in folders)
                {
                    var path = Combine(fullpath, folder);
                
                    if(!Exists(path)) CreateDirectory(path);
                }

            }
        }

        static class Assets
        {
            public static void ImportAsset(string packageName, string insubfolder, string folder =
                "C:/Users/test2/AppData/Roaming/Unity/Asset Store-5.x")
                => AssetDatabase.ImportPackage(Combine(folder, insubfolder, packageName), false);

        }


    }
}
#endif
