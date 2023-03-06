using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreWildcardPathProcessor
    {
        private sealed class Parser
        {
            public          string                  expression;
            public          int                     length;
            public          int                     pos;
            public          List<string>            names;
            public          StringBuilder           regex;

            public                                  Parser(string expression)
            {
                this.expression = expression;
                this.length     = expression.Length;

                pos   = 0;
                names = new List<string>();
                regex = new StringBuilder();

                regex.Append("^");

                while (pos < expression.Length) {
                    while (pos < length && expression[pos] != '{') {
                        _copyandescapechar();
                    }

                    _namedglob();
                }

                regex.Append("$");
            }

            private         void                    _namedglob()
            {
                _expectchar('{');
                    regex.Append("(?<");
                    string name = _getname();
                    names.Add(name);
                    regex.Append(name);
                    regex.Append(">");

                _expectchar(':');

                while (pos < length && expression[pos] != '}') {
                    switch(expression[pos]) {
                    case '\\':
                        ++pos;
                        _copyandescapechar();
                        break;

                    case '[':
                        _simple_range();
                        break;

                    case '(':
                        _regex_copy(')');
                        break;

                    case '*':
                        regex.Append(".*");
                        ++pos;
                        break;

                    default:
                        _copyandescapechar();
                        break;
                    }
                }

                _expectchar('}');
                regex.Append("?)");
            }
            private         string                  _getname()
            {
                StringBuilder   rtn = new StringBuilder();

                while (_char_name(expression[pos]))
                    rtn.Append(expression[pos++]);

                return rtn.ToString();
            }
            private         void                    _simple_range()
            {
                regex.Append('[');
                    ++pos;

                    while (pos < length) {
                        char c = expression[pos];

                        if (c == '\\') {
                            ++pos;
                            _copyandescapechar();
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
                    _copydigits();

                    if (pos < length && expression[pos] == ',') {
                        ++pos;
                        regex.Append(',');
                        _copydigits();
                    }

                    regex.Append('}');
                _expectchar('}');
            }
            private         void                    _regex_copy(char closechar)
            {
                regex.Append(expression[pos++]);

                while (pos < length && expression[pos] != closechar) {
                    switch(expression[pos]) {
                    case '\\':
                        ++pos;
                        _copyandescapechar();
                        break;

                    case '(':
                        _regex_copy(')');
                        break;

                    case '{':
                        _regex_copy('}');
                        break;

                    case '[':
                        _regex_copyrange();
                        break;

                    default:
                        regex.Append(expression[pos++]);
                        break;
                    }
                }

                _expectchar(closechar);
                regex.Append(closechar);
            }
            private         void                    _regex_copyrange()
            {
                regex.Append(expression[pos++]);

                while (pos < length && expression[pos] != ']') {
                    switch(expression[pos]) {
                    case '\\':
                        ++pos;
                        _copyandescapechar();
                        break;

                    default:
                        _copyandescapechar();
                        break;
                    }
                }

                _expectchar(']');
                regex.Append(']');
            }
            private         void                    _copydigits()
            {
                while (_char_digit(expression[pos]))
                    regex.Append(expression[pos++]);
            }
            private         void                    _expectchar(char c)
            {
                if (pos >= length || expression[pos] != c)
                    throw new FormatException("Invalid wildcard path expression.");

                ++pos;
            }
            private         void                    _copyandescapechar()
            {
                if (pos >= length)
                    throw new FormatException("Invalid wildcard path expression.");

                char c = expression[pos++];

                if ((c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                    (c == '_') ||
                    (c == '-') ||
                    (c == '/')) {
                    regex.Append(c);
                }
                else {
                    regex.Append(@"\u");
                    regex.Append(((Int16)c).ToString("X4", CultureInfo.InvariantCulture));
                }
            }

            private static  bool                    _char_digit(char c)
            {
                return (c >= '0' && c <= '9');
            }
            private static  bool                    _char_name(char c)
            {
                return (c >= 'a' && c <= 'z') ||
                       (c >= 'A' && c <= 'Z') ||
                       (c >= '0' && c <= '9') ||
                       (c == '_');
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
