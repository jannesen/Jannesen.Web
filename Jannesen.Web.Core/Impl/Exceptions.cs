using System;

namespace Jannesen.Web.Core.Impl
{
    public interface IFileLocation
    {
        string          Filename        { get; }
        int             LineNumber      { get; }
    }

    public abstract class WebException: Exception
    {
        public  abstract    bool            logError        { get; }

        protected                           WebException(string message): base(message)
        {
        }
        protected                           WebException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

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
            ArgumentNullException.ThrowIfNull(configReader);

            Filename   = configReader.Filename;
            LineNumber = configReader.LineNumber;
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    public class WebSourceException: Exception
    {
        public                              WebSourceException(string message): base(message)
        {
        }
        public                              WebSourceException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    public class WebConversionException: Exception
    {
        public                              WebConversionException(string message): base(message)
        {
        }
        public                              WebConversionException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    public class WebInvalidValueException: Exception
    {
        public                              WebInvalidValueException(string message): base(message)
        {
        }
        public                              WebInvalidValueException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

    public class WebHandlerConfigException: Exception
    {
        public                              WebHandlerConfigException(string message): base(message)
        {
        }
        public                              WebHandlerConfigException(string message, Exception innerException): base(message, innerException)
        {
        }

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

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

        public  override    string          Source
        {
            get {
                return "Jannesen.Web.Core";
            }
        }
    }

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
    }

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
    }

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
    }

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
    }

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
    }

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
    }

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
    }
}
