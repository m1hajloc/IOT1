using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GrpcService1.Models
{
    public class TemperatureReadingModel
    {
        [BsonId]
        public ObjectId _id { get; set; }
        [BsonElement("id")]
        public String id { get; set; }

        [BsonElement("room_id/id")]
        public String room_id { get; set; }
        public String noted_date { get; set; }
        public int temp { get; set; }

        [BsonElement("out/in")]
        public String outin { get; set; }
    }
}
