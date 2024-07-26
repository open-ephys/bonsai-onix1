using System;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class StartAcquisition : Combinator<ContextTask, IGroupedObservable<uint, oni.Frame>>
    {
        public int ReadSize { get; set; } = 2048;

        public int WriteSize { get; set; } = 2048;

        public override IObservable<IGroupedObservable<uint, oni.Frame>> Process(IObservable<ContextTask> source)
        {
            return source.SelectMany(context =>
            {
                return Observable.Create<IGroupedObservable<uint, oni.Frame>>((observer, cancellationToken) =>
                {
                    var frameSubscription = context.GroupedFrames.SubscribeSafe(observer);
                    try
                    {
                        return context.StartAsync(ReadSize, WriteSize, cancellationToken)
                                      .ContinueWith(_ => frameSubscription.Dispose());
                    }
                    catch
                    {
                        frameSubscription.Dispose();
                        throw;
                    }
                });
            });
        }
    }
}
