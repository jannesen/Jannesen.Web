using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Jannesen.Web.Core.Impl
{
    public struct SystemSection:    IDisposable
    {
        private         WindowsIdentity         _currentIdentity;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")] // Just Ignore
        [DllImport("advapi32")]
        private static extern       bool        RevertToSelf();
    }
}
