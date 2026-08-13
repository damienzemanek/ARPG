using System;
using UnityEngine;

namespace DependencyInjection
{
    //Purpose: Supplying Dependancies
    // Have as many providers as you want
    public class Provider : MonoBehaviour, IDependencyProvider
    {
        //Dependancies

        [Provide]
        public ServiceA ProvideServiceA()
        {
            return new ServiceA();
        }

        [Provide]
        public ServiceB ProvideServiceB()
        {
            return new ServiceB();
        }


        [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }


    }


    //Purpose: Example Service 
    public class ServiceA
    { 
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceA.Initialize({message})");
        }
        
    }

    //Purpose: Example Service
    public class ServiceB
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceB.Initialize({message})");
        }

    }

    //Purpose: Example Factory for Service
    /// <summary>
    /// Factory that Caches a service when created the first time
    /// - That way you are not creating a new service everytime your fullfilling a dependency
    /// </summary>
    public class FactoryA
    {
        ServiceA cachedServiceA;

        public ServiceA CreatedServiceA()
        {
            if(cachedServiceA == null)
                cachedServiceA = new ServiceA();

            return cachedServiceA;
        }
    }

}
