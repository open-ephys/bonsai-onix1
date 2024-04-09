using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix
{
    public class StartAcquisition : Combinator<ContextTask, oni.Frame>
    {
        public int ReadSize { get; set; } = 2048;

        public int WriteSize { get; set; } = 2048;

        public override IObservable<oni.Frame> Process(IObservable<ContextTask> source)
        {
            return source.SelectMany(context =>
            {
                return Observable.Create<oni.Frame>(observer =>
                {
                    var disposable = context.FrameReceived.SubscribeSafe(observer);
                    try
                    {
                        context.Start(ReadSize, WriteSize);
                    }
                    catch
                    {
                        disposable.Dispose();
                        throw;
                    }
                    return Disposable.Create(() =>
                    {
                        context.Stop();
                        disposable.Dispose();
                    });
                });
            });
        }
    }
}
