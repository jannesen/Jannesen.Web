using System;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreDataSourceAttribute("header")]
    sealed class http_header: WebCoreDataSource
    {
        private readonly            string                  _name;

        public                                              http_header(string name_args): base(name_args)
        {
            switch(Name) {
            case "method":
            case "if-modified-since":
            case "if_modified_since":
            case "if-none-match":
            case "if_none_match":
            case "user-agent":
            case "user_agent":
            case "remote-addr":
            case "remote_addr":
            case "basic-username":
            case "basic_username":
            case "basic-passwd":
            case "basic_passwd":
            case "referer":
                _name = Name.Replace("_", "-", StringComparison.Ordinal);
                break;

            default:
                if (Name.StartsWith("x-", StringComparison.OrdinalIgnoreCase)) {
                    _name = Name;
                }
                else {
                    throw new WebSourceException("Unknown source header '" + Name + "'.");
                }
                break;
            }
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            switch(_name) {
            case "method":              return new WebCoreDataValue((object)httpCall.HttpMethod);
            case "if-modified-since":   return new WebCoreDataValue((object?)httpCall.RequestIfModifiedSince);
            case "if-none-match":       return new WebCoreDataValue((object?)httpCall.RequestIfNoneMatch);
            case "referer":             return new WebCoreDataValue((object?)httpCall.RequestReferer);
            case "user-agent":          return new WebCoreDataValue((object?)httpCall.RequestUserAgent);
            case "remote-addr":         return new WebCoreDataValue((object?)httpCall.RequestRemoteAddr);
            case "basic-username":      return new WebCoreDataValue((object?)httpCall.RequestBasicAutorization.UserName);
            case "basic-passwd":        return new WebCoreDataValue((object?)httpCall.RequestBasicAutorization.Passwd);
            default:                    return new WebCoreDataValue((object?)httpCall.GetHeader(_name));
            }
        }

        public      override        string                  ToString()
        {
            return "header:" + Name;
        }
    }
}
