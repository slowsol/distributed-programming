using System.Collections.Generic;

namespace RepositoryLibrary
{
    public interface IRepository
    {
        string Get(string key);
        List<string> GetAllByPrefix(string prefix);

        bool IsKeyExist(string key);
        void Save(string key, string value);
    }
}