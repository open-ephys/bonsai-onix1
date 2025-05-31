using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents a, possibly automatically set, port voltage. 
    /// </summary>
    public class AutoPortVoltage
    {
        /// <summary>
        /// Gets or sets a value the requested port voltage. If null, the voltage will be set automatically.
        /// </summary>
        public double? Requested { get; set; }

        /// <summary>
        /// Gets or sets the last applied port voltage
        /// </summary>
        [XmlIgnore]
        public double? Applied { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPortVoltage"/> class.
        /// </summary>
        public AutoPortVoltage() 
        {
            Requested = null;
            Applied = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPortVoltage"/> class.
        /// </summary>
        /// <param name="value">A value determining the requested port voltage</param>
        public AutoPortVoltage(double? value) 
        {
            Requested = value;
            Applied = null;
        }
    }

    internal class PortVoltageConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is AutoPortVoltage portVoltage)
            {
                if (portVoltage.Requested.HasValue)
                {
                    return string.Format($"{portVoltage.Requested:0.0}");
                }
                else if (portVoltage.Applied.HasValue)
                {
                    return $"{portVoltage.Applied:0.0} (Auto)";
                }
                else
                {
                    return "";
                } 
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {

                if (string.IsNullOrEmpty(stringValue))
                {
                    return new AutoPortVoltage(null);
                }
                else if (double.TryParse(stringValue, NumberStyles.Number, culture ?? CultureInfo.CurrentCulture, out double result))
                {
                    return new AutoPortVoltage(result);
                }

                throw new FormatException($"'{stringValue}' is not a valid value for PortVoltage.");
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
