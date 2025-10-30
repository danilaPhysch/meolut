using EphemerisHub.Models;

namespace EphemerisHub.Infrastructure;

public static class TleExtensions
{
    public static Tle MapToEntity(this Tle tle) =>
        MapByCsSatNum(tle.CsSatNum, tle.Time, tle.Name, tle.Line1, tle.Line2);

    private static Tle MapByCsSatNum(int csSatNum, DateTime time, string name, string line1, string line2) =>
        csSatNum switch
        {
            > 300 and < 400 => new GpsTle { CsSatNum = csSatNum, Time = time, Name = name, Line1 = line1, Line2 = line2 },
            > 400 and < 500 => new GalileoTle { CsSatNum = csSatNum, Time = time, Name = name, Line1 = line1, Line2 = line2 },
            > 500 and < 600 => new GlonassTle { CsSatNum = csSatNum, Time = time, Name = name, Line1 = line1, Line2 = line2 },
            > 600 and < 700 => new BeidouTle { CsSatNum = csSatNum, Time = time, Name = name, Line1 = line1, Line2 = line2 },
            _ => throw new ArgumentOutOfRangeException(nameof(csSatNum), csSatNum, "Unknown CsSatNum range")
        };
}