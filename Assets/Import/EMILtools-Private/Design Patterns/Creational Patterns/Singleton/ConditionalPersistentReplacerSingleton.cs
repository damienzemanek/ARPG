using UnityEngine;

namespace EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns
{
    public abstract class ConditionalPersistentReplacerSingleton<T> : MonoBehaviour
        where T : Component
    {
        public bool autoUnparentOnAwake = true;
        protected static T instance;
        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;
        public static T Instance
        {
            get
            {
                //Getter
                if (instance != null) return instance;

                //Try to find
                instance = FindAnyObjectByType<T>();
                if (instance != null) return instance;

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
        /// Determines if this instance should replace the current singleton.
        /// </summary>
        protected abstract bool ShouldReplace(T existingInstance);


        protected virtual void Awake()
        {
            InitializeSingleton();
        }


        void InitializeSingleton()
        {
            if (!Application.isPlaying) return;
            
            if (autoUnparentOnAwake) transform.SetParent(null);
            if (!HasInstance) { SetAsInstance(); return; }
            if (instance == this) return;
            
            if (ShouldReplace(instance))
            {
                Destroy(instance.gameObject);
                SetAsInstance();
            }
            else
                Destroy(gameObject);
        }


        private void SetAsInstance()
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}