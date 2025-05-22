using System.Collections.Concurrent;

namespace SATS.AI.Chat;

public class ChatStore
{
    private readonly ConcurrentDictionary<string, CachedChat> _dict = [];

    public bool Contains(string key)
    {
        return _dict.TryGetValue(key, out _);
    }
    
    public CachedChat? Get(string key)
    {
        if (_dict.TryGetValue(key, out CachedChat? cachedChat))
        {
            return cachedChat;
        }

        return null;
    }

    public List<CachedChat> GetAll()
    {
        return [.. _dict.Values];
    }

    public void Set(string key, CachedChat cachedChat)
    {
        _dict.TryAdd(key, cachedChat);
    }
}

