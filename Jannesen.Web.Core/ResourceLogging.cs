using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Jannesen.Web.Core.Impl;

namespace Jannesen.Web.Core
{
    [WebCoreResourceAttribute("logging")]
    public class ResourceLogging: WebCoreResource
    {
        private readonly        string              _directory;
        private readonly        Lock                _logLock;
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
            ArgumentNullException.ThrowIfNull(configReader);

            _directory   = configReader.GetValueString("directory");
            _logLock     = new Lock();
            _nextFile    = DateTime.MinValue;
        }

        protected   override    void                Dispose(bool disposing)
        {
            lock(_logLock) {
                if (_filestream != null) {
                    _filestream.Dispose();
                    _filestream = null;
                }
            }
            base.Dispose(disposing);
        }

        public                  void                Logging(WebCoreCall call, WebCoreResponse response, HttpResponse httpResponse)
        {
            ArgumentNullException.ThrowIfNull(call);

            lock(_logLock) {
                try {
                    using (StreamWriter writer = _getLogStream()) {
                        _logRequest(writer, call);
                        _logResponse(writer, response, httpResponse);
                        _logEnd(writer);
                    }
                }
                catch(Exception logerr) {
                    Application.LogError("Logging failed", logerr);
                }
            }
        }
        public                  void                Logging(WebCoreCall call, Exception err)
        {
            ArgumentNullException.ThrowIfNull(call);

            lock(_logLock) {
                try {
                    using (StreamWriter writer = _getLogStream()) {
                        _logRequest(writer, call);
                        _logError(writer, err);
                        _logEnd(writer);
                    }
                }
                catch(Exception logerr) {
                    Application.LogError("Logging failed", logerr);
                }
            }
        }

        private                 StreamWriter        _getLogStream()
        {
            DateTime now = DateTime.Now;

            if (_filestream == null || now > _nextFile) {
                if (_filestream != null) {
                    _filestream.Dispose();
                    _filestream = null;
                }

                string fileName = _directory + "\\weblog-" + now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log";
                Application.LogInfo("New logfile: " + fileName);
                _filestream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1);
                _nextFile   = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerDay) + TimeSpan.TicksPerDay);
            }

            return new StreamWriter(_filestream, System.Text.Encoding.UTF8, 0x10000, true);
        }
        private     static      void                _logRequest(StreamWriter writer, WebCoreCall call)
        {
            writer.Write("### REQUEST ### @");
            writer.WriteLine(call.Timestamp.ToString("yyyy-dd-MM HH:mm:ss", CultureInfo.InvariantCulture));

            writer.Write(call.Request.Method);
            writer.Write(" ");
            writer.WriteLine(call.Request.GetEncodedPathAndQuery());

            bool    textbody = false;

            foreach(var h in call.Request.Headers) {
                var key = h.Key;

                for(int i = 0 ; i < h.Value.Count ; i++) {
                    var value = h.Value[i];

                    switch(h.Key) {
                    case "Authorization":
                        value = "*****";
                        break;

                    case "Content-Type":
                        if (value.IndexOf("charset=utf-8", StringComparison.Ordinal) > 0 ||
                            value.IndexOf("charset=UTF-8", StringComparison.Ordinal) > 0)
                            textbody = true;
                        break;
                    }

                    writer.Write(h.Key);
                    writer.Write(": ");
                    writer.WriteLine(value);
                }
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
        private     static      void                _logResponse(StreamWriter writer, WebCoreResponse response, HttpResponse httpResponse)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(httpResponse);

            writer.WriteLine("### RESPONSE ");

            writer.WriteLine(httpResponse.StatusCode);

            bool    hasContentLength = false;

            foreach(var h in httpResponse.Headers) {
                var key = h.Key;

                for(int i = 0 ; i < h.Value.Count; ++i) {
                    var value = h.Value[i];

                    if (key == "Content-Length")
                        hasContentLength = true;

                    writer.Write(key);
                    writer.Write(": ");
                    writer.WriteLine(value);
                }
            }

            if (hasContentLength) {
                response.WriteLoggingData(writer);
            }
        }
        private     static      void                _logError(StreamWriter writer, Exception err)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.WriteLine("### ERROR");

            while (err != null) {
                writer.WriteLine("[" + err.GetType().Name + "]: " + err.Message);
                err = err.InnerException;
            }
        }
        private     static      void                _logEnd(StreamWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.WriteLine("###");
            writer.WriteLine();
            writer.WriteLine();
        }
    }
}
