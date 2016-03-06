using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Runtime.InteropServices;

namespace Nitridan.MsiLauncher
{
    internal class MsiPackage : IDisposable
    {
        private readonly IntPtr msiHandle;

        private readonly Session session;

        public MsiPackage(string msiPath)
        {
            var uiHandle = IntPtr.Zero;
            NativeMethods.MsiSetInternalUI(NativeMethods.INSTALLUILEVEL.INSTALLUILEVEL_FULL, ref uiHandle);
            var error = NativeMethods.MsiOpenPackageW(msiPath, out msiHandle);
            if (error != 0)
            {
                throw new Exception("Invalid MSI");
            }

            session = Session.FromHandle(msiHandle, true);
        }

        public Session Session => session;

        public Database Database => session.Database;

        public void Dispose()
        {
            if (msiHandle != IntPtr.Zero)
            {
                NativeMethods.MsiCloseHandle(msiHandle);
            }
        }

        private static class NativeMethods
        {
            [DllImport("msi.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
            public static extern UInt32 MsiCloseHandle(IntPtr handle);

            [DllImport("msi.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
            public static extern UInt32 MsiOpenPackageW(string szPackagePath, out IntPtr hProduct);

            [DllImport("msi.dll", SetLastError = true)]
            public static extern UInt32 MsiSetInternalUI(INSTALLUILEVEL dwUILevel, ref IntPtr phWnd);

            public enum INSTALLUILEVEL
            {
                INSTALLUILEVEL_NOCHANGE = 0,    // UI level is unchanged
                INSTALLUILEVEL_DEFAULT = 1,    // default UI is used
                INSTALLUILEVEL_NONE = 2,    // completely silent installation
                INSTALLUILEVEL_BASIC = 3,    // simple progress and error handling
                INSTALLUILEVEL_REDUCED = 4,    // authored UI, wizard dialogs suppressed
                INSTALLUILEVEL_FULL = 5,    // authored UI with wizards, progress, errors
                INSTALLUILEVEL_ENDDIALOG = 0x80, // display success/failure dialog at end of install
                INSTALLUILEVEL_PROGRESSONLY = 0x40, // display only progress dialog
                INSTALLUILEVEL_HIDECANCEL = 0x20, // do not display the cancel button in basic UI
                INSTALLUILEVEL_SOURCERESONLY = 0x100, // force display of source resolution even if quiet
            }
        }
    }
}
