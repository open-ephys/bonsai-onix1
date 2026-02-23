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
    /// Produces a sequence of <see cref="NeuropixelsV1DataFrame">NeuropixelsV1DataFrames</see> from a
    /// NeuropixelsV1f headstage.
    /// </summary>
    /// <remarks>
    /// This data IO operator must be linked to an appropriate configuration, such as a <see
    /// cref="ConfigureNeuropixelsV1f"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    public class NeuropixelsV1fData : Source<NeuropixelsV1DataFrame>
    {
        int bufferSize = 36;

        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [TypeConverter(typeof(NeuropixelsV1f.NameConverter))]
        [Description(SingleDeviceFactory.DeviceNameDescription)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        /// <remarks>
        /// Buffer size sets the number of super frames that are buffered before propagating data.
        /// A super frame consists of 384 channels from the spike-band and 32 channels from the LFP band.
        /// The buffer size must be a multiple of 12.
        /// </remarks>
        [Description("Number of super-frames (384 channels from spike band and 32 channels from " +
            "LFP band) to buffer before propagating data. Must be a multiple of 12.")]
        public int BufferSize
        {
            get => bufferSize;
            set => bufferSize = (int)(Math.Ceiling((double)value / NeuropixelsV1.FramesPerRoundRobin) * NeuropixelsV1.FramesPerRoundRobin);
        }

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
        /// Generates a sequence of <see cref="NeuropixelsV1DataFrame"/> objects.
        /// </summary>
        /// <returns>A sequence of <see cref="NeuropixelsV1DataFrame"/> objects.</returns>
        public unsafe override IObservable<NeuropixelsV1DataFrame> Generate()
        {
            var spikeBufferSize = BufferSize;
            var lfpBufferSize = spikeBufferSize / NeuropixelsV1.FramesPerRoundRobin;
            var bufferSize = BufferSize;
            var orderByDepth = OrderByDepth;

            return DeviceManager.GetDevice(DeviceName).SelectMany(
                deviceInfo => Observable.Create<NeuropixelsV1DataFrame>(observer =>
                {
                    var sampleIndex = 0;
                    var info = (NeuropixelsV1fDeviceInfo)deviceInfo;
                    var device = info.GetDeviceContext(typeof(NeuropixelsV1f));
                    var spikeBuffer = new ushort[NeuropixelsV1.ChannelCount, spikeBufferSize];
                    var lfpBuffer = new ushort[NeuropixelsV1.ChannelCount, lfpBufferSize];
                    var frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
                    var hubClockBuffer = new ulong[spikeBufferSize];
                    var clockBuffer = new ulong[spikeBufferSize];
                    int[,] channelOrder = orderByDepth ? Neuropixels.OrderChannelsByDepth(info.ProbeGroup.ChannelMap, RawToChannel) : RawToChannel;

                    var frameObserver = Observer.Create<oni.Frame>(
                        frame =>
                        {
                            var payload = (NeuropixelsV1fPayload*)frame.Data.ToPointer();
                            NeuropixelsV1fDataFrame.CopyAmplifierBuffer(payload->AmplifierData, frameCountBuffer, spikeBuffer, lfpBuffer, sampleIndex, channelOrder);
                            hubClockBuffer[sampleIndex] = payload->HubClock;
                            clockBuffer[sampleIndex] = frame.Clock;

                            if (++sampleIndex >= spikeBufferSize)
                            {
                                var spikeData = Mat.FromArray(spikeBuffer);
                                var lfpData = Mat.FromArray(lfpBuffer);
                                observer.OnNext(new NeuropixelsV1DataFrame(clockBuffer, hubClockBuffer, frameCountBuffer, spikeData, lfpData));
                                frameCountBuffer = new int[spikeBufferSize * NeuropixelsV1.FramesPerSuperFrame];
                                hubClockBuffer = new ulong[spikeBufferSize];
                                clockBuffer = new ulong[spikeBufferSize];
                                sampleIndex = 0;
                            }
                        },
                        observer.OnError,
                        observer.OnCompleted);
                    return deviceInfo.Context
                        .GetDeviceFrames(device.Address)
                        .SubscribeSafe(frameObserver);
                }));
        }

        // ADC to channel
        // First dimension: ADC index
        // Second dimension: frame index within super frame
        // Output: channel number
        static readonly int[,] RawToChannel = {
            {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22 },
            {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23 },
            {24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46 },
            {25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47 },
            {48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70 },
            {49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71 },
            {72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94 },
            {73, 75, 77, 79, 81, 83, 85, 87, 89, 91, 93, 95 },
            {96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118 },
            {97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119 },
            {120, 122, 124, 126, 128, 130, 132, 134, 136, 138, 140, 142 },
            {121, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 143 },
            {144, 146, 148, 150, 152, 154, 156, 158, 160, 162, 164, 166 },
            {145, 147, 149, 151, 153, 155, 157, 159, 161, 163, 165, 167 },
            {168, 170, 172, 174, 176, 178, 180, 182, 184, 186, 188, 190 },
            {169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191 },
            {192, 194, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214 },
            {193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215 },
            {216, 218, 220, 222, 224, 226, 228, 230, 232, 234, 236, 238 },
            {217, 219, 221, 223, 225, 227, 229, 231, 233, 235, 237, 239 },
            {240, 242, 244, 246, 248, 250, 252, 254, 256, 258, 260, 262 },
            {241, 243, 245, 247, 249, 251, 253, 255, 257, 259, 261, 263 },
            {264, 266, 268, 270, 272, 274, 276, 278, 280, 282, 284, 286 },
            {265, 267, 269, 271, 273, 275, 277, 279, 281, 283, 285, 287 },
            {288, 290, 292, 294, 296, 298, 300, 302, 304, 306, 308, 310 },
            {289, 291, 293, 295, 297, 299, 301, 303, 305, 307, 309, 311 },
            {312, 314, 316, 318, 320, 322, 324, 326, 328, 330, 332, 334 },
            {313, 315, 317, 319, 321, 323, 325, 327, 329, 331, 333, 335 },
            {336, 338, 340, 342, 344, 346, 348, 350, 352, 354, 356, 358 },
            {337, 339, 341, 343, 345, 347, 349, 351, 353, 355, 357, 359 },
            {360, 362, 364, 366, 368, 370, 372, 374, 376, 378, 380, 382 },
            {361, 363, 365, 367, 369, 371, 373, 375, 377, 379, 381, 383 } };
    }
}
