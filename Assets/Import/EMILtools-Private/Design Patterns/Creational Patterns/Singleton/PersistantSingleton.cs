using UnityEngine;

namespace EMILtools.Design_Patterns.Creational_Patterns {
namespace CreationalPatterns{
    //Destroy any NEW singletons that are made
        public class PersistantReplacerSingleton<T> : MonoBehaviour where T: Component 
        {
            public bool autoUnparentOnAwake = true;
            protected static T instance;
            public static bool HasInstance => (instance != null);
            public static T TryGetInstance() => HasInstance ? instance : null;

            public static T Instance
            {
                get
                {
                    //Getter
                    if(instance != null) return instance;
                    
                    //Try to find
                    instance = FindAnyObjectByType<T>();
                    if(instance != null) return instance;

                    //Auo generate
                    instance = AutoGenerateInstance();

                    return instance;
                }
            }

            protected static T AutoGenerateInstance()
            {
                GameObject go = new GameObject(typeof(T).Name + " Auto-Generated");
                return go.AddComponent<T>();
            }

            /// <summary>
            /// Ensure you call base.Awake()
            /// </summary>
            protected virtual void Awake() => InitializeSingleton();

            private void InitializeSingleton()
            {
                if (!Application.isPlaying) return;

                if (autoUnparentOnAwake) transform.SetParent(null);

                if(!HasInstance)
                {
                    instance = this as T;
                    DontDestroyOnLoad(gameObject);
                }
                else if (instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }


}}