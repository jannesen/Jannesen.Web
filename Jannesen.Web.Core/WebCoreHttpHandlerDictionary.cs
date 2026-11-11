using System;
using System.Collections.Generic;

namespace Jannesen.Web.Core;

internal class WebCoreHttpHandlerDictionary
{
    private sealed class VerbDictionary
    {
        private readonly            Dictionary<string, WebCoreHttpHandler>      _exact;
        private readonly            List<WebCoreHttpHandler>                    _wildcard;

        public                                                              VerbDictionary()
        {
            _exact      = new Dictionary<string,WebCoreHttpHandler>(4096);
            _wildcard   = new List<WebCoreHttpHandler>(1024);
        }

        public                  void                                        Add(WebCoreHttpHandler httpHandler)
        {
            if (httpHandler.WildcardPathProcessor == null)
                _exact.Add(httpHandler.Path, httpHandler);
            else {
                var i = 0;

                while (i < _wildcard.Count && _wildcard[i].WildcardPathProcessor!.Prefix.Length >= httpHandler.WildcardPathProcessor.Prefix.Length) {
                    ++i;
                }

                _wildcard.Insert(i, httpHandler);
            }
        }
        public                  WebCoreHttpHandler?                         GetHandler(string path)
        {
            {
                if (_exact.TryGetValue(path, out var rtn)) {
                    return rtn;
                }
            }

            {
                for(var i = 0 ; i < _wildcard.Count ; ++i) {
                    if (_wildcard[i].WildcardPathProcessor!.IsMatch(path)) {
                        return _wildcard[i];
                    }
                }
            }

            return null;
        }
    }

    private readonly        Dictionary<string, VerbDictionary>          _dictionary;

    public                                                              WebCoreHttpHandlerDictionary()
    {
        _dictionary = new Dictionary<string, VerbDictionary>(32);
    }

    public                  void                                        Add(WebCoreHttpHandler httpHandler)
    {
        ArgumentNullException.ThrowIfNull(httpHandler);

        if (!_dictionary.TryGetValue(httpHandler.Verb, out var verbDictionary))
            _dictionary.Add(httpHandler.Verb, verbDictionary = new VerbDictionary());

        verbDictionary.Add(httpHandler);
    }
    public                  WebCoreHttpHandler?                         GetHandler(string path, string verb)
    {
        if (!_dictionary.TryGetValue((verb != "HEAD" ? verb : "GET"), out var verbDictionary)) {
            return null;
        }

        return verbDictionary.GetHandler(path);
    }
}
