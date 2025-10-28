using System;
using System.Collections.Generic;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreResource: IDisposable
    {
        private readonly        WebApplication      _application;
        private readonly        string              _name;
        private readonly        bool                _down;

        public      abstract    string              Type
        {
            get ;
        }
        public                  WebApplication      Application
        {
            get {
                return _application;
            }
        }
        public                  string              Name
        {
            get {
                return _name;
            }
        }
        public                  bool                Down
        {
            get {
                return _down;
            }
        }

        protected                                   WebCoreResource(WebCoreConfigReader configReader)
        {
            ArgumentNullException.ThrowIfNull(configReader);

            _application = configReader.ApplicationConfig.Application;
            _name        = configReader.GetValueString("name");
            _down        = configReader.GetValueBool  ("down", false);
        }
                                                    ~WebCoreResource()
        {
            Dispose(false);
        }
        public                  void                Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected   virtual     void                Dispose(bool disposing)
        {
        }
    }

    public sealed class CoreResourceDictionary: IDisposable
    {
        private readonly        Dictionary<string, WebCoreResource>     _dictionary;

        public                                                          CoreResourceDictionary()
        {
            _dictionary = new Dictionary<string,WebCoreResource>(256);
        }

        public                  WebCoreResource                         GetWebResource(Type type, string name)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(name);

            if (_dictionary.TryGetValue(type.Name + "/" + name, out var rtn)) {
                if (rtn.GetType() == type && rtn.Name == name)
                    return rtn;
            }

            throw new WebResourceNotFoundException("Unknown resource '" + type.Name + "/" + name + "'.");
        }
        public                  void                                    Dispose()
        {
            foreach(WebCoreResource resource in _dictionary.Values) {
                try {
                    resource.Dispose();
                }
                catch(Exception err) {
                    resource.Application.LogError("Unloading resource '" + resource.Name + "' failed.", err);
                }
            }
        }

        public                  void                                    Add(WebCoreResource resource)
        {
            ArgumentNullException.ThrowIfNull(resource);

            _dictionary.Add(resource.GetType().Name + "/" + resource.Name, resource);
        }
    }
}
