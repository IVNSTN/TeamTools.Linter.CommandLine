using System.Text;

namespace TeamTools.Common.Linting
{
    public static class ObjectPools
    {
        public static ObjectPool<StringBuilder> StringBuilderPool { get; } = new ObjectPool<StringBuilder>(() => new StringBuilder(), obj => obj.Clear());
    }
}
