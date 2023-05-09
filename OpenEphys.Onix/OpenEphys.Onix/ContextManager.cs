using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace OpenEphys.Onix
{
    class ContextManager
    {
        static readonly Dictionary<(string driver, int index), (ReplaySubject<ContextTask> context, RefCountDisposable refCount)> contextSubjects = new();
        static readonly object subjectLock = new();

        internal static ContextDisposable ReserveContext(string driver, int index)
        {
            lock (subjectLock)
            {
                if (!contextSubjects.TryGetValue((driver, index), out var contextResource))
                {
                    var subject = new ReplaySubject<ContextTask>(2);
                    var dispose = Disposable.Create(() =>
                    {
                        subject.Dispose();
                        contextSubjects.Remove((driver, index));
                    });

                    var refCount = new RefCountDisposable(dispose);
                    contextSubjects.Add((driver, index), (subject, refCount));
                    return new ContextDisposable(subject, refCount);
                }

                return new ContextDisposable(contextResource.context, contextResource.refCount.GetDisposable());
            }
        }

        internal sealed class ContextDisposable : IDisposable
        {
            IDisposable resource;

            public ContextDisposable(ISubject<ContextTask> subject, IDisposable disposable)
            {
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                resource = disposable ?? throw new ArgumentNullException(nameof(disposable));
            }

            public ISubject<ContextTask> Subject { get; private set; }

            public void Dispose()
            {
                lock (subjectLock)
                {
                    if (resource != null)
                    {
                        resource.Dispose();
                        resource = null;
                    }
                }
            }
        }
    }
}
