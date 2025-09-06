using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// Note: This wrapper centralizes StreamJsonRpc usage inside SharedModel so that
// consumers don't need to reference StreamJsonRpc directly.
namespace HotPreview.SharedModel.Protocol
{
    public sealed class HotPreviewJsonRpc : IDisposable
    {
        private readonly StreamJsonRpc.JsonRpc _rpc;

        public HotPreviewJsonRpc(Stream sendingStream, Stream receivingStream)
        {
            _rpc = new StreamJsonRpc.JsonRpc(sendingStream, receivingStream);
        }

        public HotPreviewJsonRpc(Stream sendingStream, Stream receivingStream, object? target)
        {
            _rpc = new StreamJsonRpc.JsonRpc(sendingStream, receivingStream, target);
        }

        public void StartListening()
        {
            _rpc.StartListening();
        }

        public void AddLocalRpcTarget<TInterface>(TInterface target) where TInterface : class
        {
            _rpc.AddLocalRpcTarget(target);
        }

        public TInterface Attach<TInterface>() where TInterface : class
        {
            return _rpc.Attach<TInterface>();
        }

        public Task<T> InvokeWithParameterObjectAsync<T>(string method, object? argument, CancellationToken cancellationToken = default)
        {
            return _rpc.InvokeWithParameterObjectAsync<T>(method, argument, cancellationToken);
        }

        public Task InvokeWithParameterObjectAsync(string method, object? argument, CancellationToken cancellationToken = default)
        {
            return _rpc.InvokeWithParameterObjectAsync(method, argument, cancellationToken);
        }

        public void Dispose()
        {
            _rpc.Dispose();
        }

        public Task Completion => _rpc.Completion;

        public TraceSource TraceSource => _rpc.TraceSource;
    }
}
