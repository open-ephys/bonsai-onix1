using Bonsai;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace OpenEphys.Onix
{
    [Description("")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class CreateContext
    {
        public string Driver { get; set; } = "riffa";

        public int Index { get; set; }

        public IObservable<ContextTask> Generate()
        {
            return Observable.Create<ContextTask>(observer =>
            {
                var driver = Driver;
                var index = Index;
                var disposable = ContextManager.ReserveContext(driver, index);
                var context = new ContextTask(driver, index);
                var subject = disposable.Subject;
                observer.OnNext(context);
                subject.OnNext(context);

                return Disposable.Create(() =>
                {
                    subject.OnCompleted();
                    disposable.Dispose();
                    context.Dispose();
                });
            });
        }
    }
}
