using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreWildcardPathProcessor
    {
        private class Parser
        {
            public      string                  expression;
            public      int                     length;
            public      int                     pos;
            public      List<string>            names;
            public      StringBuilder           regex;

            public                              Parser(string expression)
            {
                this.expression = expression;
                this.length     = expression.Length;
            }

            public      void                    Process()
            {
                pos   = 0;
                names = new List<string>();
                regex = new StringBuilder();

                regex.Append("^");

                while (pos < expression.Length) {
                    _process_nonwildcard();

                    _expectchar('{');
                        regex.Append("(?<");
                        string name = _process_get(c => ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '_')));
                        names.Add(name);
                        regex.Append(name);
                        regex.Append(">");

                    _expectchar(':');
                    _process_wildcard();

                    _expectchar('}');
                    regex.Append("?)");
                }

                regex.Append("$");
            }

            private     void                    _expectchar(char c)
            {
                if (pos >= length || expression[pos] != c)
                    throw new FormatException("Invalid wildcard path expression.");

                ++pos;
            }
            private     void                    _process_nonwildcard()
            {
                while (pos < length && expression[pos] != '{')
                    _regex_copyandescapechar();
            }
            private     string                  _process_get(Func<char,bool> filter)
            {
                StringBuilder   rtn = new StringBuilder();

                while (filter(expression[pos]))
                    rtn.Append(expression[pos++]);

                return rtn.ToString();
            }
            private     void                    _process_copy(Func<char,bool> filter)
            {
                while (filter(expression[pos]))
                    regex.Append(expression[pos++]);
            }
            private     void                    _process_wildcard()
            {
                while (pos < length && expression[pos] != '}') {
                    if (expression[pos] == '\\') {
                        ++pos;
                        _regex_copyandescapechar();
                    }
                    else
                    if (expression[pos] == '[') {
                        regex.Append('[');
                            ++pos;

                            while (pos < length) {
                                char c = expression[pos];

                                if (c == '\\') {
                                    ++pos;
                                    _regex_copyandescapechar();
                                }
                                else
                                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-' || c == ' ') {
                                    regex.Append(c);
                                    ++pos;
                                }
                                else
                                    break;
                            }

                            regex.Append(']');
                        _expectchar(']');

                        _expectchar('{');
                            regex.Append('{');
                            _process_copy(c => (c >= '0' && c <= '9'));

                            if (pos < length && expression[pos] == ',') {
                                ++pos;
                                regex.Append(',');
                                _process_copy(c => (c >= '0' && c <= '9'));
                            }

                            regex.Append('}');
                        _expectchar('}');
                    }
                    else
                    if (expression[pos] == '*') {
                        regex.Append(".*");
                        ++pos;
                    }
                    else
                        _regex_copyandescapechar();
                }
            }
            private     void                    _regex_copyandescapechar()
            {
                if (pos >= length)
                    throw new FormatException("Invalid wildcard path expression.");

                regex.Append(@"\u");
                regex.Append(((Int16)expression[pos++]).ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private readonly        string                              _prefix;
        private readonly        string                              _suffix;
        private readonly        string                              _expression;
        private readonly        string[]                            _names;
        private readonly        Regex                               _regex;

        public                  string                              Prefix
        {
            get {
                return _prefix;
            }
        }
        public                  string                              Suffix
        {
            get {
                return _suffix;
            }
        }
        public                  string                              Expression
        {
            get {
                return _expression;
            }
        }
        public                  string[]                            Names
        {
            get {
                return _names;
            }
        }

        public                                                      WebCoreWildcardPathProcessor(string prefix, string suffix, string expression)
        {
            _prefix     = prefix;
            _suffix     = suffix;
            _expression = expression;

            if (expression.Length == 1 && expression == "*")
                return ; // Simple wildcard

            if (expression.Length >= 2 && expression[0] == '{' && expression[expression.Length - 1] == '}') {
                Parser parser = new Parser(expression);
                parser.Process();
                _names = parser.names.ToArray();
                _regex = new Regex(parser.regex.ToString(), RegexOptions.Compiled | RegexOptions.Singleline);
                return ; // Complex wildcard
            }

            throw new FormatException("Invalid wildcard path.");
        }

        public      static      WebCoreWildcardPathProcessor        GetProcessor(string path)
        {
            int     asteriskBegin   = path.IndexOf('*');
            int     asteriskEnd     = path.LastIndexOf('*');
            int     braceBegin      = path.IndexOf('{');
            int     braceEnd        = path.LastIndexOf('}');

            if ((asteriskBegin >= 0 || braceBegin >= 0) && (asteriskEnd >= 0 || braceEnd >= 0)) {
                int     begin = asteriskBegin;
                int     end   = asteriskEnd;

                if (braceBegin >= 0 && (begin == -1 || braceBegin < begin))     begin = braceBegin;
                if (braceEnd   >= 0 && (end   == -1 || braceEnd   > end  ))     end   = braceEnd;

                return new WebCoreWildcardPathProcessor(path.Substring(0, begin), path.Substring(end + 1), path.Substring(begin, (end + 1 - begin)));
            }

            return null;
        }

        public                  bool                                IsMatch(string path)
        {
            if (path.Length - _prefix.Length - _suffix.Length >= 0 &&
                path.StartsWith(_prefix, StringComparison.Ordinal) && path.EndsWith(_suffix, StringComparison.Ordinal))
            {
                if (_regex == null)
                    return true;

                if (_regex.IsMatch(path.Substring(_prefix.Length, path.Length - _prefix.Length - _suffix.Length)))
                    return true;
            }

            return false;
        }

        public                  Match                               RegexMatch(string path)
        {
            if (_regex != null)
                return _regex.Match(path, _prefix.Length, path.Length - _prefix.Length - _suffix.Length);

            return null;
        }
    }
}
