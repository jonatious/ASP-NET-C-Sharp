using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace slnBackup_copyToProd.Classes
{
    public class CWFSUser : IDisposable
    {

        #region PINVOKE MEMBERS
        System.Security.Principal.WindowsImpersonationContext impersonationContext;

        int LOGON32_LOGON_INTERACTIVE = 2;
        int LOGON32_PROVIDER_DEFAULT = 0;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(
            String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static bool CloseHandle(IntPtr handle);
        #endregion //PINVOKE MEMBERS

        public bool _impersonated;
        public bool _disposed;

        public bool Impersonated
        {
            get { return _impersonated; }
        }

        public CWFSUser(string userName, string domain, string password)
        {
            Initialize(userName, domain, password);
        }

        ~CWFSUser()
        {
            Dispose(false);
        }

        public void Initialize(string userName, string domain, string password)
        {
            _disposed = false;
            ImpersonateUser(userName, domain, password);
        }

        public void ImpersonateUser(string userName, string domain, string password)
        {
            if (!_disposed)
            {
                System.Security.Principal.WindowsIdentity tmpWindowsIdentity;
                IntPtr token = IntPtr.Zero;
                IntPtr duplicateToken = IntPtr.Zero;
                string originalUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                if (RevertToSelf())
                {
                    if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                    {
                        if (DuplicateToken(token, LOGON32_LOGON_INTERACTIVE, ref duplicateToken) != 0)
                        {
                            tmpWindowsIdentity = new WindowsIdentity(duplicateToken);
                            string name = tmpWindowsIdentity.Name;
                            impersonationContext = tmpWindowsIdentity.Impersonate();
                            if (impersonationContext != null)
                            {
                                string newUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                                _impersonated = true;
                            }
                            else
                            {
                                throw new ImpersonationFailedException("Impersonation failed during impersonate for : " + domain + "\\" + userName + " at " + DateTime.Now.ToString());
                            }
                        }
                        else
                        {
                            throw new ImpersonationFailedException("Impersonation failed during token duplication for: " + domain + "\\" + userName + " at " + DateTime.Now.ToString());
                        }
                    }
                    else
                    {
                        throw new ImpersonationFailedException("Impersonation failed during login call for: " + domain + "\\" + userName + " at " + DateTime.Now.ToString());
                    }
                }
                else
                {
                    throw new ImpersonationFailedException("Impersonation failed : Unable to revert to self for: " + domain + "\\" + userName + " at " + DateTime.Now.ToString());
                }
                if (duplicateToken != IntPtr.Zero)
                {
                    CloseHandle(duplicateToken);
                }
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
            }

            return;
        }

        public void UndoImpersonation()
        {
            if (Impersonated)
            {
                impersonationContext.Undo();
                _impersonated = false;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    string originalUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    UndoImpersonation();
                    string newUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                _disposed = true;
            }
        }

        #endregion
    }

    public class ImpersonationFailedException : ApplicationException
    {
        public ImpersonationFailedException()
            : base(string.Empty)
        {
        }

        public ImpersonationFailedException(string message)
            : base(message)
        {
        }
    }

}