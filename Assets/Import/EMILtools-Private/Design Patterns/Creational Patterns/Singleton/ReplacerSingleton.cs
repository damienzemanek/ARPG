using UnityEngine;
using System;
using EMILtools.Extensions;
using Unity.VisualScripting;
using Extensions;

namespace DesignPatterns {
namespace CreationalPatterns{

        //Destroy any OLD singletons that are present
        public class NewestSingleton<T> : MonoBehaviour where T: Component 
        {
            protected static T instance;
            public static bool HasInstance => (instance != null);

            public float InitializationTime { get; private set; }

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
                GameObject go = new GameObject(typeof(T).Name + " Replacement Auto-Generated");
                return go.AddComponent<T>();
            }

            /// <summary>
            /// Ensure you call base.Awake()
            /// </summary>
            protected virtual void Awake() => InitializeSingleton();

            private void InitializeSingleton()
            {
                if (!Application.isPlaying) return;
                InitializationTime = Time.time;
                DontDestroyOnLoad(gameObject);

                //Find and destroy all old singletons (initialization time is LESS than the new initialization time)
                T[] oldInstances = FindObjectsByType<T>(FindObjectsSortMode.None);
                foreach(T old in oldInstances)
                {
                    if (old.Get<NewestSingleton<T>>().InitializationTime < InitializationTime)
                        Destroy(old.gameObject);
                }

                if (!HasInstance) instance = this as T;


            }
        }


}}