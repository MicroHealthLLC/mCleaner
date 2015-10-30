using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace mCleaner.Helpers
{
    public class Permissions
    {
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        public static void SetPrivileges(bool Enabled)
        {
            SetPrivilege("SeShutdownPrivilege", Enabled);
            SetPrivilege("SeBackupPrivilege", Enabled);
            SetPrivilege("SeRestorePrivilege", Enabled);
            SetPrivilege("SeDebugPrivilege", Enabled);
        }

        public static bool SetPrivilege(string privilege, bool enabled)
        {
            try
            {
                TokPriv1Luid tp = new TokPriv1Luid();
                IntPtr hproc = Process.GetCurrentProcess().Handle;
                IntPtr htok = IntPtr.Zero;

                if (!OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
                    return false;

                if (!LookupPrivilegeValue(null, privilege, ref tp.Luid))
                    return false;

                tp.Count = 1;
                tp.Luid = 0;
                tp.Attr = ((enabled) ? (SE_PRIVILEGE_ENABLED) : (0));

                bool bRet = (AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero));

                // Cleanup
                CloseHandle(htok);

                return bRet;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the user is an admin
        /// </summary>
        /// <returns>True if it is in admin group</returns>
        public static bool IsUserAdministrator
        {
            get
            {
                //bool value to hold our return value
                bool isAdmin;
                try
                {
                    //get the currently logged in user
                    WindowsIdentity user = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(user);
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (UnauthorizedAccessException ex)
                {
                    isAdmin = false;
#if (DEBUG)
                    throw ex;
#endif
                }
                catch (Exception ex)
                {
                    isAdmin = false;
#if (DEBUG)
                    throw ex;
#endif
                }
                return isAdmin;
            }
        }
    }
}
