using System;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A load tester data frame.
    /// </summary>
    public class LoadTesterDataFrame : DataFrame
    {
        /// <summary>
        /// Gets the difference between the hub clock and the loopback value at the moment the loopback
        /// value was received.
        /// </summary>
        /// <remarks>
        /// This value is the result of subtracting the loopback value written by the <see
        /// cref="LoadTesterLoopback"/> operator from the device's hub clock counter value at the moment that
        /// the loop back value was received. Typically, the <see cref="DataFrame.HubClock"/> member is sent
        /// to <see cref="LoadTesterLoopback"/> operator such that this value is a hardware-timed measurement
        /// value of real-time latency. This value is only updated when a new loopback value is sent to the
        /// device using the <see cref="LoadTesterLoopback"/> operator.
        /// </remarks>
        public ulong HubClockDelta { get; }

        /// <summary>
        /// Gets the counter array.
        /// </summary>
        /// <remarks>
        /// This is a <see cref="ConfigureLoadTester.ReceivedWords"/>-long array of incrementing integers that
        /// is used for simulating the bandwidth of physical data sources.
        /// </remarks>
        public ushort[] Counter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTesterDataFrame"/> class.
        /// </summary>
        /// <param name="frame">A data frame produced by a load tester device.</param>
        /// <param name="receivedWords">The number of counter words that appear at the end of the load test
        /// data frame. This number is determined by the value of <see
        /// cref="ConfigureLoadTester.ReceivedWords"/>.</param>
        public unsafe LoadTesterDataFrame(oni.Frame frame, uint receivedWords)
            : base(frame.Clock)
        {
            var payload = (LoadTesterPayload*)frame.Data.ToPointer();
            var counterPtr = (ushort*)((byte*)payload + sizeof(LoadTesterPayload));

            HubClock = payload->HubClock;
            HubClockDelta = payload->HubClockDelta;
            Counter = new Span<ushort>(counterPtr, (int)receivedWords).ToArray();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LoadTesterPayload
    {
        public ulong HubClock;
        public ulong HubClockDelta;
        // NB: The ushort Counter array may or may not reside here. Its size is determined by ReceivedWords.
    }
}

