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
        public string Driver { get; set; }

        public int Index { get; set; }

        public IObservable<ContextTask> Generate()
        {
            return Observable.Defer(() => Observable.Return(new ContextTask(Driver, Index)));
        }
    }
}
