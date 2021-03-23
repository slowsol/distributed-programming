using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RepositoryLibrary
{
    public class RedisRepository : IRepository
    {
        private readonly ConnectionMultiplexer _redis;

        private readonly ILogger<IRepository> _logger;

        private IDatabase _db
        {
            get
            {
                return _redis.GetDatabase();
            }
        }

        private IServer _server
        {
            get
            {
                return _redis.GetServer(System.Environment.GetEnvironmentVariable("REDIS_HOST") 
                    + ":" + System.Environment.GetEnvironmentVariable("REDIS_PORT"));
            }
        }

        public RedisRepository(ILogger<IRepository> logger)
        {
            _redis = ConnectionMultiplexer.Connect(System.Environment.GetEnvironmentVariable("REDIS_HOST"));

            _logger = logger;
        }

        public string Get(string key)
        {
            return _db.StringGet(key);
        }

        public List<string> GetAllByPrefix(string prefix)
        {
            var values = _server.Keys(pattern: prefix + "*").ToList();

            return values.ConvertAll(v => Get((string)v));
        }

        public void Save(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                _logger.LogDebug("Failed to save text. Key or value is null or empty.");

                return;
            }

            _logger.LogDebug("Save {Key}: {Value} to db.", key, value);

            _db.StringSet(key, value);
        }
    }
}