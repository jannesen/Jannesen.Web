using System;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core.Impl.Source
{
    [WebCoreAttributeDataSource("header")]
    class http_header: WebCoreDataSource
    {
        enum ValueNameCode
        {
            Method,
            IfModifiedSince,
            IfNoneMatch,
            Referer,
            UserAgent,
            RemoteAddr,
            BasicUsername,
            BasicPasswd
        }

        private                     ValueNameCode           _valueCode;

        public                                              http_header(string name_args): base(name_args)
        {
            switch(Name) {
            case "method":                      _valueCode = ValueNameCode.Method;                  break;
            case "if-modified-since":
            case "if_modified_since":           _valueCode = ValueNameCode.IfModifiedSince;         break;
            case "if-none-match":
            case "if_none_match":               _valueCode = ValueNameCode.IfNoneMatch;             break;
            case "referer":                     _valueCode = ValueNameCode.Referer;                 break;
            case "user-agent":
            case "user_agent":                  _valueCode = ValueNameCode.UserAgent;               break;
            case "remote-addr":
            case "remote_addr":                 _valueCode = ValueNameCode.RemoteAddr;              break;
            case "basic-username":
            case "basic_username":              _valueCode = ValueNameCode.BasicUsername;           break;
            case "basic-passwd":
            case "basic_passwd":                _valueCode = ValueNameCode.BasicPasswd;             break;
            default:                            throw new WebSourceException("Unknown source header '" + Name + "'.");
            }
        }

        public      override        WebCoreDataValue        GetValue(WebCoreCall httpCall)
        {
            switch(_valueCode) {
            case ValueNameCode.Method:              return new WebCoreDataValue((object)httpCall.HttpMethod);
            case ValueNameCode.IfModifiedSince:     return new WebCoreDataValue((object)httpCall.RequestIfModifiedSince);
            case ValueNameCode.IfNoneMatch:         return new WebCoreDataValue((object)httpCall.RequestIfNoneMatch);
            case ValueNameCode.Referer:             return new WebCoreDataValue((object)httpCall.RequestReferer);
            case ValueNameCode.UserAgent:           return new WebCoreDataValue((object)httpCall.RequestUserAgent);
            case ValueNameCode.RemoteAddr:          return new WebCoreDataValue((object)httpCall.RequestRemoteAddr);
            case ValueNameCode.BasicUsername:       return new WebCoreDataValue((object)httpCall.RequestBasicAutorization.UserName);
            case ValueNameCode.BasicPasswd:         return new WebCoreDataValue((object)httpCall.RequestBasicAutorization.Passwd);
            default:                                throw new NotImplementedException("Parameter header:" + Name + " not implemented.");
            }
        }

        public      override        string                  ToString()
        {
            return "header:" + Name;
        }
    }
}
