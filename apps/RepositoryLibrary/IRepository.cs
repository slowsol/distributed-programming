using System.Collections.Generic;

namespace RepositoryLibrary
{
    public interface IRepository
    {
        string Get(string key, string shardKey);
        string GetRegionName(string shardKey);

        void Save(string key, string value, string shardKey);
        void SaveShard(string key, string regionId);

        bool IsKeyExist(string key, string shardKey);
        bool IsValueExist(string prefix, string value);
    }
}
