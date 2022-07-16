using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Jannesen.Web.Core.Impl
{
    public interface IFileLocation
    {
        string          Filename        { get; }
        int             LineNumber      { get; }
    }

    [Serializable]
    public abstract class WebException: Exception
    {
        public  abstract    bool            logError        { get; }

        protected                           WebException(string message): base(message)
        {
        }
        protected                           WebException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebConfigException: WebException, IFileLocation
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public              string          Filename                { get ; }
        public              int             LineNumber              { get ; }

        public                              WebConfigException(string message, WebCoreConfigReader configReader): this(message, null, configReader)
        {
        }
        public                              WebConfigException(string message, Exception innerException, WebCoreConfigReader configReader): base(message, innerException)
        {
            Filename   = configReader.Filename;
            LineNumber = configReader.LineNumber;
        }

        protected                           WebConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Filename   = info.GetString(nameof(Filename));
            this.LineNumber = info.GetInt32(nameof(LineNumber));
        }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public      override    void        GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Filename),   this.Filename);
            info.AddValue(nameof(LineNumber), this.LineNumber);
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebSourceException: Exception
    {
        public                              WebSourceException(string message): base(message)
        {
        }
        public                              WebSourceException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebSourceException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebConversionException: Exception
    {
        public                              WebConversionException(string message): base(message)
        {
        }
        public                              WebConversionException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebConversionException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebInvalidValueException: Exception
    {
        public                              WebInvalidValueException(string message): base(message)
        {
        }
        public                              WebInvalidValueException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebInvalidValueException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebHandlerConfigException: Exception
    {
        public                              WebHandlerConfigException(string message): base(message)
        {
        }
        public                              WebHandlerConfigException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebHandlerConfigException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public class WebResourceNotFoundException: WebException
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public                              WebResourceNotFoundException(string message): base(message)
        {
        }
        protected                           WebResourceNotFoundException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    [Serializable]
    public  class WebInitializationException: WebException
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public                              WebInitializationException(string message): base(message)
        {
        }
        public                              WebInitializationException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebInitializationException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public  class WebAppNotInitialized: WebException
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public                              WebAppNotInitialized(): base("WebApplication not initialized")
        {
        }
        protected                           WebAppNotInitialized(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public  class WebResourceDownException: WebException
    {
        public  override    bool            logError
        {
            get {
                return false;
            }
        }

        public                              WebResourceDownException(string message): base(message)
        {
        }
        protected                           WebResourceDownException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public  class WebRequestException: WebException
    {
        public  override    bool            logError
        {
            get {
                return false;
            }
        }

        public                              WebRequestException(string message): base(message)
        {
        }
        public                              WebRequestException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebRequestException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public  class WebResponseException: WebException
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public                              WebResponseException(string message): base(message)
        {
        }
        public                              WebResponseException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebResponseException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public  class WebBasicAutorizationException: WebException
    {
        public  override    bool            logError
        {
            get {
                return false;
            }
        }

        public                              WebBasicAutorizationException(string message): base(message)
        {
        }
        public                              WebBasicAutorizationException(string message, Exception innerException): base(message, innerException)
        {
        }
        protected                           WebBasicAutorizationException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }

    [Serializable]
    public class InternalErrorException: WebException
    {
        public  override    bool            logError
        {
            get {
                return true;
            }
        }

        public                              InternalErrorException(string message): base(message)
        {
        }
        protected                           InternalErrorException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
