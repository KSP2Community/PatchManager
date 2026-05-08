using System;
using System.Collections.Generic;
using System.Linq;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// Holds a pattern for wildcard matching on asset names
/// </summary>
public sealed class NamePattern
{
    private static readonly Dictionary<string, NamePattern> Cache = new();
    private static readonly char[] Wildcards = { '*', '?' };

    private readonly string _pattern;
    private readonly bool _matchAll;
    private readonly bool _isLiteral;

    /// <summary>
    /// Get a pattern instance for a pattern
    /// </summary>
    /// <param name="pattern">The pattern</param>
    /// <returns>The instance</returns>
    public static NamePattern Get(string pattern)
    {
        if (Cache.TryGetValue(pattern, out var result))
        {
            return result;
        }
        return Cache[pattern] = new NamePattern(pattern);
    }

    private NamePattern(string pattern)
    {
        _pattern = pattern;
        _matchAll = pattern == "*";
        _isLiteral = Wildcards.All(x => !_pattern.Contains(x));
    }

    /// <summary>
    /// Does name match the pattern?
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns><c>true</c> if name matches the pattern, <c>false</c> otherwise</returns>
    public bool Matches(string name)
    {
        if (_matchAll) return true;
        if (_isLiteral) return string.Equals(name, _pattern, StringComparison.Ordinal);

        // Standard quick pattern matching algorithm :3
        var i = 0;
        var j = 0;
        var starJ = -1;
        var starI = -1;
        while (i < name.Length)
        {
            if (j < _pattern.Length && (_pattern[j] == '?' || _pattern[j] == name[i]))
            {
                i++; j++;
            }
            else if (j < _pattern.Length && _pattern[j] == '*')
            {
                starJ = j;
                starI = i;
                j++;
            }
            else if (starJ != -1)
            {
                j = starJ + 1;
                starI++;
                i = starI;
            }
            else
            {
                return false;
            }
        }
        while (j < _pattern.Length && _pattern[j] == '*') j++;
        return j == _pattern.Length;
    }
}