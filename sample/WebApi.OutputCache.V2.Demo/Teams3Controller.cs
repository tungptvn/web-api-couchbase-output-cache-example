using Couchbase;
using Couchbase.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Http;
using System.Web.Http.Controllers;
using WebApi.OutputCache.V2.TimeAttributes;

namespace WebApi.OutputCache.V2.Demo
{
    public class Teams3Controller : ApiController
    {
        private ICluster _cluster;
        private IBucket _bucket;
        public Teams3Controller()
        {
            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket("WebapiCaheDemo");
        }
        private static readonly List<Team> Teams = new List<Team>
            {
                new Team {Id = 1, League = "NHL", Name = "Leafs"},
                new Team {Id = 2, League = "NHL", Name = "Habs"},
            };

        //[CacheOutput(ClientTimeSpan = 50, ServerTimeSpan = 50)]
        public object Get()
        {
            var result = _bucket.Get<byte[]>("webapi.outputcache.v2.demo.teams2controller-get:application/json; charset=utf-8").Value;
            MemoryStream ms = new MemoryStream(result);
            
                IFormatter br = new BinaryFormatter();
                var str =  (object)br.Deserialize(ms);
                return str;
            
            //return Content(HttpStatusCode.OK, result);
        }

        [CacheOutputUntil(2016, 7, 20)]
        public Team GetById(int id)
        {
            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            return team;
        }

        [InvalidateCacheOutput("Get")]
        public void Post(Team value)
        {
            if (!ModelState.IsValid) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            Teams.Add(value);
        }

        public void Put(int id, Team value)
        {
            if (!ModelState.IsValid) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            team.League = value.League;
            team.Name = value.Name;

            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((TeamsController t) => t.GetById(0)));
        }

        public void Delete(int id)
        {
            var team = Teams.FirstOrDefault(i => i.Id == id);
            if (team == null) throw new HttpResponseException(HttpStatusCode.NotFound);

            Teams.Remove(team);
        }
    }
}
