using UnityEngine;

/// <summary>
/// Polymorphic Base
/// </summary>
public interface ISignalReceiver { }

/// <summary>
/// Typed Polymorphic Signal
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISignalReceiverC<T> : ISignalReceiver { } 


/// <summary>
/// Tagged Signal
/// </summary>
public interface ISignalReceiverT<T> : ISignalReceiver
{
    void Send(T t) => ReceiveSignal(t);
    void ReceiveSignal(T t);
}


/// <summary>
/// Tagged Signal
/// </summary>
public interface ISignalReceiverTC<T> : ISignalReceiver, ISignalReceiverC<T>
{
    string tag { get; }
    void Send(string senderTag, T ctx) => ReceiveSignal(senderTag, ctx);
    void ReceiveSignal(string tag, T ctx);
}


