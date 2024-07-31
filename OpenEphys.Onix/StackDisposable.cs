using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace OpenEphys.Onix
{
    internal class StackDisposable : IDisposable
    {
        readonly Stack<IDisposable> _disposables;
        readonly IDisposable disposable;

        public StackDisposable()
        {
            _disposables = new Stack<IDisposable>();
            disposable = Disposable.Create(DisposeAll);
        }

        public StackDisposable(int capacity)
        {
            _disposables = new Stack<IDisposable>(capacity);
            disposable = Disposable.Create(DisposeAll);
        }

        public void Push(IDisposable item)
        {
            _disposables.Push(item);
        }

        private void DisposeAll()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
