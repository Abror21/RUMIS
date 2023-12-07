using System;

namespace Izm.Rumis.Domain.Attributes
{
    /// <summary>
    /// Specifies a classifier type a property is associated with.
    /// Should be applied on foreign key property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ClassifierTypeAttribute : Attribute
    {
        public string Value { get; }
        public bool IsGroup { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Classifier type</param>
        /// <param name="isGroup">Set to true if classifier type is a group. 
        /// It means that related property allows any classifier type from the specified group.</param>
        public ClassifierTypeAttribute(string value, bool isGroup = false)
        {
            this.Value = value;
            this.IsGroup = isGroup;
        }
    }
}
