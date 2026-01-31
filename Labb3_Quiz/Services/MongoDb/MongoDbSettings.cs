using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Services.MongoDb
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; init; } = "mongodb://localhost:27017";

        public string DatabaseName { get; init; } = "MelvinEdlund";


        public static MongoDbSettings FromEnvironment()
        {
            var cs = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            var db = Environment.GetEnvironmentVariable("MONGODB_DATABASE");

            return new MongoDbSettings
            {
                ConnectionString = string.IsNullOrWhiteSpace(cs) ? "mongodb://localhost:27017" : cs,
                DatabaseName = string.IsNullOrWhiteSpace(db) ? "MelvinEdlund" : db
            };
        }
    }
}
