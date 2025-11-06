using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface IVcsAccessor
    {
        IEnumerable<string> GetModifiedFiles(string folder);
    }
}
