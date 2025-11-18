using System;

namespace TeamTools.Common.Linting
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DataTypeAttribute : Attribute
    {
        public DataTypeAttribute(string dataTypeName)
        {
            if (string.IsNullOrEmpty(dataTypeName))
            {
                throw new ArgumentNullException(nameof(dataTypeName));
            }

            this.DataTypeName = dataTypeName;
        }

        public string DataTypeName { get; }
    }
}
