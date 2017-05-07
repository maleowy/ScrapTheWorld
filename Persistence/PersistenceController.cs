using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using LiteDB;

namespace Persistence
{
    public class PersistenceController : ApiController
    {
        private static readonly string DbName = "DB.db";

        [HttpGet]
        public IEnumerable<KeyValue> Get(string table)
        {
            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);
                return collection.FindAll();
            }
        }

        [HttpPost]
        public void Post(string table, string key, [FromBody]string value)
        {
            var content = Request.Content.ReadAsStringAsync().Result;

            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);
                collection.Insert(new KeyValue { Key = key, Value = value ?? content });
            }
        }

        [HttpPut]
        public void Put(string table, string key, [FromBody]string value)
        {
            var content = Request.Content.ReadAsStringAsync().Result;

            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);

                KeyValue fromDb = collection.FindById(key);
                fromDb.Value = value ?? content;

                collection.Update(fromDb);
            }
        }

        [HttpDelete]
        public void Delete(string table, string key)
        {
            using (var db = new LiteDatabase(DbName))
            {
                var values = db.GetCollection<KeyValue>(table);
                values.Delete(key);
            }
        }

        [HttpGet]
        public HttpResponseMessage Get()
        {
            var response = new HttpResponseMessage { Content = new StringContent(File.ReadAllText("index.html")) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }
    }
}
