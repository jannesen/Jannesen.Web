﻿using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Jannesen.Web.Core.Impl
{
    [Serializable]
    public class WebCoreUrlPathData: NameValueCollection, IWebCoreCallProcessor
    {
        public                      WebCoreUrlPathData()
        {
        }
        protected                   WebCoreUrlPathData(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public          void        Proces(WebCoreCall httpCall)
        {
            if (httpCall.Handler.WildcardPathProcessor == null)
                throw new WebHandlerConfigException("URL-PATH parameters not available, because not a wildcard handler.");

            string[]    names = httpCall.Handler.WildcardPathProcessor.Names;

            if (names != null) {
                Match   match = httpCall.Handler.WildcardPathProcessor.RegexMatch(WebApplication.GetRelPath(httpCall.Request.Path));

                foreach(string name in names)
                    base.Add(name, match.Groups[name].Value);
            }
        }
    }
}
