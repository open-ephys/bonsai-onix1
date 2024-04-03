using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OpenEphys.Onix
{
    internal static class ObservableExtensions
    {
        public static IObservable<ContextTask> ConfigureHost(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> action)
        {
            return source.Do(context => context.ConfigureHost(action));
        }

        public static IObservable<ContextTask> ConfigureLink(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> action)
        {
            return source.Do(context => context.ConfigureLink(action));
        }

        public static IObservable<ContextTask> ConfigureDevice(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> selector)
        {
            return Observable.Create<ContextTask>(observer =>
            {
                var contextObserver = Observer.Create<ContextTask>(
                    context =>
                    {
                        context.ConfigureDevice(ctx =>
                        {
                            try
                            {
                                var disposable = selector(ctx);
                                return Disposable.Create(() =>
                                {
                                    try { disposable.Dispose(); }
                                    catch (Exception ex)
                                    {
                                        observer.OnError(ex);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                                throw;
                            }
                        });
                        observer.OnNext(context);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(contextObserver);
            });
        }
    }
}
