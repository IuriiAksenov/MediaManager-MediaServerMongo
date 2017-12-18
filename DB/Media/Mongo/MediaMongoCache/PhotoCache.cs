using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger;

namespace MediaMongoCache
{
    public class PhotoCache
    {
        private struct Item
        {
            public byte[] _image;
            public DateTime _lastTocuchedTime;

            public Item(byte[] image, DateTime lastTouchedTime)
            {
                _image = image;
                _lastTocuchedTime = lastTouchedTime;
            }
        }

        private readonly int _capacity = 0;
        private int _size = 0;
        private readonly ConcurrentDictionary<int, Item> _cache;
        private readonly object _sync = new object();

        public PhotoCache() : this(10) { }
        public PhotoCache(int capacity)
        {
            Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(PhotoCache)}: Constructor is starting");
            _capacity = capacity;
            _cache = new ConcurrentDictionary<int, Item>();
            Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(PhotoCache)}: Constructor is completed");
        }

        public byte[] AddOrUpdate(int productId, byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(AddOrUpdate)}: AddOrUpdate is starting. {nameof(productId)}:{productId}");

                if (_size + 1 > _capacity)
                {
                    lock (_sync)
                    {
                        if (_size + 1 > _capacity)
                        {
                            var oldItems = _cache.OrderBy(x => x.Value._lastTocuchedTime);
                            for (int i = 0; i < oldItems.Count() / 2; i++)
                            {
                                var isDeleted = TryRemove(oldItems.ElementAt(i).Key);
                                if (isDeleted) _size--;
                            }
                        }
                    }
                }
                Interlocked.Increment(ref _size);
                Item item = new Item(image, DateTime.Now);
                var result = _cache.AddOrUpdate(productId, item, (x, y) => y)._image;
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(AddOrUpdate)}: AddOrUpdate is finished. {nameof(productId)}:{productId}");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoCache)}.{nameof(AddOrUpdate)}: AddOrUpdate Error. {nameof(productId)}:{productId}");
                return new byte[0];
            }
        }

        public bool IsExist(int productId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(IsExist)}: IsExist is starting. {nameof(productId)}:{productId}");
                var result = _cache.Any(x => x.Key == productId);
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(IsExist)}: IsExist is completed. {nameof(productId)}:{productId}");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoCache)}.{nameof(IsExist)}: IsExist Error. {nameof(productId)}:{productId}");
                return false;
            }
        }

        public bool TryGetValue(int productId, out byte[] image)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryGetValue)}: TryGetValue is starting. {nameof(productId)}:{productId}");
                Item item = new Item();
                var result = _cache.TryGetValue(productId, out item);
                image = item._image;
                if ((item._image?.Length ?? -1) > 0)
                {
                    Item newItem = new Item(item._image, DateTime.Now);
                    _cache.TryUpdate(productId, newItem, item);
                    Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryGetValue)}: TryGetValue is completed. {nameof(productId)}:{productId}");
                    return true;
                }
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryGetValue)}: TryGetValue is completed. {nameof(productId)}:{productId}");
                return false;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoCache)}.{nameof(TryGetValue)}: TryGetValue Error. {nameof(productId)}:{productId}");
                image = new byte[0];
                return false;
            }
        }

        public bool TryUpdate(int productId, byte[] newImage)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryUpdate)}: TryUpdate is starting. {nameof(productId)}:{productId}");
                Item oldItem = new Item();
                Item newItem = new Item(newImage, DateTime.Now);
                _cache.TryGetValue(productId, out oldItem);
                var result =  _cache.TryUpdate(productId, newItem, oldItem);
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryUpdate)}: TryUpdate is completed. {nameof(productId)}:{productId}");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoCache)}.{nameof(TryUpdate)}: TryUpdate Error. {nameof(productId)}:{productId}");
                return false;
            }
        }

        public bool TryRemove(int productId)
        {
            try
            {
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryRemove)}: TryRemove is starting. {nameof(productId)}:{productId}");
                Item item = new Item();
                var result =  _cache.TryRemove(productId, out item);
                Log.Instance.LogAsInfo($"{nameof(PhotoCache)}.{nameof(TryRemove)}: TryRemove is completed. {nameof(productId)}:{productId}");
                return result;
            }
            catch (Exception e)
            {
                Log.Instance.ExceptionInfo(e).LogAsError($"{nameof(PhotoCache)}.{nameof(TryRemove)}: TryRemove Error. {nameof(productId)}:{productId}");
                return false;
            }
        }

    }
}
