using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Models
{
    public class Category
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";
    }
}
