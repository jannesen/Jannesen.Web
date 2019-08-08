using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Jannesen.Web.Core.Impl
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct SystemSection: IDisposable
    {
#pragma warning disable IDE0069 // Not the owner
        private         WindowsIdentity         _currentIdentity;
#pragma warning restore IDE0069

        public          void                    Dispose()
        {
            if (_currentIdentity!=null) {
                WindowsIdentity identity    = _currentIdentity;
                _currentIdentity = null;
                identity.Impersonate();
            }
        }

        public          void                    ToSystem()
        {
            if (_currentIdentity==null) {
                WindowsIdentity Identity = WindowsIdentity.GetCurrent();

                if (!RevertToSelf())
                    throw new SystemException("RevertToSelf failed.");

                _currentIdentity = Identity;
            }
        }

        [DllImport("advapi32")]
        private static extern       bool        RevertToSelf();
    }
#pragma warning restore CA1815
}
