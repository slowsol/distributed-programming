using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RepositoryLibrary
{
    public enum Region
    {
        Main,
        Ru,
        Eu,
        Other
    }

    public class RedisRepository : IRepository
    {
        private readonly ConnectionMultiplexer _mainConnection;
        private readonly ConnectionMultiplexer _ruConnection;
        private readonly ConnectionMultiplexer _euConnection;
        private readonly ConnectionMultiplexer _otherConnection;

        private readonly ILogger<IRepository> _logger;

        public RedisRepository(ILogger<IRepository> logger)
        {
            _mainConnection = ConnectionMultiplexer.Connect(GetSocketAddress(Region.Main));
            _ruConnection = ConnectionMultiplexer.Connect(GetSocketAddress(Region.Ru));
            _euConnection = ConnectionMultiplexer.Connect(GetSocketAddress(Region.Eu));
            _otherConnection = ConnectionMultiplexer.Connect(GetSocketAddress(Region.Other));

            _logger = logger;
        }

        public string Get(string key, string shardKey)
        {
            var region = GetRegion(shardKey);

            return GetDatabase(region).StringGet(key);
        }

        public string GetRegionName(string shardKey)
        {
            return GetDatabase(Region.Main).StringGet(shardKey);
        }

        public bool IsKeyExist(string key, string shardKey)
        {
            var region = GetRegion(shardKey);

            return GetDatabase(region).KeyExists(key);
        }

        public bool IsValueExist(string prefix, string value)
        {
            var keys = GetKeysByPrefix(prefix);

            return keys.Any(k => value == GetFromAllRegions(k));
        }

        public void Save(string key, string value, string shardKey)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                _logger.LogDebug("Failed to save text. Key or value is null or empty.");

                return;
            }

            var region = GetRegion(shardKey);

            _logger.LogDebug("Save {Key}: {Value} to db.", key, value);

            GetDatabase(region).StringSet(key, value);
        }

        public void SaveShard(string key, string regionId)
        {
            GetDatabase(Region.Main).StringSet(key, regionId);
        }

        private string GetFromAllRegions(string key)
        {
            foreach (Region region in Region.GetValues(typeof(Region)))
            {
                var db = GetDatabase(region);

                if (db.KeyExists(key))
                {
                    return db.StringGet(key);
                }
            }

            return string.Empty;
        }

        private List<RedisKey> GetKeysByPrefix(string prefix)
        {
            var keys = new List<RedisKey>();

            foreach (Region region in Region.GetValues(typeof(Region)))
            {
                keys.AddRange(GetServerByRegion(region).Keys(pattern: prefix + "*").ToList());
            }

            return keys;
        }

        private Region GetRegion(string shardKey)
        {
            var regionName = GetRegionName(shardKey);

            switch (regionName.ToUpper())
            {
                case "RU":
                    return Region.Ru;
                case "EU":
                    return Region.Eu;
                default:
                    return Region.Other;
            }
        }

        private IDatabase GetDatabase(Region region)
        {
            var connection = GetConnection(region);

            return connection.GetDatabase();
        }

        private IServer GetServerByRegion(Region region)
        {
            var connection = GetConnection(region);
            var socketAddress = GetSocketAddress(region);

            return connection.GetServer(socketAddress);
        }

        private IConnectionMultiplexer GetConnection(Region region)
        {
            switch (region)
            {
                case Region.Main:
                    return _mainConnection;
                case Region.Ru:
                    return _ruConnection;
                case Region.Eu:
                    return _euConnection;
                default:
                    return _otherConnection;
            }
        }

        private string GetSocketAddress(Region region)
        {
            var hostname = "";
            var port = System.Environment.GetEnvironmentVariable("REDIS_PORT");

            switch (region)
            {
                case Region.Main:
                    hostname = System.Environment.GetEnvironmentVariable("REDIS_HOST");

                    break;
                case Region.Ru:
                    hostname = System.Environment.GetEnvironmentVariable("REDIS_RU_HOST");

                    break;
                case Region.Eu:
                    hostname = System.Environment.GetEnvironmentVariable("REDIS_EU_HOST");

                    break;
                case Region.Other:
                    hostname = System.Environment.GetEnvironmentVariable("REDIS_OTHER_HOST");

                    break;
            }

            return hostname + ":" + port;
        }
    }
}