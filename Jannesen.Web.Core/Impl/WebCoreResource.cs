using System;
using System.Collections.Generic;
using System.Web;

namespace Jannesen.Web.Core.Impl
{
    public abstract class WebCoreResource: IDisposable
    {
        private readonly        string              _name;
        private readonly        bool                _down;

        public      abstract    string              Type
        {
            get ;
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
            _name = configReader.GetValueString("name");
            _down = configReader.GetValueBool  ("down", false);
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
                    WebApplication.LogError("Unloading resource '" + resource.Name + "' failed.", err);
                }
            }
        }

        public                  void                                    Add(WebCoreResource resource)
        {
            _dictionary.Add(resource.GetType().Name + "/" + resource.Name, resource);
        }
    }
}
