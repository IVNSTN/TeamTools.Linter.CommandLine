using System;

namespace TeamTools.Common.Linting
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RuleIdentityAttribute : Attribute
    {
        public RuleIdentityAttribute(string id, string mnemo)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrEmpty(mnemo))
            {
                throw new ArgumentNullException(nameof(mnemo));
            }

            Id = id;
            Mnemo = mnemo;
            FullName = $"{id}{IdSeparator}{mnemo}";
        }

        public static string IdSeparator { get; } = ":";

        public string FullName { get; }

        public string Id { get; }

        public string Mnemo { get; }
    }
}
