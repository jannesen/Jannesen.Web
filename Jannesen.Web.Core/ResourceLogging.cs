using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Jannesen.Web.Core;

[WebCoreResource("logging")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by reflection")]
public class ResourceLogging: WebCoreResource
{
    private readonly        string              _directory;
    private readonly        SemaphoreSlim       _logLock;
    private                 FileStream?         _filestream;
    private                 DateTime            _nextFile;

    public      override    string              Type        => "log";
    public                  string              Directory   => _directory;

    public                                      ResourceLogging(WebCoreConfigReader configReader): base(configReader)
    {
        ArgumentNullException.ThrowIfNull(configReader);

        _directory   = configReader.GetValueString("directory");
        _logLock     = new SemaphoreSlim(1, 1);
        _filestream  = null;
        _nextFile    = DateTime.MinValue;
    }

    protected   override    void                Dispose(bool disposing)
    {
        lock(_logLock) {
            _filestream?.Dispose();
            _filestream = null;
        }
        _logLock.Dispose();
        base.Dispose(disposing);
    }

    public            async Task                LoggingAsync(WebCoreCall call, IWebCoreResponse response, HttpResponse httpResponse)
    {
        ArgumentNullException.ThrowIfNull(call);

        await _logLock.WaitAsync();
        try {
            using (var writer = _getLogStream()) {
                await _logRequestAsync(writer, call);
                await _logResponseAsync(writer, response, httpResponse);
                await _logEndAsync(writer);
            }
        }
        catch(Exception logerr) {
            Application.LogError("Logging failed", logerr);
        }
        finally {
            _logLock.Release();
        }
    }
    public            async Task                LoggingAsync(WebCoreCall call, Exception err)
    {
        ArgumentNullException.ThrowIfNull(call);

        await _logLock.WaitAsync();
        try {
            using (var writer = _getLogStream()) {
                await _logRequestAsync(writer, call);
                await _logErrorAsync(writer, err);
                await _logEndAsync(writer);
            }
        }
        catch(Exception logerr) {
            Application.LogError("Logging failed", logerr);
        }
        finally {
            _logLock.Release();
        }
    }

    private                 StreamWriter        _getLogStream()
    {
        var now = DateTime.Now;

        if (_filestream == null || now > _nextFile) {
            _filestream?.Dispose();
            _filestream = null;

            var fileName = _directory + "\\weblog-" + now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log";
            Application.LogInfo("New logfile: " + fileName);
            _filestream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 1);
            _nextFile   = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerDay) + TimeSpan.TicksPerDay);
        }

        return new StreamWriter(_filestream, System.Text.Encoding.UTF8, 0x10000, true);
    }
    private static    async Task                _logRequestAsync(StreamWriter writer, WebCoreCall call)
    {
        await writer.WriteAsync("### REQUEST ### @");
        await writer.WriteLineAsync(call.Timestamp.ToString("yyyy-dd-MM HH:mm:ss", CultureInfo.InvariantCulture));

        await writer.WriteAsync(call.Request.Method);
        await writer.WriteAsync(" ");
        await writer.WriteLineAsync(call.Request.GetEncodedPathAndQuery());

        foreach(var h in call.Request.Headers) {
            for(var i = 0 ; i < h.Value.Count ; i++) {
                var value = h.Value[i];

                switch(h.Key) {
                case "Authorization":
                    value = "*****";
                    break;
                }

                await writer.WriteAsync(h.Key);
                await writer.WriteAsync(": ");
                await writer.WriteLineAsync(value);
            }
        }

        var body = call.GetProcessorResultOptional(WebCoreProcessorRequestBody.Processor);
        if (body != null) {
            if (body.ContentType != null && body.ContentType.Contains("charset=utf-8", StringComparison.OrdinalIgnoreCase)) {
                await writer.WriteLineAsync();
                await writer.FlushAsync();
                await writer.BaseStream.WriteAsync(new ReadOnlyMemory<byte>(body.Data, 0, body.Data.Length));
                await writer.WriteLineAsync();
            }
            else {
                await writer.WriteLineAsync("[BINARY-DATA]");
            }
        }
    }
    private static    async Task                _logResponseAsync(StreamWriter writer, IWebCoreResponse response, HttpResponse httpResponse)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(httpResponse);

        await writer.WriteLineAsync("### RESPONSE ");

        await writer.WriteLineAsync(httpResponse.StatusCode.ToString(CultureInfo.InvariantCulture));

        var hasContentLength = false;

        foreach(var h in httpResponse.Headers) {
            var key = h.Key;

            for(var i = 0 ; i < h.Value.Count; ++i) {
                var value = h.Value[i];

                if (key == "Content-Length")
                    hasContentLength = true;

                await writer.WriteAsync(key);
                await writer.WriteAsync(": ");
                await writer.WriteLineAsync(value);
            }
        }

        if (hasContentLength) {
            await response.WriteLoggingDataAsync(writer);
        }
    }
    private static    async Task                _logErrorAsync(StreamWriter writer, Exception? err)
    {
        await writer.WriteLineAsync("### ERROR");

        while (err != null) {
            await writer.WriteLineAsync("[" + err.GetType().Name + "]: " + err.Message);
            err = err.InnerException;
        }
    }
    private static    async Task                _logEndAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync("###");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync();
    }
}
