using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    internal static class ObservableExtensions
    {
        public static IObservable<ContextTask> ConfigureAndLatchController(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchController(action), configure);
        }

        public static IObservable<ContextTask> ConfigureAndLatchLink(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchLink(action), configure);
        }

        public static IObservable<ContextTask> ConfigureAndLatchDevice(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchDevice(action), configure);
        }

        public static IObservable<ContextTask> ConfigureAndLatchDevice(this IObservable<ContextTask> source, Func<ContextTask, IObserver<ContextTask>, IDisposable> configure)
        {
            return Observable.Create<ContextTask>(observer => source
                .ConfigureAndLatchDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        public static IObservable<ContextTask> ConfigureDirectDevice(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureDirectDevice(action), configure);
        }

        public static IObservable<ContextTask> ConfigureDirectDevice(this IObservable<ContextTask> source, Func<ContextTask, IObserver<ContextTask>, IDisposable> configure)
        {
            return Observable.Create<ContextTask>(observer => source
                .ConfigureDirectDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        static IObservable<ContextTask> ConfigureContext(
            this IObservable<ContextTask> source,
            Action<ContextTask, Func<ContextTask, IDisposable>> configureContext,
            Func<ContextTask, IDisposable> configure)
        {
            return Observable.Create<ContextTask>(observer =>
            {
                var contextObserver = Observer.Create<ContextTask>(
                    context =>
                    {
                        configureContext(context, ctx =>
                        {
                            try
                            {
                                var disposable = configure(ctx);
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

        public static IDisposable SubscribeSafe<TSource, TResult>(
            this IObservable<TSource> source,
            IObserver<TResult> observer,
            Action<TSource> onNext)
        {
            var sourceObserver = Observer.Create<TSource>(
                value =>
                {
                    try { onNext(value); }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                observer.OnCompleted);
            return source.SubscribeSafe(sourceObserver);
        }
    }
}
