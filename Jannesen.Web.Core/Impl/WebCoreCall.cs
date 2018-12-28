using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public interface IWebCoreCallProcessor
    {
        void                Proces(WebCoreCall httpCall);
    }

    public class WebCoreCall
    {
        private         DateTime                            _timestamp;
        private         HttpContext                         _context;
        private         HttpRequest                         _request;
        private         WebCoreHttpHandler                  _handler;
        private         byte[]                              _requestBodyData;
        private         List<object>                        _requestProcessors;

        public          DateTime                            Timestamp
        {
            get {
                return _timestamp;
            }
        }
        public          HttpContext                         Context
        {
            get {
                return _context;
            }
        }
        public          HttpRequest                         Request
        {
            get {
                return _request;
            }
        }
        public          WebCoreHttpHandler                  Handler
        {
            get {
                return _handler;
            }
        }
        internal        byte[]                              RequestBodyData
        {
            get {
                return _requestBodyData;
            }
        }
        public          System.Web.Caching.Cache            Cache
        {
            get {
                return _context.Cache;
            }
        }

        public          string                              HttpMethod
        {
            get {
                return _request.HttpMethod;
            }
        }
        public          string                              RequestContentType
        {
            get {
                return _request.ContentType;
            }
        }
        public          int?                                RequestContentLength
        {
            get {
                string s = _request.Headers["Content-Length"];

                if (!string.IsNullOrEmpty(s)) {
                    if (int.TryParse(s, out var rtn))
                        return rtn;
                }

                return null;
            }
        }
        public          DateTime?                           RequestIfModifiedSince
        {
            get {
                string s = _request.Headers["If-Modified-Since"];

                if (!string.IsNullOrEmpty(s)) {
                    if (DateTime.TryParseExact(s, "R", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out var rtn))
                        return rtn;
                }

                return null;
            }
        }
        public          string                              RequestIfNoneMatch
        {
            get {
                string s = _request.Headers["If-None-Match"];

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
        public          string                              RequestReferer
        {
            get {
                string s = _request.Headers["Referer"];

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
        public          string                              RequestUserAgent
        {
            get {
                string s = _request.UserAgent;

                return !string.IsNullOrEmpty(s) ? s : null;
            }
        }
        public          string                              RequestRemoteAddr
        {
            get {
                string rtn = _request.Headers["X-Forwarded-For"];

                if (string.IsNullOrEmpty(rtn))
                    rtn = _request.UserHostAddress;

                return !string.IsNullOrEmpty(rtn) ? rtn : null;
            }
        }
        public          WebCoreProcessorBasicAutorization   RequestBasicAutorization
        {
            get {
                return GetRequestProcessor<WebCoreProcessorBasicAutorization>();
            }
        }
        public          WebCoreUrlPathData                  RequestUrlPathData
        {
            get {
                return GetRequestProcessor<WebCoreUrlPathData>();
            }
        }
        public          WebCoreProcessorTextXml             RequestTextXml
        {
            get {
                return GetRequestProcessor<WebCoreProcessorTextXml>();
            }
        }
        public          WebCoreProcessorTextJson            RequestTextJson
        {
            get {
                return GetRequestProcessor<WebCoreProcessorTextJson>();
            }
        }

        public                                              WebCoreCall(HttpContext httpCall, WebCoreHttpHandler handler)
        {
            _timestamp   = DateTime.UtcNow;
            _context     = httpCall;
            _request     = httpCall.Request;
            _handler     = handler;
        }

        public          T                                   GetRequestProcessor<T>() where T: IWebCoreCallProcessor, new()
        {
        // Find if already created.
            if (_requestProcessors != null) {
                for (int i = 0 ; i < _requestProcessors.Count ; ++i) {
                    if (_requestProcessors[i] is T)
                        return (T)_requestProcessors[i];
                }
            }

        // Create and process
            {
                T   processor = new T();

                processor.Proces(this);

                if (_requestProcessors == null)
                    _requestProcessors = new List<object>();

                _requestProcessors.Add(processor);

                return processor;
            }
        }
        public          byte[]                              GetBodyData()
        {
            if (_requestBodyData == null) {
                int?    length = RequestContentLength;

                if (!length.HasValue)
                    throw new WebRequestException("Missing content-length header.");

                byte[]  buf = new byte[length.Value];

                using (System.IO.Stream inputStream = _request.InputStream)
                {
                    int     size = 0;
                    int     rs;

                    while (size < buf.Length && (rs = inputStream.Read(buf, size, buf.Length - size)) > 0)
                        size += rs;

                    if (size != buf.Length)
                        throw new WebRequestException("BODY is incomplete.");
                }

                _requestBodyData = buf;
            }

            return _requestBodyData;
        }
        public          StreamReader                        GetBodyText(string contenttype)
        {
            var     requestContentType = RequestContentType;

            if (string.IsNullOrEmpty(requestContentType)) {
                if (_request.ContentLength <= 0)
                    return null;

                throw new WebRequestException("Missing Content-Type.");
            }

            System.Text.Encoding    encoding         = null;
            string[]                contentTypeParts = requestContentType.Split(';');

            for (int i = 1 ; i <contentTypeParts.Length ; ++i)
                contentTypeParts[i] = contentTypeParts[i].TrimStart();

            if (contenttype != null && contentTypeParts[0] != contenttype)
                throw new WebRequestException("Expect " + contenttype + " body.");

            for (int i = 1 ; i < contentTypeParts.Length ; ++i) {
                if (contentTypeParts[i].StartsWith("charset=")) {
                    string  charset     = contentTypeParts[i].Substring(8);

                    switch(charset.ToLower())
                    {
                    case "unicode": encoding = System.Text.Encoding.Unicode;    break;
                    case "utf-8":   encoding = System.Text.Encoding.UTF8;       break;
                    default:        throw new NotImplementedException("Invalid characterset '" + charset + "'.");
                    }
                }
            }

            if (encoding == null)
                throw new WebRequestException("Missing charset in Content-Type " + requestContentType + ".");

            return new System.IO.StreamReader(new System.IO.MemoryStream(GetBodyData(), false), encoding, false, 4096, false);
        }
        public          string                              GetBodyString(string contenttype)
        {
            using(System.IO.StreamReader reader = GetBodyText(contenttype))
                return reader != null ? reader.ReadToEnd() : null;
        }
    }
}
