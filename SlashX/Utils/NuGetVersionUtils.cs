using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Utils
{
    internal class NuGetVersionUtils
    {
        public static VersionRange? Intersect(VersionRange a, VersionRange b)
        {
            if (a == null || b == null)
                return null;

            NuGetVersion? minVersion;
            bool includeMin;

            if (a.MinVersion != null && b.MinVersion != null)
            {
                int cmp = a.MinVersion.CompareTo(b.MinVersion);
                if (cmp > 0)
                {
                    minVersion = a.MinVersion;
                    includeMin = a.IsMinInclusive;
                }
                else if (cmp < 0)
                {
                    minVersion = b.MinVersion;
                    includeMin = b.IsMinInclusive;
                }
                else
                {
                    minVersion = a.MinVersion;
                    includeMin = a.IsMinInclusive && b.IsMinInclusive;
                }
            }
            else if (a.MinVersion != null)
            {
                minVersion = a.MinVersion;
                includeMin = a.IsMinInclusive;
            }
            else if (b.MinVersion != null)
            {
                minVersion = b.MinVersion;
                includeMin = b.IsMinInclusive;
            }
            else
            {
                minVersion = null;
                includeMin = true;
            }

            NuGetVersion? maxVersion;
            bool includeMax;

            if (a.MaxVersion != null && b.MaxVersion != null)
            {
                int cmp = a.MaxVersion.CompareTo(b.MaxVersion);
                if (cmp < 0)
                {
                    maxVersion = a.MaxVersion;
                    includeMax = a.IsMaxInclusive;
                }
                else if (cmp > 0)
                {
                    maxVersion = b.MaxVersion;
                    includeMax = b.IsMaxInclusive;
                }
                else
                {
                    maxVersion = a.MaxVersion;
                    includeMax = a.IsMaxInclusive && b.IsMaxInclusive;
                }
            }
            else if (a.MaxVersion != null)
            {
                maxVersion = a.MaxVersion;
                includeMax = a.IsMaxInclusive;
            }
            else if (b.MaxVersion != null)
            {
                maxVersion = b.MaxVersion;
                includeMax = b.IsMaxInclusive;
            }
            else
            {
                maxVersion = null;
                includeMax = true;
            }

            if (minVersion != null && maxVersion != null)
            {
                int cmp = minVersion.CompareTo(maxVersion);
                if (cmp > 0 || (cmp == 0 && (!includeMin || !includeMax)))
                {
                    return null;
                }
            }
            if (minVersion == null && maxVersion == null)
                return VersionRange.All;

            if (maxVersion == null)
                return new VersionRange(minVersion!, includeMin, null, false);

            if (minVersion == null)
                return new VersionRange(null, false, maxVersion, includeMax);

            if (minVersion == maxVersion && includeMin && includeMax)
                return new VersionRange(minVersion);

            return new VersionRange(minVersion, includeMin, maxVersion, includeMax);
        }
    }
}
