using Couchbase;
using Couchbase.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Demo
{
    class CouchbaseCache : IApiOutputCache
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ICluster _cluster;
        private IBucket _bucket;
        public CouchbaseCache()
        {
            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket("WebapiCaheDemo");
        }
        public IEnumerable<string> AllKeys
        {
            get
            {
                var keyList = new List<string>();
                var query = _bucket.CreateQuery("dev_WebapiCacheDemoView", "WebapiCacheDemoView", false);
                var result = _bucket.Query<dynamic>(query);
                foreach (var row in result.Rows)
                {
                    keyList.Add(row.Key);
                }

                return keyList;
            }
        }

        public void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
          
            var expi = ToExpiration(expiration.DateTime);
         
            var retval = _bucket.Get<object>(key).Value;

            if (retval == null)
            {
                _bucket.Insert(key, o, expi);
                retval = o;
            }

            return;
           
        }
        public bool Contains(string key)
        {
            var keyList = new List<string>();
            var query = _bucket.CreateQuery("dev_WebapiCacheDemoView", "WebapiCacheDemoView", false);
            var result = _bucket.Query<dynamic>(query);
            foreach (var row in result.Rows)
            {
                keyList.Add(row.Key);
            }

            return keyList.Contains(key);
        }

        public object Get(string key)
        {

            var result = _bucket.Get<object>(key);
            return result.Value;
        }

        public T Get<T>(string key) where T : class
        {
            var result = _bucket.Get<object>(key);
            return result.Value as T;
        }

        public void Remove(string key)
        {
            _bucket.Remove(key);
        }

        public void RemoveStartsWith(string key)
        {
            _bucket.Remove(key);

        }

       
        private static uint ToExpiration(DateTime utcExpiry)
        {
            return (uint)(DateTime.SpecifyKind(utcExpiry, DateTimeKind.Utc) - DateTime.UtcNow).TotalSeconds;
        }
    }
}
