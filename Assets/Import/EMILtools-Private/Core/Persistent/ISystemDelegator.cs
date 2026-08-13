using System;

namespace EMILtools.Core
{


    public interface IDelegator { }

    public interface IInvokeWithEnum : IDelegator
    {
        void Invoke(Enum value);
    }
    
    /// <summary>
    /// Low level Delegate
    /// </summary>
    /// <typeparam name="TAbstractedDelegate"></typeparam>
    public interface IDelegatorAbstract<TAbstractedDelegate> : IDelegator
    {
        TAbstractedDelegate Add(TAbstractedDelegate cb);
        TAbstractedDelegate Remove(TAbstractedDelegate cb);
    }
    
    /// <summary>
    /// Delegate with no constraints
    /// </summary>
    public interface ISystemDelegator : IDelegatorAbstract<Delegate>
    {
        Delegate Add(Delegate cb);
        Delegate Remove(Delegate cb);
    }

    /// <summary>
    /// Generic Constrained TDelegate
    /// No CRTP
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    public interface IPersistentDelegate<TDelegate> : IDelegator
        where TDelegate : Delegate
    {
        void Add(TDelegate cb);
        void Remove(TDelegate cb);
        int Count { get; }
        void PrintInvokeListNames();
    }

    /// <summary>
    /// Generic Constrained TDelegate and TPersistentAction
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <typeparam name="TPersistentCRTP"></typeparam>
    public interface IPersistentAction<TDelegate, out TPersistentCRTP> : IPersistentDelegate<TDelegate>
        where TDelegate : Delegate
        where TPersistentCRTP : IPersistentAction<TDelegate, TPersistentCRTP>
    {
        TPersistentCRTP Add(TDelegate cb);
        TPersistentCRTP Remove(TDelegate cb);
    }
    
}