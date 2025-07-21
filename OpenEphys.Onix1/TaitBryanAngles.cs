using System;
using System.Numerics;

/// <summary>
/// Represents a vector that describes the orientation of a rigid body using a set of 3 ordered rotations
/// about the object's principal axes. 
/// </summary>
/// <remarks>
/// Starting from a ENU (X: East, Y: North, Z: Up) coordinate system:
/// <list type="bullet">
/// <item><description>Yaw represents the bearing, which is the rotation about Z/Up.</description></item>
/// <item><description>Pitch represents the elevation, which is the rotation about Y' after application of Yaw
/// to the ENU coordinates.</description></item>
/// <item><description>Roll gives the bank angle, which is the rotation about X'' after application of Yaw and
/// Pitch to the the ENU coordinates.</description></item>
/// </list>
/// </remarks>
public struct TaitBryanAngles : IEquatable<TaitBryanAngles>
{
    private Vector3 _vector;

    /// <summary>
    /// Gets or sets the Yaw.
    /// </summary>
    /// <remarks>Yaw represents the bearing.</remarks>
    public float Yaw
    {
        readonly get => _vector.X;
        set => _vector.X = value;
    }

    /// <summary>
    /// Gets or sets the Pitch.
    /// </summary>
    /// <remarks>Pitch represents the elevation.</remarks>
    public float Pitch
    {
        readonly get => _vector.Y;
        set => _vector.Y = value;
    }

    /// <summary>
    /// Gets or sets the Roll.
    /// </summary>
    /// <remarks>Roll represents the bank angle.</remarks>
    public float Roll
    { 
        readonly get => _vector.Z;
        set => _vector.Z = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaitBryanAngles"/> struct with the specified yaw, pitch,
    /// and roll values.
    /// </summary>
    /// <param name="yaw">The yaw angle (bearing/rotation about Z axis).</param>
    /// <param name="pitch">The pitch angle (elevation/rotation about Y' axis).</param>
    /// <param name="roll">The roll angle (bank/rotation about X'' axis).</param>
    public TaitBryanAngles(float yaw, float pitch, float roll)
    {
        _vector = new Vector3(yaw, pitch, roll);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaitBryanAngles"/> struct from an existing <see
    /// cref="Vector3"/>.
    /// </summary>
    /// <param name="vector">The vector containing the yaw (X), pitch (Y), and roll (Z) values.</param>
    public TaitBryanAngles(Vector3 vector)
    {
        _vector = vector;
    }

    /// <summary>
    /// Implicitly converts a <see cref="TaitBryanAngles"/> to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="euler">The Tait-Bryan angles to convert.</param>
    /// <returns>A <see cref="Vector3"/> where X=Yaw, Y=Pitch, Z=Roll.</returns>
    public static implicit operator Vector3(TaitBryanAngles euler) => euler._vector;

    /// <summary>
    /// Implicitly converts a <see cref="Vector3"/> to a <see cref="TaitBryanAngles"/>.
    /// </summary>
    /// <param name="vector">The vector to convert, where X=Yaw, Y=Pitch, Z=Roll.</param>
    /// <returns>A <see cref="TaitBryanAngles"/> instance.</returns>
    public static implicit operator TaitBryanAngles(Vector3 vector) => new(vector);

    /// <summary>
    /// Determines whether two <see cref="TaitBryanAngles"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(TaitBryanAngles left, TaitBryanAngles right) => left._vector == right._vector;

    /// <summary>
    /// Determines whether two <see cref="TaitBryanAngles"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(TaitBryanAngles left, TaitBryanAngles right) => left._vector != right._vector;

    /// <inheritdoc cref = "Vector3.Equals(Vector3)"/>
    public bool Equals(TaitBryanAngles other) => _vector.Equals(other._vector);

    /// <inheritdoc cref = "Vector3.Equals(object)"/>
    public override bool Equals(object obj) => obj is TaitBryanAngles other && Equals(other);

    /// <inheritdoc cref = "Vector3.GetHashCode"/>
    public override int GetHashCode() => _vector.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string representation of the Tait-Bryan angles in the format "Yaw: {{Yaw}, Pitch: {Pitch},
    /// Roll: {Roll}".</returns>
    public override readonly string ToString() => $"Yaw: {Yaw}, Pitch: {Pitch}, Roll: {Roll}";

}
