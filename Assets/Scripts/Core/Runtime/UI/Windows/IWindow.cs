namespace Core.UI.Windows
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IWindow
    {
        GameObject gameObject { get; }
        UniTask OpenAsync(CancellationToken ct);
        UniTask CloseAsync(CancellationToken ct);
        UniTask HideAsync(CancellationToken ct);
        UniTask ShowAsync(CancellationToken ct);
    }

    public interface IViewModel : IDisposable
    {
        
    }

    public interface IPayloadReceiver<in TPayload>
    {
        void SetPayload(TPayload payload);
    }

    public interface IPayloadedWindow<in TPayload>
    {
        void SetPayload(TPayload payload);
    }

}