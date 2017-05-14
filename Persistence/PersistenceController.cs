using System;
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
        private static readonly string DbName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DB.db");

        [HttpGet]
        public IEnumerable<KeyValue> Get(string table)
        {
            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);
                return collection.FindAll();
            }
        }

        [HttpGet]
        public KeyValue Get(string table, string key)
        {
            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);
                return collection.FindById(key);
            }
        }

        [HttpPost]
        public void Post(string table, string key, [FromBody]string value)
        {
            var content = Request.Content.ReadAsStringAsync().Result;

            using (var db = new LiteDatabase(DbName))
            {
                var collection = db.GetCollection<KeyValue>(table);

                KeyValue fromDb = collection.FindById(key);

                if (fromDb == null)
                {
                    collection.Insert(new KeyValue {Key = key, Value = value ?? content});
                }
                else
                {
                    fromDb.Value = value ?? content;
                    collection.Update(fromDb);
                }
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
            var indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
            var indexHtml = File.ReadAllText(indexPath).Replace("localhost:8081", $"{Program.IP}:{Program.Port}").Replace("table=test", $"table={Program.Table}");

            var response = new HttpResponseMessage { Content = new StringContent(indexHtml) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }
    }
}
