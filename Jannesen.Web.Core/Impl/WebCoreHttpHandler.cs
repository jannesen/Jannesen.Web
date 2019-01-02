using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreHttpHandler: IHttpHandler
    {
        private                 string                              _path;
        private                 string                              _verb;
        private                 bool                                _public;
        private                 WebCoreWildcardPathProcessor        _wildcardPathProcessor;
        private                 ResourceLogging                     _logging;

        public                  string                              Path
        {
            get {
                return _path;
            }
        }
        public                  string                              Verb
        {
            get {
                return _verb;
            }
        }
        public      virtual     bool                                Public
        {
            get {
                return _public;
            }
        }
        public                  WebCoreWildcardPathProcessor        WildcardPathProcessor
        {
            get {
                return _wildcardPathProcessor;
            }
        }
        public      virtual     bool                                MapTo200
        {
            get {
                return false;
            }
        }
        public                  ResourceLogging                     Logging
        {
            get {
                return _logging;
            }
        }
        public      virtual     string                              Mimetype
        {
            get {
                return null;
            }
        }

        public                  bool                                IsReusable
        {
            get {
                return true;
            }
        }

        public                                                      WebCoreHttpHandler(WebCoreConfigReader configReader)
        {
            _path   = configReader.GetValuePathName("path");
            _verb   = string.Intern(configReader.GetValueString("verb", "GET").ToUpper());
            _public = configReader.GetValueBool("public", false);

            string logging = configReader.GetValueString("logging", null);

            if (logging != null)
                _logging = configReader.Application.waGetResource<ResourceLogging>(logging);

            _wildcardPathProcessor = WebCoreWildcardPathProcessor.GetProcessor(_path);
        }

        public      virtual     IHttpHandler                        GetHttpHandler()
        {
            return (IHttpHandler)this;
        }

        public      virtual     void                                ProcessRequest(HttpContext context)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (Public) {
                using (SystemSection systemSection = new SystemSection())
                {
                    systemSection.ToSystem();

                    _processRequest(context);
                }
            }
            else {
                _processRequest(context);
            }
        }

        public      abstract    WebCoreResponse                     Process(WebCoreCall httpCall);
        public      virtual     int                                 ProcessErrorCode(Exception err, ref string code)
        {
            return 0;
        }

        private                 void                                _processRequest(HttpContext context)
        {
            WebCoreResponse     webResponse;
            WebCoreCall         httpCall    = new WebCoreCall(context, this);

            try {
                webResponse = Process(httpCall);
            }
            catch(HttpException err) {
                switch(err.GetHttpCode()) {
                case (int)HttpStatusCode.BadRequest:
                case (int)HttpStatusCode.RequestTimeout:
                case (int)HttpStatusCode.RequestEntityTooLarge:
                case (int)HttpStatusCode.InternalServerError:
                case (int)HttpStatusCode.NotImplemented:
                case (int)HttpStatusCode.BadGateway:
                case (int)HttpStatusCode.ServiceUnavailable:
                case (int)HttpStatusCode.GatewayTimeout:
                    webResponse = new WebCoreResponseError(this, err, Mimetype);
                    break;

                default:
                    if (_logging != null)
                        _logging.Logging(httpCall, err);
                    throw;
                }

            }
            catch(Exception err) {
                if (!(err is WebException && ((WebException)err).logError == false))
                    WebApplication.LogError(this, httpCall, err);

                webResponse = new WebCoreResponseError(this, err, Mimetype);
            }

            if (webResponse != null) {
                webResponse.Send(httpCall, context.Response);

                if (_logging != null)
                    _logging.Logging(httpCall, webResponse, context.Response);
            }
        }
    }

    public class CoreHttpHandlerDictionary
    {
        private class VerbDictionary
        {
            private                 Dictionary<string, WebCoreHttpHandler>      _exact;
            private                 List<WebCoreHttpHandler>                    _wildcard;

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
                    int     i = 0;

                    while (i < _wildcard.Count && _wildcard[i].WildcardPathProcessor.Prefix.Length >= httpHandler.WildcardPathProcessor.Prefix.Length)
                        ++i;

                    _wildcard.Insert(i, httpHandler);
                }
            }
            public                  WebCoreHttpHandler                          GetHandler(string path)
            {
                {
                    if (_exact.TryGetValue(path, out var rtn))
                        return rtn;
                }

                {
                    for(int i = 0 ; i < _wildcard.Count ; ++i) {
                        if (_wildcard[i].WildcardPathProcessor.IsMatch(path))
                            return _wildcard[i];
                    }
                }

                return null;
            }
        }

        private                 Dictionary<string, VerbDictionary>          _dictionary;

        public                                                              CoreHttpHandlerDictionary()
        {
            _dictionary = new Dictionary<string, VerbDictionary>(32);
        }

        public                  void                                        Add(WebCoreHttpHandler httpHandler)
        {
            if (!_dictionary.TryGetValue(httpHandler.Verb, out var verbDictionary))
                _dictionary.Add(httpHandler.Verb, verbDictionary = new VerbDictionary());

            verbDictionary.Add(httpHandler);
        }
        public                  WebCoreHttpHandler                          GetHandler(string path, string verb)
        {
            if (!_dictionary.TryGetValue((verb != "HEAD" ? verb : "GET"), out var verbDictionary))
                return null;

            return verbDictionary.GetHandler(path);
        }
    }
}
