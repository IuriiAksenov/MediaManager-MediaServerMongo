using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MediaSQL
{
    public struct AppCache
    {
        public static int sizeMb = 50;

        private static int size = sizeMb * 1024 * 1024;
        private static ConcurrentDictionary<int, byte[]> appDictionary = new ConcurrentDictionary<int, byte[]>();
        private static ConcurrentQueue<int> cacheCounter = new ConcurrentQueue<int>();
        private static int currentFull = 0;
        


        public static void Add(int productId, byte[] photo)
        {
            while (currentFull + photo.Length >= size)
            {
                int oldId;
                if (!cacheCounter.TryDequeue(out oldId))
                    throw new Exception("The insertion of photo failed");

                DeletePhoto(oldId);
            }

            if (!appDictionary.TryAdd(productId, photo))
                throw new Exception("The insertion of photo failed");
            cacheCounter.Enqueue(productId);

            Interlocked.Add(ref currentFull, photo.Length);
        }

        public static byte[] FindPhoto(int productId)
        {

            if (appDictionary.TryGetValue(productId, out byte[] value))
                return value;
            else
                return null;
        }

        public static void DeletePhoto(int product_id)
        {
            Interlocked.Add(ref currentFull, -appDictionary[product_id].Length);
            byte[] temp;
            if (!appDictionary.TryRemove(product_id, out temp))
                throw new Exception("This photo id already deleted");
        }

        //todo cacheCounter.Clear(); не находит эту строчку
        public static void ClearCache()
        {
            object obj = new object();
            lock (obj)
            {
                appDictionary.Clear();
                cacheCounter = new ConcurrentQueue<int>(); // Dmitry Belkin BUG: clear the queue
                currentFull = 0;
            }
        }

}
}

