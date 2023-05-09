using System;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;

namespace OpenEphys.Onix
{
    internal static class ObservableExtensions
    {
        public static IObservable<ContextTask> ConfigureDevice(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> selector)
        {
            return Observable.Create<ContextTask>(observer =>
            {
                var sourceDisposable = new SingleAssignmentDisposable();
                var contextObserver = Observer.Create<ContextTask>(
                    context =>
                    {
                        sourceDisposable.Disposable = selector(context);
                        observer.OnNext(context);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return new CompositeDisposable(
                    sourceDisposable,
                    source.SubscribeSafe(contextObserver));
            });
        }
    }
}
