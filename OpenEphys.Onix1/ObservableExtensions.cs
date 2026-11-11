using System;
using System.Reactive;
using System.Reactive.Linq;

namespace OpenEphys.Onix1
{
    internal static class ObservableExtensions
    {
        public static IObservable<TContext> ConfigureAndLatchController<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action, reportError) => context.ConfigureAndLatchController(ctx => action((TContext)ctx), reportError), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchLink<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action, reportError) => context.ConfigureAndLatchLink(ctx => action((TContext)ctx), reportError), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchDevice<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action, reportError) => context.ConfigureAndLatchDevice(ctx => action((TContext)ctx), reportError), configure);
        }

        public static IObservable<TContext> ConfigureAndLatchDevice<TContext>(this IObservable<TContext> source, Func<TContext, IObserver<TContext>, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer => source
                .ConfigureAndLatchDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action, reportError) => context.ConfigureDirectDevice(ctx => action((TContext)ctx), reportError), configure);
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IObserver<TContext>, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer => source
                .ConfigureDirectDevice(context => configure(context, observer))
                .SubscribeSafe(observer));
        }

        static IObservable<TContext> ConfigureContext<TContext>(
            this IObservable<TContext> source,
            Action<TContext, Func<TContext, IDisposable>, Action<Exception>> configureContext,
            Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer =>
            {
                var contextObserver = Observer.Create<TContext>(
                    context =>
                    {
                        configureContext(context, configure, observer.OnError);
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
