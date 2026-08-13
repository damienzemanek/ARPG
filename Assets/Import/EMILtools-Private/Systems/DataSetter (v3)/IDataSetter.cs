using System;
using EMILtools.Core;

namespace EMILtools.Systems
{

    
    /// <summary>
    /// Settables manage generic state using Template Method Pattern
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IDataSetter
    {
        public IPublisher Publisher { get; set;  }
        public ISubscriber Subscriber { get; }
        public PersistentAction OnSetEvent { get; }
    }
    

}



