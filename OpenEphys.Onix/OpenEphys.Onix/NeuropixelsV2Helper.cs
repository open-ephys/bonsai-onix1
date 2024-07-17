using System;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix
{
    public class NeuropixelsV2Helper
    {
        public static double ParseGainCalibrationFile(StreamReader file)
        {
            if (file != null)
            {
                var gainCalibration = file.ReadLine().Split(',').Skip(1).FirstOrDefault();

                if (double.TryParse(gainCalibration, out var gain))
                {
                    return gain;
                }
                else
                {
                    throw new FormatException($"Gain calibration file is improperly formatted: `{gainCalibration}` is an invalid line.");
                }
            }

            throw new ArgumentNullException("Invalid stream reader, check that the file name is correct.");
        }
    }
}
