using System;
using System.Xml;
using System.Web;
using Jannesen.FileFormat.Json;

namespace Jannesen.Web.Core.Impl
{
    public class WebCoreProcessorTextJson : IWebCoreCallProcessor
    {
        private                 object              _document;

        public                  object              Document
        {
            get {
                return _document;
            }
        }

        public                  void                Proces(WebCoreCall httpCall)
        {
            try {
                using(System.IO.StreamReader reader = httpCall.GetBodyText("application/json"))
                {
                    if (reader != null)
                        _document = JsonReader.Parse(reader);
                }
            }
            catch(Exception err) {
                throw new WebRequestException("Error in JSON HTTP-BODY.", err);
            }
        }

        public                  bool                GetValue(string name, out object rtn)
        {
            if (_document == null)
                throw new WebRequestException("Empty body or null.");

            if (!(_document is JsonObject))
                throw new WebRequestException("JSON root must by a object");

            if (name.IndexOf(".") < 0)
                return ((JsonObject)_document).TryGetValue(name, out rtn);

            object  obj = _document;

            foreach(string n in name.Split('.')) {
                if (!(obj is JsonObject))
                    throw new WebInvalidValueException("Invalid Json query.");

                if (!((JsonObject)obj).TryGetValue(n, out obj)) {
                    rtn = null;
                    return false;
                }
            }

            rtn = obj;
            return true;
        }
    }
}
