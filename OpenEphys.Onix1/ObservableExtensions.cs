using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    internal static class ObservableExtensions
    {
        public static IObservable<TContext> ConfigureAndLatchController<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchController(ctx => action((TContext)ctx)), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchLink<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchLink(ctx => action((TContext)ctx)), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchDevice<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action) => context.ConfigureAndLatchDevice(ctx => action((TContext)ctx)), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchDevice<TContext>(this IObservable<TContext> source, Func<TContext, IObserver<TContext>, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer => source
                .ConfigureAndLatchDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action) => context.ConfigureDirectDevice(ctx => action((TContext)ctx)), configure);
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IObserver<TContext>, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer => source
                .ConfigureDirectDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        public static IObservable<TContext> ConfigureCalibration<TContext>(
            this IObservable<TContext> source,
            uint deviceAddress,
            bool enabled,
            Func<IObservable<oni.Frame>, IObservable<oni.Frame>> data,
            Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            // NB: Unlike other stages, this cannot be disabled from within its callbacks: a disabled device
            // produces no frames and acquisition start would block, so it must not be registered at all.
            if (!enabled) return source;

            // NB: The outer observer is only needed to forward errors produced by the calibration data
            // sequence to this node; configure errors are already handled by ConfigureContext
            return Observable.Create<TContext>(observer => source
                .ConfigureContext(
                    (context, action) => context.ConfigureCalibration(
                        deviceAddress,
                        frames => data(frames)?.Do(_ => { }, observer.OnError),
                        ctx => action((TContext)ctx)),
                    configure)
                .SubscribeSafe(observer));
        }

        static IObservable<TContext> ConfigureContext<TContext>(
            this IObservable<TContext> source,
            Action<TContext, Func<TContext, IDisposable>> configureContext,
            Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer =>
            {
                var contextObserver = Observer.Create<TContext>(
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
