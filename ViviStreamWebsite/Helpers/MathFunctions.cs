using System;

namespace ViviStreamWebsite.Helpers
{
    public static class MathFunctions
    {
        public static int GetCooldownMinutes(DateTime start)
        {
            TimeSpan span = DateTime.UtcNow.Subtract(start);
            return (int)Math.Round(span.TotalMinutes);
        }
    }
}
