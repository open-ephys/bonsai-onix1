using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    [Collection(TestDriverCollection.Name)]
    public class ContextTaskTests
    {
        // A constructor that fails must release the hardware, so that opening it again fails the same way
        // rather than because the first context still holds it.
        [Fact]
        public void Constructor_UnsupportedFirmware_ReleasesHardware()
        {
            Assert.Throws<NotSupportedException>(() => new OnixContextTask(TestDriver.Name, TestDriver.UnsupportedFirmwareIndex, ValidationLevel.Normal));

            // NB: If the first context was not disposed, it still holds the host index, and the test driver
            // refuses to open it again with ONI_ERETRIG, which ContextTask throws as an ONIException
            Assert.Throws<NotSupportedException>(() => new OnixContextTask(TestDriver.Name, TestDriver.UnsupportedFirmwareIndex, ValidationLevel.Normal));
        }

        // A context whose initialization fails after the driver has opened the hardware must release it, so
        // that opening it again fails the same way.
        [Fact]
        public void Constructor_BadDeviceTable_ReleasesHardware()
        {
            var error = Assert.Throws<oni.ONIException>(() => new GenericContextTask(TestDriver.Name, TestDriver.BadDeviceTableIndex));
            Assert.Equal(TestDriver.RepeatedDeviceIndexError, error.Number);

            // NB: If the first context was not destroyed, it still holds the host index, and the test driver
            // refuses to open it again with ONI_ERETRIG
            error = Assert.Throws<oni.ONIException>(() => new GenericContextTask(TestDriver.Name, TestDriver.BadDeviceTableIndex));
            Assert.Equal(TestDriver.RepeatedDeviceIndexError, error.Number);
        }

        // A read failure during acquisition is reported to frame observers, which may dispose the context in
        // response, as a workflow does when it is torn down by the error.
        [Fact]
        public void ReadFailure_ObserverDisposesContext_DoesNotDeadlock()
        {
            var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);
            context.WriteRegister(context.DeviceTable.Keys.First(), TestDriver.ReadFaultRegister, 1);

            using var disposed = new ManualResetEventSlim();
            context.GroupedFrames.Subscribe(_ => { }, _ =>
            {
                context.Dispose();
                disposed.Set();
            });

            context.StartAsync((int)context.MaxReadFrameSize, (int)context.MaxWriteFrameSize);
            Assert.True(disposed.Wait(TimeSpan.FromSeconds(5)), "Disposing the context from the error handler did not return.");
        }

        // A configuration action that fails when acquisition starts is reported to the operator that queued it,
        // whose observer may dispose the context in response, as a workflow does when it is torn down by the
        // error.
        [Fact]
        public void ConfigurationFailure_ObserverDisposesContext_DoesNotDeadlock()
        {
            var context = new GenericContextTask(TestDriver.Name, TestDriver.Index);

            using var errorReported = new ManualResetEventSlim();
            using var disposed = new ManualResetEventSlim();

            // NB: The source does not complete, like CreateContext, since an error after completion is ignored
            Observable.Never<GenericContextTask>().StartWith(context)
                .ConfigureDirectDevice(_ => throw new InvalidOperationException())
                .Subscribe(_ => { }, _ =>
                {
                    errorReported.Set();
                    context.Dispose();
                    disposed.Set();
                });

            // NB: Not on a thread pool thread, where Dispose would run DisposeContext inline while waiting for it,
            // which does not deadlock. On another thread, so that a deadlock fails the test instead of hanging it.
            new Thread(() =>
            {
                try { context.StartAsync((int)context.MaxReadFrameSize, (int)context.MaxWriteFrameSize); }
                catch (InvalidOperationException) { }
            }) { IsBackground = true }.Start();
            Assert.True(errorReported.Wait(TimeSpan.FromSeconds(5)), "The configuration error was not reported.");
            Assert.True(disposed.Wait(TimeSpan.FromSeconds(5)), "Disposing the context from the error handler did not return.");
        }

        sealed class GenericContextTask : ContextTask
        {
            public GenericContextTask(string driver, int index)
                : base(driver, index)
            {
            }
        }
    }
}
