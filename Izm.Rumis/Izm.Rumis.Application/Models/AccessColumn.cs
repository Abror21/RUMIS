using System;

namespace Izm.Rumis.Application.Models
{
    public class AccessColumn
    {
        public string Name { get; }
        public string Type { get; }

        /// <summary>
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type - see https://docs.microsoft.com/en-us/sql/odbc/microsoft/microsoft-access-data-types for available types</param>
        public AccessColumn(string name, string type)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));

            Name = name;
            Type = type;
        }
    }
}
