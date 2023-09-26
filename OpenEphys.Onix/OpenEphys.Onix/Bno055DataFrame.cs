using System;
using System.Net;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class Bno055DataFrame
    {
        public unsafe Bno055DataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (Bno055Payload*)frame.Data.ToPointer();
            HubClock = unchecked((ulong)IPAddress.NetworkToHostOrder(payload->HubClock));
            Payload = *payload;
        }

        public ulong Clock { get; }

        public ulong HubClock { get; }

        public Bno055Payload Payload { get; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Bno055Payload
    {
        public long HubClock;
        public short EulerAngleYaw;
        public short EulerAngleRoll;
        public short EulerAnglePitch;
        public short QuaternionW;
        public short QuaternionX;
        public short QuaternionY;
        public short QuaternionZ;
        public short AccelerationX;
        public short AccelerationY;
        public short AccelerationZ;
        public short GravityX;
        public short GravityY;
        public short GravityZ;
        public byte Temperature;
        public Bno055CalibrationFlags Calibration;
    }

    [Flags]
    public enum Bno055CalibrationFlags : byte
    {
        None = 0,
        System = 0x3,
        Gyroscope = 0xC,
        Accelerometer = 0x30,
        Magnetometer = 0xC0
    }
}
