using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Bonsai;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Produces a sequence of <see cref="NeuropixelsV2eBetaDataFrame"/> objects from a NeuropixelsV2eBeta headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureNeuropixelsV2eBeta"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Produces a sequence of NeuropixelsV2eDataFrame objects from a NeuropixelsV2e headstage.")]
    public class NeuropixelsV2eBetaData : Source<NeuropixelsV2eBetaDataFrame>
    {
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV2eBeta.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// Buffer size sets the number of frames that are buffered before propagating data.
        /// </remarks>
        [Description("The number of samples collected for each channel that are used to create a single NeuropixelsV2eBetaDataFrame.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public int BufferSize { get; set; } = 30;

        /// <summary>
        /// Gets or sets the probe index.
        /// </summary>
        [Description("The index of the probe from which to collect sample data")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public NeuropixelsV2Probe ProbeIndex { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that controls if the channels are ordered by depth.
        /// </summary>
        /// <remarks>
        /// If <see cref="OrderByDepth"/> is false, then channels are ordered from 0 to 383. 
        /// If <see cref="OrderByDepth"/> is true, then channels are ordered based on the depth
        /// of the electrodes.
        /// </remarks>
        [Description("Determines if the channels are returned ordered by depth.")]
        [Category(DeviceFactory.ConfigurationCategory)]
        public bool OrderByDepth { get; set; } = false;

        /// <summary>
        /// Generates a sequence of <see cref="NeuropixelsV2eDataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV2eDataFrame"/> objects.</returns>
        public unsafe override IObservable<NeuropixelsV2eBetaDataFrame> Generate()
        {
            var bufferSize = BufferSize;
            return DeviceManager.GetDevice(DeviceName).SelectMany(deviceInfo =>
            {
                var info = (NeuropixelsV2eDeviceInfo)deviceInfo;
                var (gainCorrection, probeConfiguration, metadata) = ProbeIndex switch
                {
                    NeuropixelsV2Probe.ProbeA => (info.GainCorrectionA, info.ProbeConfigurationA, info.ProbeMetadataA),
                    NeuropixelsV2Probe.ProbeB => (info.GainCorrectionB, info.ProbeConfigurationB, info.ProbeMetadataB),
                    _ => throw new InvalidEnumArgumentException($"Unexpected {nameof(ProbeIndex)} value: {ProbeIndex}")
                };

                if (metadata.ProbeSerialNumber == null)
                {
                    throw new InvalidOperationException($"{ProbeIndex} is not detected. Ensure that the flex connection is properly seated.");
                }
                else if (gainCorrection == null)
                {
                    throw new NullReferenceException($"Gain correction value is null for {ProbeIndex}.");
                }

                var device = info.GetDeviceContext(typeof(NeuropixelsV2eBeta));
                var passthrough = device.GetPassthroughDeviceContext(typeof(DS90UB9x));
                var probeData = device.Context
                    .GetDeviceFrames(passthrough.Address)
                    .Where(frame => NeuropixelsV2eBetaDataFrame.GetProbeIndex(frame) == (int)ProbeIndex);
                var orderByDepth = OrderByDepth;
                var invertPolarity = probeConfiguration.InvertPolarity;

                return Observable.Create<NeuropixelsV2eBetaDataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var amplifierBuffer = new ushort[NeuropixelsV2.ChannelCount, bufferSize];
                    var frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
                    var hubClockBuffer = new ulong[bufferSize];
                    var clockBuffer = new ulong[bufferSize];
                    int[,] channelOrder = orderByDepth ? Neuropixels.OrderChannelsByDepth(probeConfiguration.ChannelMap, RawToChannel) : RawToChannel;

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV2BetaPayload*)frame.Data.ToPointer();
                            NeuropixelsV2eBetaDataFrame.CopyAmplifierBuffer(payload->SuperFrame, amplifierBuffer, frameCounter, sampleIndex, gainCorrection.Value, invertPolarity, channelOrder);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;
                            if (++sampleIndex >= bufferSize)
                            {
                                var amplifierData = Mat.FromArray(amplifierBuffer);
                                var dataFrame = new NeuropixelsV2eBetaDataFrame(
                                    clockBuffer,
                                    hubClockBuffer,
                                    amplifierData,
                                    frameCounter);
                                observer.OnNext(dataFrame);
                                frameCounter = new int[NeuropixelsV2eBeta.FramesPerSuperFrame * bufferSize];
                                hubClockBuffer = new ulong[bufferSize];
                                clockBuffer = new ulong[bufferSize];
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    return probeData.SubscribeSafe(frameObserver);
                });
            });
        }

        // ADC & frame-index to channel mapping
        // First dimension: data index
        // Second dimension: frame index within super frame
        private static readonly int[,] RawToChannel = {
            { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 },                          // Data Index 9, ADC 1  
            { 96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118, 120, 122, 124, 126 },       // Data Index 10, ADC 7 
            { 192, 194, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214, 216, 218, 220, 222 },     // Data Index 11, ADC 13 
            { 288, 290, 292, 294, 296, 298, 300, 302, 304, 306, 308, 310, 312, 314, 316, 318 },     // Data Index 12, ADC 19 
            { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 },                          // Data Index 13, ADC 2 
            { 97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119, 121, 123, 125, 127 },       // Data Index 14, ADC 8 
            { 193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221, 223 },     // Data Index 15, ADC 14 
            { 289, 291, 293, 295, 297, 299, 301, 303, 305, 307, 309, 311, 313, 315, 317, 319 },     // Data Index 16, ADC 20 
            { 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52, 54, 56, 58, 60, 62 },                     // Data Index 17, ADC 3 
            { 128, 130, 132, 134, 136, 138, 140, 142, 144, 146, 148, 150, 152, 154, 156, 158 },     // Data Index 18, ADC 9 
            { 224, 226, 228, 230, 232, 234, 236, 238, 240, 242, 244, 246, 248, 250, 252, 254 },     // Data Index 19, ADC 15 
            { 320, 322, 324, 326, 328, 330, 332, 334, 336, 338, 340, 342, 344, 346, 348, 350 },     // Data Index 20, ADC 21 
            { 33, 35, 37, 39, 41, 43, 45, 47, 49, 51, 53, 55, 57, 59, 61, 63 },                     // Data Index 21, ADC 4 
            { 129, 131, 133, 135, 137, 139, 141, 143, 145, 147, 149, 151, 153, 155, 157, 159 },     // Data Index 22, ADC 10 
            { 225, 227, 229, 231, 233, 235, 237, 239, 241, 243, 245, 247, 249, 251, 253, 255 },     // Data Index 23, ADC 16 
            { 321, 323, 325, 327, 329, 331, 333, 335, 337, 339, 341, 343, 345, 347, 349, 351 },     // Data Index 24, ADC 22 
            { 64, 66, 68, 70, 72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94 },                     // Data Index 25, ADC 5 
            { 160, 162, 164, 166, 168, 170, 172, 174, 176, 178, 180, 182, 184, 186, 188, 190 },     // Data Index 26, ADC 11 
            { 256, 258, 260, 262, 264, 266, 268, 270, 272, 274, 276, 278, 280, 282, 284, 286 },     // Data Index 27, ADC 17 
            { 352, 354, 356, 358, 360, 362, 364, 366, 368, 370, 372, 374, 376, 378, 380, 382 },     // Data Index 28, ADC 23 
            { 65, 67, 69, 71, 73, 75, 77, 79, 81, 83, 85, 87, 89, 91, 93, 95 },                     // Data Index 29, ADC 6 
            { 161, 163, 165, 167, 169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191 },     // Data Index 30, ADC 12 
            { 257, 259, 261, 263, 265, 267, 269, 271, 273, 275, 277, 279, 281, 283, 285, 287 },     // Data Index 31, ADC 18 
            { 353, 355, 357, 359, 361, 363, 365, 367, 369, 371, 373, 375, 377, 379, 381, 383 }      // Data Index 32, ADC 24 
         };
    }
}
