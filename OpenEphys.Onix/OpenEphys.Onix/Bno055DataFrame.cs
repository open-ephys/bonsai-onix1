using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix
{
    public class Bno055DataFrame
    {
        public unsafe Bno055DataFrame(oni.Frame frame)
        {
            Clock = frame.Clock;
            var payload = (Bno055Payload*)frame.Data.ToPointer();
            HubClock = BitHelper.SwapEndian(payload->HubClock);
            EulerAngle = new Vector3(
                y: Bno055.EulerAngleScale * payload->EulerAngle[0],  // yaw
                z: Bno055.EulerAngleScale * payload->EulerAngle[1],  // roll
                x: Bno055.EulerAngleScale * payload->EulerAngle[2]); // pitch
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

        public ulong HubClock { get; }

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
        public ulong HubClock;
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
