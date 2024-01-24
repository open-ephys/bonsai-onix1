using Bonsai;
using System;
using System.ComponentModel;
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
                var context = new ContextTask(driver, index);
                try
                {
                    observer.OnNext(context);
                    return context;
                }
                catch
                {
                    context.Dispose();
                    throw;
                }
            });
        }
    }
}
