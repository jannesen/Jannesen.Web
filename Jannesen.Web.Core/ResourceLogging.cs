using System;
using System.Globalization;
using System.IO;
using System.Web;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    [WebCoreAttribureResource("logging")]
    public class ResourceLogging: WebCoreResource
    {
        private readonly        string              _directory;
        private readonly        object              _logLock;
        private                 FileStream          _filestream;
        private                 DateTime            _nextFile;

        public      override    string              Type
        {
            get {
                return "log";
            }
        }

        public                  string              Directory
        {
            get {
                return _directory;
            }
        }

        public                                      ResourceLogging(WebCoreConfigReader configReader): base(configReader)
        {
            _directory = configReader.GetValueString("directory");
            _logLock   = new object();
            _nextFile  = DateTime.MinValue;
        }

        protected   override    void                Dispose(bool disposing)
        {
            lock(_logLock) {
                if (_filestream != null) {
                    _filestream.Dispose();
                    _filestream = null;
                }
            }
        }

        public                  void                Logging(WebCoreCall call, WebCoreResponse response, HttpResponse httpResponse)
        {
            lock(_logLock) {
                try {
                    using (StreamWriter writer = _getLogStream())
                    {
                        _logRequest(writer, call);
                        _logResponse(writer, response, httpResponse);
                        _logEnd(writer);
                    }
                }
                catch(Exception logerr) {
                    WebApplication.LogError("Logging failed", logerr);
                }
            }
        }
        public                  void                Logging(WebCoreCall call, Exception err)
        {
            lock(_logLock) {
                try {
                    using (StreamWriter writer = _getLogStream())
                    {
                        _logRequest(writer, call);
                        _logError(writer, err);
                        _logEnd(writer);
                    }
                }
                catch(Exception logerr) {
                    WebApplication.LogError("Logging failed", logerr);
                }
            }
        }

        public                  StreamWriter        _getLogStream()
        {
            DateTime now = DateTime.Now;

            if (_filestream == null || now > _nextFile) {
                if (_filestream != null) {
                    _filestream.Dispose();
                    _filestream = null;
                }

                string fileName = _directory + "\\weblog-" + now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log";
                WebApplication.LogEvent(WebApplication.EventID.NewLogfile, "New logfile: " + fileName);
                _filestream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1);
                _nextFile   = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerDay) + TimeSpan.TicksPerDay);
            }

            return new StreamWriter(_filestream, System.Text.Encoding.UTF8, 0x10000, true);
        }
        public      static      void                _logRequest(StreamWriter writer, WebCoreCall call)
        {
            writer.Write("### REQUEST ### @");
            writer.WriteLine(call.Timestamp.ToString("yyyy-dd-MM HH:mm:ss", CultureInfo.InvariantCulture));

            writer.Write(call.Request.HttpMethod);
            writer.Write(" ");
            writer.WriteLine(call.Request.Url.ToString());

            bool    textbody = false;

            for(int i = 0 ; i < call.Request.Headers.Count ; ++i) {
                string  key   = call.Request.Headers.GetKey(i);
                string  value = call.Request.Headers[i];

                switch(key) {
                case "Authorization":
                    value = "*****";
                    break;

                case "Content-Type":
                    if (value.IndexOf("charset=utf-8", StringComparison.InvariantCulture) > 0 || value.IndexOf("charset=UTF-8", StringComparison.InvariantCulture) > 0)
                        textbody = true;
                    break;
                }
                writer.Write(key);
                writer.Write(": ");
                writer.WriteLine(value);
            }

            if (call.RequestBodyData != null) {
                if (textbody) {
                    writer.WriteLine();
                    writer.Flush();
                    writer.BaseStream.Write(call.RequestBodyData, 0, call.RequestBodyData.Length);
                    writer.WriteLine();
                }
                else
                    writer.WriteLine("[BINARY-DATA]");
            }
        }
        public      static      void                _logResponse(StreamWriter writer, WebCoreResponse response, HttpResponse httpResponse)
        {
            writer.WriteLine("### RESPONSE ");

            writer.WriteLine(httpResponse.StatusCode);

            bool    hasContentLength = false;

            for(int i = 0 ; i < httpResponse.Headers.Count ; ++i) {
                string  key = httpResponse.Headers.GetKey(i);

                if (key == "Content-Length")
                    hasContentLength = true;

                writer.Write(key);
                writer.Write(": ");
                writer.WriteLine(httpResponse.Headers[i]);
            }

            if (hasContentLength)
                response.WriteLoggingData(writer);
        }
        public      static      void                _logError(StreamWriter writer, Exception err)
        {
            writer.WriteLine("### ERROR");

            while (err != null) {
                writer.WriteLine("[" + err.GetType().Name + "]: " + err.Message);
                err = err.InnerException;
            }
        }
        public      static      void                _logEnd(StreamWriter writer)
        {
            writer.WriteLine("###");
            writer.WriteLine();
            writer.WriteLine();
        }
    }
}
