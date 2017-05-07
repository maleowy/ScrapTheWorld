using LiteDB;

namespace Persistence
{
    public class KeyValue
    {
        [BsonId]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
