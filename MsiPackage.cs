using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Evil.MsiLauncher
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

        public Session Session
        {
            get
            {
                return session;
            }
        }

        public Database Database
        {
            get
            {
                return session.Database;
            }
        }

        public void Dispose()
        {
            if (msiHandle != IntPtr.Zero)
            {
                NativeMethods.MsiCloseHandle(msiHandle);
            }
        }

        public IDictionary<string, string> GetRuntimeProperties()
        {
            var properties = new Dictionary<string, string>();
            using (var view = session.Database.OpenView("SELECT * FROM `_Property`"))
            {
                view.Execute();
                foreach (var record in view)
                {
                    properties.Add(record[1].ToString(), record[2].ToString());
                }
            }

            return properties;
        }

        public SequenceItem[] GetUiSequence()
        {
            SequenceItem[] sequenceItems;
            using (var view = session.Database.OpenView("SELECT * FROM `InstallUISequence` ORDER BY `Sequence`"))
            {
                view.Execute();
                sequenceItems = view.Select(x =>
                {
                    int sequence;
                    return new SequenceItem
                    {
                        Action = x["Action"].ToString(),
                        Condition = x["Condition"].ToString(),
                        Sequence = int.TryParse(x["Sequence"].ToString(), out sequence) ? sequence : 0
                    };
                }).Where(x => x.Sequence > 0).ToArray();
            }

            return sequenceItems;
        }

        #region Native methods

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

        #endregion
    }
}
