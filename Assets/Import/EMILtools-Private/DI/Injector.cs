using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using DesignPatterns.CreationalPatterns;
using EMILtools.Extensions;

namespace DependencyInjection
{
    //Purpose:
    // - Any Method or Field marked with the [Inject] Attribute we expect that dependency to be satisfied
    //   by our system
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute { }



    //Purpose:
    // - Any method marked with the [Provide] Attribute we expect to supply an instance of a dependancy
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute { }


    //Marker Interface
    public interface IDependencyProvider { }

    //Purpose:
    //
    // Have only 1 Injector
    public class Injector : Singleton<Injector> 
    {
        //Looking for Instaance, public, non public : Methods and Fields
        const BindingFlags k_bindingFlags = BindingFlags.Instance |
                                            BindingFlags.Public |
                                            BindingFlags.NonPublic;

        //Find all the dependancies and store them in the dictionary by type
        readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();
        public List<MonoBehaviour> injectables = new List<MonoBehaviour>();

        protected override void Awake()
        {
            base.Awake();

            // Providing Dependancies
            // 1 Find all modules implementing IDependancyProvider
            IEnumerable<IDependencyProvider> providers = FindMonoBehaviors().OfType<IDependencyProvider>();

            // 2 Register each found IDependencyProvider into our dictionary
            foreach (var provider in providers)
            {
                print("DI: (1) Regitering a provider: " + provider);
                RegisterProvider(provider);
            }
            Debug.Log($"DI: Registered all Providers");


            // Field Injection
            //  1 Find all Injectable Objects and inject their dependancies

            //      a Grabs a list of Monobehaviors where they have injectable members
            injectables = FindMonoBehaviors().Where(IsInjectable).ToList();
            foreach(var injectable in injectables)
            {
                //Inject at each injectable monobehaviour
                Inject(injectable);
            }
            Debug.Log($"DI: Injected all Fields");
        }

        void Inject(object injectable)
        {
            var type = injectable.GetType();

            Debug.Log($"DI: Injecting into {type.Name} containing injectable fields");

            var injectableFields = type.GetFields(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach(var field in injectableFields)
            {
                var fieldType = field.FieldType;
                Debug.Log($"DI: Field Injecting into Injectable Field {fieldType.Name} into {type.Name}");

                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                    throw new Exception($"Failed to resolve {fieldType.Name} into {type.Name}");

                field.SetValue(injectable, resolvedInstance);
                Debug.Log($"DI: Field Injected {fieldType.Name} into {type.Name}");
            }

            var injectableMethods = type.GetMethods(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach(var injectableMethod in injectableMethods)
            {
                Debug.Log($"DI: Method Injecting into Injectable Field {injectableMethod.Name} into {type.Name}");

                var requiredParameters = injectableMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();

                var resolvedInstances = requiredParameters.Select(Resolve).ToArray();
                if(resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                    throw new Exception($"DI: Failed to Inject {type.Name}.{injectableMethod.Name}");

                injectableMethod.Invoke(injectable, resolvedInstances);
                Debug.Log($"DI: Method Injected {type.Name}.{injectableMethod.Name}");
            }
        }

        public void RuntimeInject(MonoBehaviour inject)
        {
            injectables.Add(inject);
            Inject(inject);
            Debug.Log($"DI: Runtime Injected {inject}");
        }

        public void RuntimeProvide(IDependencyProvider provide)
        {
            RegisterProvider(provide);
            Debug.Log($"DI: Runtime Provided {provide}");
        }
        object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        // Helper Function Purpose:
        // TRUE/FALSE if a monobehavior contains any members marked with [Inject]
        static bool IsInjectable(MonoBehaviour obj)
        {
            //Grab all fields with the correct binding flags
            var members = obj.GetType().GetMembers(k_bindingFlags);
            
            //Grab & Return the members marked with [Inject] 
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        //Purpose: Every IDependencyProvider passed into (RegisterProvider) most likely has a method or field
        //         annotated with a [Provide] Attribute 
        void RegisterProvider(IDependencyProvider provider)
        {
            Debug.Log($"DI: (2)     Attempting To Register Provider {provider.GetType().Name}");

            //Store for all the methods of the provider
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            int providedCount = 0;

            //Find the methods marked with [Provide]
            foreach (var method in methods)
            {
                //If theres no [Provide] Attribute, then continue to the next iteration immedietly
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                //Checking the return type of the Method, Ie: void, int, etc
                var returnType = method.ReturnType;

                //Invoking the Method, and getting whatever it returns
                var providedInstance = method.Invoke(provider, null);

                //If the returned (provided) thing we asked for is there, add it to the registry
                if (providedInstance != null)
                {
                    registry.Add(returnType, providedInstance);
                    Debug.Log($"DI: (3)         Registered {returnType.Name} from {provider.GetType().Name}");
                    providedCount++;
                }
                else //Otherwise throw an expection
                    throw new Exception($"DI: (4) FAIL :Provider {provider.GetType().Name} returned null for {returnType.Name}");
            }

            if(providedCount == 0)
                this.Error($"DI: (0)         No [Provide] attributes found on provider {provider.GetType().Name}, even though its marked as a IDependancyProvider");

        }


        //Helper Method:
        // Purpose: Return us all of the Monobehaviors in the scene
        static MonoBehaviour[] FindMonoBehaviors()
        {
            return FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);
        }


    }



}
