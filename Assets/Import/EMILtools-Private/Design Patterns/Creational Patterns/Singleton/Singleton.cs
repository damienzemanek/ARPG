using UnityEngine;
using System;
using Unity.VisualScripting;

namespace DesignPatterns {
namespace CreationalPatterns{

        public class Singleton<T> : MonoBehaviour where T: Component 
        {
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

            void InitializeSingleton()
            {
                if (!Application.isPlaying) return;

                instance = this as T;
            }
        }


}}