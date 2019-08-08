using System;
using System.Web;

namespace Jannesen.Web.Core
{
    public class ApplModule: IHttpModule
    {
        private     static readonly     object      _lock = new object();
        private     static              int         _usage;

        public                          void        Dispose()
        {
            int     usage;

            lock(_lock) {
                usage = --_usage;
            }

            if (usage == 0)
                WebApplication.AppDomainUnload();
        }
        public                          void        Init(HttpApplication application)
        {
            int     usage;

            lock(_lock) {
                usage = _usage++;
            }

            if (usage==0)
                WebApplication.AppDomainInitialize();
        }
    }
}
