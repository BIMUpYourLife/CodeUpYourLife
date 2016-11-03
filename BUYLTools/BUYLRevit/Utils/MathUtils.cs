using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUYLRevit.Utils
{
    public static class MathUtils
    {
        public static double DegreeToRad(double degreeAngle)
        {
            return degreeAngle * 0.0174533;
        }

        public static double MMToFeet(double mm)
        {
            return mm * 0.00328084;
        }
    }
}
