using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// 3D-orientation data produced by a Bosch Bno55 9-axis inertial measurement unit (IMU).
    /// </summary>
    /// <remarks>
    /// The physical interpretation of the orientation measurements contained within a <see
    /// cref="Bno055DataFrame"/> depends on the sensor fusion mode that is enabled and the axis configuration
    /// that is chosen (see page. 26 of the <a
    /// href="https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bno055-ds000.pdf">datasheet</a>)
    /// . If the chip is in NDOF mode and is calibrated, orientation measurements (Quaternion, Euler Angles,
    /// and Gravity Vector) are absolute ("allocentric") and referenced to the gravity vector and Earth's
    /// magnetic field. Specifically, if the chip's axes are oriented such that Y points towards magnetic
    /// north, X points towards magnetic east, and Z points opposite the gravity vector, the orientation
    /// reading will be null (i.e. Quaternion: X = 0, Y = 0, Z = 0, W = 1; Euler Angles: Yaw = 0, Pitch = 0,
    /// and Roll = 0 degrees; Gravity: X = 0, Y = 0, Z = 9.8 m/s^2). Linear acceleration readings are always
    /// taken relative to the chip's axis definitions (they are "egocentric").
    /// </remarks>
    public class Bno055DataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bno055DataFrame"/> class.
        /// </summary>
        /// <param name="frame">An ONI data frame containing Bno055 data.</param>
        public unsafe Bno055DataFrame(oni.Frame frame)
            : this(frame.Clock, (Bno055Payload*)frame.Data.ToPointer())
        {
        }

        internal unsafe Bno055DataFrame(ulong clock, Bno055Payload* payload)
            : this(clock, &payload->Data)
        {
            HubClock = payload->HubClock;
        }

        internal unsafe Bno055DataFrame(ulong clock, Bno055DataPayload* payload)
            : base(clock)
        {
            EulerAngle = new TaitBryanAngles(
                yaw: Bno055.EulerAngleScale * payload->EulerAngle[0],
                pitch: Bno055.EulerAngleScale * payload->EulerAngle[1],
                roll: Bno055.EulerAngleScale * payload->EulerAngle[2]);
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

        /// <summary>
        /// Gets the 3D orientation in as Euler angles using the Tait-Bryan formalism.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>Yaw: 0 to 360 degrees.</description></item>
        /// <item><description>Roll: -180 to 180 degrees</description></item>
        /// <item><description>Pitch: -90 to 90 degrees</description></item>
        /// </list>
        /// </remarks>
        public TaitBryanAngles EulerAngle { get; }

        /// <summary>
        /// Gets the 3D orientation represented as a Quaternion.
        /// </summary>
        public Quaternion Quaternion { get; }

        /// <summary>
        /// Gets the linear acceleration vector in units of m / s^2.
        /// </summary>
        public Vector3 Acceleration { get; }

        /// <summary>
        /// Gets the gravity acceleration vector in units of m / s^2.
        /// </summary>
        public Vector3 Gravity { get; }

        /// <summary>
        /// Gets the chip temperature in Celsius.
        /// </summary>
        public int Temperature { get; }

        /// <summary>
        /// Gets MEMS subsystem and sensor fusion calibration status.
        /// </summary>
        public Bno055CalibrationFlags Calibration { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Bno055Payload
    {
        public ulong HubClock;
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

    /// <summary>
    /// Specifies the MEMS subsystem and sensor fusion calibration status.
    /// </summary>
    [Flags]
    public enum Bno055CalibrationFlags : byte
    {
        /// <summary>
        /// Specifies that no sub-system is calibrated.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the magnetometer is poorly calibrated.
        /// </summary>
        MagnetometerLow = 0b0000_0001,

        /// <summary>
        /// Specifies that the magnetometer is partially calibrated.
        /// </summary>
        MagnetometerMed = 0b0000_0010,

        /// <summary>
        /// Specifies that the magnetometer is fully calibrated.
        /// </summary>
        MagnetometerFull = 0b0000_0011,

        /// <summary>
        /// Specifies that the accelerometer is poorly calibrated.
        /// </summary>
        AccelerometerLow = 0b0000_0100,

        /// <summary>
        /// Specifies that the accelerometer is partially calibrated.
        /// </summary>
        AccelerometerMed = 0b0000_1000,

        /// <summary>
        /// Specifies that the accelerometer is fully calibrated.
        /// </summary>
        AccelerometerFull = 0b0000_1100,

        /// <summary>
        /// Specifies that the gyroscope is poorly calibrated.
        /// </summary>
        GyroscopeLow = 0b0001_0000,

        /// <summary>
        /// Specifies that the gyroscope is partially calibrated.
        /// </summary>
        GyroscopeMed = 0b0010_0000,

        /// <summary>
        /// Specifies that the gyroscope is fully calibrated.
        /// </summary>
        GyroscopeFull = 0b0011_0000,

        /// <summary>
        /// Specifies that sensor fusion is poorly calibrated.
        /// </summary>
        SystemLow = 0b0100_0000,

        /// <summary>
        /// Specifies that sensor fusion is partially calibrated.
        /// </summary>
        SystemMed = 0b1000_0000,

        /// <summary>
        /// Specifies that sensor fusion is fully calibrated.
        /// </summary>
        SystemFull = 0b1100_0000,

    }
}
