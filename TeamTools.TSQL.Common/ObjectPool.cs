// src: https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-create-an-object-pool
using System;
using System.Collections.Concurrent;

namespace TeamTools.Common.Linting
{
    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> objects = new ConcurrentBag<T>();
        private readonly Func<T> objectGenerator;
        private readonly Action<T> objectCleanup;

        public ObjectPool(Func<T> objectGenerator, Action<T> objectCleanup)
        {
            this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            this.objectCleanup = objectCleanup ?? throw new ArgumentNullException(nameof(objectCleanup));
        }

        public T Get() => objects.TryTake(out T item) ? item : objectGenerator();

        public void Return(T item)
        {
            objectCleanup(item);
            objects.Add(item);
        }
    }
}
