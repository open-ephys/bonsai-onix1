using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai;

namespace OpenEphys.Onix1
{

    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class PauseAcquisition
    {
        public bool ResetAcquisitionTimer { get; set; } = false;
        public IObservable<TContext> Process<TContext>(IObservable<TContext> context, IObservable<bool> pause) where TContext : ContextTask
        {
            return context.Publish(currentContext =>
            {
                //NB : The real WithLatestFrom operator was implemented in a reactive exensions version
                //posterior to the one that bonsai uses
                var withLatestFromNode = new Bonsai.Reactive.WithLatestFrom();

                var pauseEffect = withLatestFromNode.Process(pause, currentContext)
                .SelectMany(async tuple =>
                {
                    if (tuple.Item1)
                    {
                        await tuple.Item2.PauseAcquisition();
                    }
                    else
                    {
                        tuple.Item2.ResumeAcquisition(ResetAcquisitionTimer);
                    }
                    return tuple.Item2;
                })
                .IgnoreElements();
                return currentContext.Merge(pauseEffect);            
            });
        }
    }
}
