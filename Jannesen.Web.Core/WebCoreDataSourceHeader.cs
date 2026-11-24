using System;
using System.Collections.Generic;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (created using reflection)

namespace Jannesen.Web.Core;

[WebCoreDataSourceAttribute("header")]
sealed class WebCoreDataSourceHeader: WebCoreDataSource
{
    private readonly            string                                  _name;

    public                                                              WebCoreDataSourceHeader(string name_args): base(name_args)
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

    public      override        IEnumerable<IWebCoreProcessor<object>>  GetRequiredProcessors()
    {
        switch(_name) {
        case "basic-username":
        case "basic-passwd":
            return [ WebCoreProcessorBasicAutorization.Definition ];
        default:
            return [];
        }
    }
    public      override        WebCoreDataValue                        GetValue(WebCoreCall httpCall)
    {
        switch(_name) {
        case "method":              return new WebCoreDataValue((object)httpCall.Request.Method);
        case "if-modified-since":   return new WebCoreDataValue((object?)httpCall.Request.IfModifiedSince);
        case "if-none-match":       return new WebCoreDataValue((object?)httpCall.Request.IfNoneMatch);
        case "referer":             return new WebCoreDataValue((object?)httpCall.Request.Referer);
        case "user-agent":          return new WebCoreDataValue((object?)httpCall.Request.UserAgent);
        case "remote-addr":         return new WebCoreDataValue((object?)httpCall.RemoteAddr);
        case "basic-username":      return new WebCoreDataValue((object?)httpCall.GetProcessorResult(WebCoreProcessorBasicAutorization.Definition).UserName);
        case "basic-passwd":        return new WebCoreDataValue((object?)httpCall.GetProcessorResult(WebCoreProcessorBasicAutorization.Definition).Passwd);
        default:                    return new WebCoreDataValue((object?)httpCall.Request.GetHeader(_name));
        }
    }

    public      override        string                                  ToString()
    {
        return "header:" + Name;
    }
}
