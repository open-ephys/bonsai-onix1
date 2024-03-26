using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class Bno055DataFrame
    {
        public unsafe Bno055DataFrame(oni.Frame frame)
            : this(frame.Clock, (Bno055Payload*)frame.Data.ToPointer())
        {
        }

        internal unsafe Bno055DataFrame(ulong clock, Bno055Payload* payload)
            : this(clock, &payload->Data)
        {
            HubSyncCounter = payload->HubSyncCounter;
        }

        internal unsafe Bno055DataFrame(ulong clock, Bno055DataPayload* payload)
        {
            Clock = clock;
            EulerAngle = new Vector3(
                x: Bno055.EulerAngleScale * payload->EulerAngle[0],
                y: Bno055.EulerAngleScale * payload->EulerAngle[1],
                z: Bno055.EulerAngleScale * payload->EulerAngle[2]);
            Quaternion = new Quaternion(
                w: Bno055.QuaternionScale * payload->Quaternion[0],
                x: Bno055.QuaternionScale * payload->Quaternion[1],
                y: Bno055.QuaternionScale * payload->Quaternion[2],
                z: Bno055.QuaternionScale * payload->Quaternion[3]);
            Acceleration = new Vector3(
                x: Bno055.AccelerationScale * payload->Acceleration[0],
                y: Bno055.AccelerationScale * payload->Acceleration[1],
                z: Bno055.AccelerationScale * payload->Acceleration[2]);
            Gravity = new Vector3(
                x: Bno055.AccelerationScale * payload->Gravity[0],
                y: Bno055.AccelerationScale * payload->Gravity[1],
                z: Bno055.AccelerationScale * payload->Gravity[2]);
            Temperature = payload->Temperature;
            Calibration = payload->Calibration;
        }

        public ulong Clock { get; }

        public ulong HubSyncCounter { get; }

        public Vector3 EulerAngle { get; }

        public Quaternion Quaternion { get; }

        public Vector3 Acceleration { get; }

        public Vector3 Gravity { get; }

        public int Temperature { get; }

        public Bno055CalibrationFlags Calibration { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Bno055Payload
    {
        public ulong HubSyncCounter;
        public Bno055DataPayload Data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Bno055DataPayload
    {
        public fixed short EulerAngle[3];
        public fixed short Quaternion[4];
        public fixed short Acceleration[3];
        public fixed short Gravity[3];
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
