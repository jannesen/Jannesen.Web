using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreHttpHandler
    {
        private readonly        string                              _path;
        private readonly        string                              _verb;
        private readonly        WebCoreWildcardPathProcessor?       _wildcardPathProcessor;
        private readonly        WebCoreErrorHandler                 _errorHandler;
        private readonly        ResourceLogging?                    _logging;

        public                  string                              Path                    => _path;
        public                  string                              Verb                    => _verb;
        public                  WebCoreWildcardPathProcessor?       WildcardPathProcessor   => _wildcardPathProcessor;
        public                  ResourceLogging?                    Logging                 => _logging;
        public      virtual     string?                             Mimetype                => null;

        protected                                                   WebCoreHttpHandler(WebCoreConfigReader configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            _path   = configReader.GetValuePathName("path");
            _verb   = string.Intern(configReader.GetValueString("verb", "GET")!.ToUpperInvariant());
            _errorHandler = WebLoader.Instance.GetErrorHandler(configReader.GetValueString("error-handler", null));

            var logging = configReader.GetValueString("logging", null);

            if (logging != null)
                _logging = configReader.ApplicationConfig.GetResource<ResourceLogging>(logging);

            _wildcardPathProcessor = WebCoreWildcardPathProcessor.GetProcessor(_path);
        }

        public      virtual     void                                ProcessRequest(WebApplicationConfig applicationConfig, HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(applicationConfig);
            ArgumentNullException.ThrowIfNull(context);

            _processRequest(applicationConfig, context);
        }

        public      abstract    WebCoreResponse                     Process(WebCoreCall httpCall);
        public      virtual     WebCoreErrorData?                   ProcessError(Exception err)
        {
            return null;
        }

        private                 void                                _processRequest(WebApplicationConfig applicationConfig, HttpContext context)
        {
            var             httpCall    = new WebCoreCall(applicationConfig, context, this);
            WebCoreResponse webResponse;

            try {
                webResponse = Process(httpCall);
            }
            catch(WebHttpException err) {
                switch(err.StatusCode) {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.RequestEntityTooLarge:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.NotImplemented:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    webResponse = _errorHandler(this, err);
                    break;

                default:
                    _logging?.Logging(httpCall, err);
                    throw;
                }
            }
            catch(Exception err) {
                if (!(err is WebException werr && werr.logError == false)) {
                    applicationConfig.Application.LogError("Error in handler: " + Path + " Url: " + httpCall.Request.GetDisplayUrl(), err);
                }

                webResponse = _errorHandler(this, err);
            }

            if (webResponse != null) {
                webResponse.Send(httpCall, context.Response);
                _logging?.Logging(httpCall, webResponse, context.Response);
            }
        }
    }

    public class CoreHttpHandlerDictionary
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

        public                                                              CoreHttpHandlerDictionary()
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
}
