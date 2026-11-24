using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;

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
                .ConfigureAndLatchDevice(context => Configure(context, observer, configure))
                .SubscribeSafe(observer));
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IDisposable> configure) where TContext : ContextTask
        {
            return source.ConfigureContext((context, action, reportError) => context.ConfigureDirectDevice(ctx => action((TContext)ctx), reportError), configure);
        }

        public static IObservable<TContext> ConfigureDirectDevice<TContext>(this IObservable<TContext> source, Func<TContext, IObserver<TContext>, IDisposable> configure) where TContext : ContextTask
        {
            return Observable.Create<TContext>(observer => source
                .ConfigureDirectDevice(context => Configure(context, observer, configure))
                .SubscribeSafe(observer));
        }

        /// <summary>
        /// Runs a configuration action that reports errors to the operator's observer.
        /// </summary>
        /// <remarks>
        /// An error reported to the observer while the action runs, such as <see
        /// cref="SubscribeSafe{TSource, TResult}"/> failing to apply the current value of a property, fails the
        /// action instead, so that the configuration it made is disposed and the error is reported once the
        /// context has released its locks. Errors reported afterwards, such as failing to apply a property
        /// changed during acquisition, go to the observer.
        /// </remarks>
        static IDisposable Configure<TContext>(
            TContext context,
            IObserver<TContext> observer,
            Func<TContext, IObserver<TContext>, IDisposable> configure)
        {
            var configuring = true;
            Exception error = null;
            var configurationObserver = Observer.Create<TContext>(
                observer.OnNext,
                ex =>
                {
                    if (configuring) error ??= ex;
                    else observer.OnError(ex);
                },
                observer.OnCompleted);

            IDisposable configuration;
            try { configuration = configure(context, configurationObserver); }
            finally { configuring = false; }

            if (error != null)
            {
                configuration.Dispose();
                ExceptionDispatchInfo.Capture(error).Throw();
            }

            return configuration;
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
