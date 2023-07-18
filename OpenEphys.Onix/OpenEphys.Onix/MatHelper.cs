using System;
using System.Reactive;
using System.Reactive.Linq;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    static class MatHelper
    {
        public static uint GetElementSize(Depth depth)
        {
            return depth switch
            {
                Depth.U8 or Depth.S8 => 1,
                Depth.U16 or Depth.S16 => 2,
                Depth.S32 or Depth.F32 => 4,
                Depth.F64 => 8,
                _ => throw new ArgumentException("Invalid depth was specified."),
            };
        }

        public static Mat GetDataHeader(this oni.Frame frame, Depth depth)
        {
            var channels = frame.DataSize / GetElementSize(depth);
            return new Mat(
                (int)channels,
                cols: 1,
                depth,
                channels: 1,
                frame.Data);
        }

        public static Mat GetDataHeader(this oni.Frame frame, uint channels, Depth depth)
        {
            var samples = frame.DataSize / GetElementSize(depth) / channels;
            return new Mat(
                (int)channels,
                (int)samples,
                depth,
                channels: 1,
                frame.Data);
        }

        public static IObservable<Mat> Buffer(IObservable<oni.Frame> source, int count, uint channels, Depth depth)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return Observable.Create<Mat>(observer =>
            {
                int bufferIndex = 0;
                Mat activeBuffer = default;
                var frameObserver = Observer.Create<oni.Frame>(
                    frame =>
                    {
                        using var frameHeader = GetDataHeader(frame, channels, depth);
                        if (frameHeader.Cols == count) observer.OnNext(frameHeader.Clone());
                        else
                        {
                            var frameIndex = 0;
                            while (frameIndex < frameHeader.Cols)
                            {
                                activeBuffer ??= new Mat((int)channels, count, depth, channels: 1);
                                var samplesToCopy = Math.Min(frameHeader.Cols - frameIndex, activeBuffer.Cols - bufferIndex);
                                using var bufferSubRect = activeBuffer.GetSubRect(new Rect(bufferIndex, 0, samplesToCopy, activeBuffer.Rows));
                                using var frameSubRect = samplesToCopy != frameHeader.Cols
                                    ? frameHeader.GetSubRect(new Rect(frameIndex, 0, samplesToCopy, frameHeader.Rows))
                                    : frameHeader;
                                CV.Copy(frameSubRect, bufferSubRect);

                                bufferIndex += samplesToCopy;
                                frameIndex += samplesToCopy;
                                if (bufferIndex >= count)
                                {
                                    observer.OnNext(activeBuffer);
                                    activeBuffer = default;
                                    bufferIndex = 0;
                                }
                            }
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(frameObserver);
            });
        }
    }
}
