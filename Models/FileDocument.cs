using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileService.Api.Models
{
    public class FileDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; }

        [BsonElement("originalFileName")]
        public string OriginalFileName { get; set; }

        [BsonElement("serviceName")]
        public string ServiceName { get; set; }

        [BsonElement("extension")]
        public string Extension { get; set; }

        [BsonElement("contentType")]
        public string ContentType { get; set; }

        [BsonElement("size")]
        public long Size { get; set; }

        [BsonElement("path")]
        public string Path { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}