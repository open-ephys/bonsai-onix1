using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    internal static class ObservableExtensions
    {
        public static IObservable<ContextTask> ConfigureHost(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureHost(action), configure);
        }

        public static IObservable<ContextTask> ConfigureLink(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureLink(action), configure);
        }

        public static IObservable<ContextTask> ConfigureDevice(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureDevice(action), configure);
        }

        public static IObservable<ContextTask> ConfigureDevice(this IObservable<ContextTask> source, Func<ContextTask, IObserver<ContextTask>, IDisposable> configure)
        {
            return Observable.Create<ContextTask>(observer => source
                .ConfigureDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        public static IObservable<ContextTask> ConfigureDeviceWithoutReset(this IObservable<ContextTask> source, Func<ContextTask, IDisposable> configure)
        {
            return source.ConfigureContext((context, action) => context.ConfigureDeviceWithoutReset(action), configure);
        }

        public static IObservable<ContextTask> ConfigureDeviceWithoutReset(this IObservable<ContextTask> source, Func<ContextTask, IObserver<ContextTask>, IDisposable> configure)
        {
            return Observable.Create<ContextTask>(observer => source
                .ConfigureDeviceWithoutReset(context => configure(context, observer))
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
