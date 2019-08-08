using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    [WebCoreAttribureResource("webservice")]
    public class ResourceWebService: WebCoreResource
    {
        private readonly        string                          _baseUrl;
        private readonly        string                          _key;
        private readonly        string                          _username;
        private readonly        string                          _passwd;
        private readonly        X509Certificate2Collection      _certificates;
        private readonly        Dictionary<string, string>      _properties;
        
        public      override    string                          Type
        {
            get {
                return "webservice";
            }
        }

        public                  string                          BaseUrl
        {
            get {
                return _baseUrl;
            }
        }
        public                  string                          Key
        {
            get {
                return _key;
            }
        }
        public                  string                          Username
        {
            get {
                return _username;
            }
        }
        public                  string                          Passwd
        {
            get {
                return _passwd;
            }
        }
        public                  X509Certificate2Collection      Certificates
        {
            get {
                return _certificates;
            }
        }

        public Dictionary<string, string> Properties
        {
            get
            {
                return _properties;
            }
        }

        public                                                  ResourceWebService(WebCoreConfigReader configReader): base(configReader)
        {
            _baseUrl  = configReader.GetValueString("baseurl");
            _key      = configReader.GetValueString("key", null);
            _username = configReader.GetValueString("username", null);
            _passwd   = (_username != null) ? configReader.GetValueString("passwd") : null;
            _properties = configReader.GetValueDictionary();

            var certificate = configReader.GetValueString("certificate", null);
            if (certificate != null) {
                try {
                    _certificates = new X509Certificate2Collection();

                    using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                    {
                        try {
                            store.Open(OpenFlags.ReadOnly);
                        }
                        catch(Exception err) {
                            throw new WebConfigException("Failed to open X509Store", err, configReader);
                        }

                        foreach (var cert in store.Certificates) {
                            if (cert.Subject == certificate) {
                                if (!cert.HasPrivateKey)
                                    throw new Exception("Certificate has no private key.");

                                _certificates.Add(cert);
                            }
                        }
                    }

                    if (_certificates.Count == 0)
                        throw new Exception("No certificate found in store.");
                }
                catch(Exception err) {
                    throw new WebConfigException("Failed to load certificate '" + certificate + "'.", err, configReader);
                }
            }
        }
    }
}
