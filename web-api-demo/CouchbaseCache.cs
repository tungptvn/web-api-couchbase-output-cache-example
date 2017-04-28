using Couchbase;
using Couchbase.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using WebApi.OutputCache.Core.Cache;

namespace web_api_demo
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
          
            var stop_watch = new Stopwatch();
            stop_watch.Start();
            logger.Debug(string.Format("=== begin Add ({0},{1},{2}) ", key, expiration, dependsOnKey));
            var expi = ToExpiration(expiration.DateTime);
            var retval = _bucket.Get<object>(key).Value;

            if (retval == null)
            {
                if (retval is byte[])
                {
                    var ms = new MemoryStream();
                    new BinaryFormatter().Serialize(ms, o);
                    _bucket.Insert(key, ms, expi);
                }
                else
                {
                    _bucket.Insert(key, o, expi);
                }
            }
            logger.Debug("=== end Add");
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
            var result = _bucket.Get<T>(key);
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
        private static uint ToExpiration(
           DateTime utcExpiry)
        {
            return (uint)(utcExpiry -DateTime.Now).TotalSeconds;
        }

    }
}