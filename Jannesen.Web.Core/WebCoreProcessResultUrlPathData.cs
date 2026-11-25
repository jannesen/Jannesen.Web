using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jannesen.Web.Core;

public sealed class WebCoreProcessResultUrlPathData
{
    private record struct NameValue(string Name, string Value);

    private readonly    NameValue[] _nameValues;

    public              string      this[string name]
    {
        get {
            foreach(var item in _nameValues) {
                if (item.Name == name) {
                    return item.Value;
                }
            }
            throw new KeyNotFoundException($"Unknown urlpath label '{name}'.");
        }
    }

    internal                        WebCoreProcessResultUrlPathData()
    {
        _nameValues = [];
    }
    internal                        WebCoreProcessResultUrlPathData(IReadOnlyList<string> names, Match match)
    {
        _nameValues = new NameValue[names.Count];

        for(var i = 0; i < names.Count; i++) {
            _nameValues[i] = new NameValue() {
                                 Name  = names[i],
                                 Value = match.Groups[names[i]].Value
                             };
        }
    }
}
