using System;

namespace EthanLibrary.Web.CacheManage.Memcached
{
    public class MemcachedClientException : ApplicationException
    {
        public MemcachedClientException(string message) : base(message)
        {
        }

        public MemcachedClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}