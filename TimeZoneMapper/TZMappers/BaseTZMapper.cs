﻿namespace TimeZoneMapper.TZMappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    ///     Provides a base class for TimeZoneMapper objects.
    /// </summary>
    public abstract class BaseTZMapper
    {
        private readonly Dictionary<string, TimeZoneInfo> _mappings;

        /// <summary>
        /// Gets the TimeZoneID version part of the resource currently in use.
        /// </summary>
        /// <remarks>This value corresponds to the &quot;typeVersion&quot; attribute of the resource data.</remarks>
        public string TZIDVersion { get; private set; }

        /// <summary>
        /// Gets the TimeZoneInfo version part of the resource currently in use.
        /// </summary>
        /// <remarks>This value corresponds to the &quot;otherVersion&quot; attribute of the resource data.</remarks>
        public string TZVersion { get; private set; }

        /// <summary>
        /// Gets the version of the resource currently in use.
        /// </summary>
        /// <remarks>This value is a composite of &quot;<see cref="TZIDVersion"/>.<see cref="TZVersion"/>&quot;.</remarks>
        public string Version { get; private set; }

        internal BaseTZMapper(string xmldata)
        {
            var root = XDocument.Parse(xmldata).Descendants("mapTimezones").First();
            _mappings = root.Descendants("mapZone")
                .Where(n => !n.Attribute("territory").Value.Equals("001"))
                .SelectMany(n => n.Attribute("type").Value.Split(new[] { ' ' }), (n, t) => new { TZID = t, TZ = TimeZoneInfo.FindSystemTimeZoneById(n.Attribute("other").Value) })
                .OrderBy(n => n.TZID)
                .ToDictionary(n => n.TZID, v => v.TZ, StringComparer.OrdinalIgnoreCase);

            this.TZIDVersion = root.Attribute("typeVersion").Value;
            this.TZVersion = root.Attribute("otherVersion").Value;
            this.Version = string.Format("{0}.{1}", this.TZIDVersion, this.TZVersion);
        }

        /// <summary>
        ///     Maps a TimeZone ID (e.g. "Europe/Amsterdam") to a corresponding TimeZoneInfo object.
        /// </summary>
        /// <param name="tzid">The TimeZone ID (e.g. "Europe/Amsterdam").</param>
        /// <returns>Returns a .Net BCL <see cref="TimeZoneInfo"/> object corresponding to the TimeZone ID.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the specified TimeZone ID is not found.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the specified TimeZone ID is null.</exception>
        public TimeZoneInfo MapTZID(string tzid)
        {
            return _mappings[tzid];
        }

        /// <summary>
        ///     Builds an array of available TimeZone ID's and returns these as an array.
        /// </summary>
        /// <returns>Returns an array of all available ('known') TimeZone ID's.</returns>
        public string[] GetAvailableTZIDs()
        {
            return _mappings.Keys.ToArray();
        }

        /// <summary>
        ///     Builds an array of available <see cref="TimeZoneInfo"/> objects that the mapper can return.
        /// </summary>
        /// <returns>Returns an array of available <see cref="TimeZoneInfo"/> objects that the mapper can return.</returns>
        public TimeZoneInfo[] GetAvailableTimeZones()
        {
            return _mappings.Values.Distinct().ToArray();
        }
    }
}