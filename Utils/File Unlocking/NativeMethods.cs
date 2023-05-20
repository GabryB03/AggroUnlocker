﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeadLock.Classes
{
    /// <summary>
    /// Collection of native methods.
    /// </summary>
    internal static class NativeMethods
    {
        #region Variables
        private const int RmRebootReasonNone = 0;
        private const int CchRmMaxAppName = 255;
        private const int CchRmMaxSvcName = 63;
        #endregion

        #region Enum_Struct
        [StructLayout(LayoutKind.Sequential)]
        private struct RmUniqueProcess
        {
            internal readonly int dwProcessId;
            private readonly System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        private enum RmAppType
        {
            // ReSharper disable once UnusedMember.Local
            RmUnknownApp = 0,
            // ReSharper disable once UnusedMember.Local
            RmMainWindow = 1,
            // ReSharper disable once UnusedMember.Local
            RmOtherWindow = 2,
            // ReSharper disable once UnusedMember.Local
            RmService = 3,
            // ReSharper disable once UnusedMember.Local
            RmExplorer = 4,
            // ReSharper disable once UnusedMember.Local
            RmConsole = 5,
            // ReSharper disable once UnusedMember.Local
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct RmProcessInfo
        {
            internal RmUniqueProcess Process;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxAppName + 1)]
            private readonly string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxSvcName + 1)]
            private readonly string strServiceShortName;
            private readonly RmAppType ApplicationType;

            private readonly uint AppStatus;
            private readonly uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            private readonly bool bRestartable;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Shellexecuteinfo
        {
            public int cbSize;
            public uint fMask;
            private readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpDirectory;
            public int nShow;
            private readonly IntPtr hInstApp;
            private readonly IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpClass;

            private readonly IntPtr hkeyClass;
            private readonly uint dwHotKey;
            private readonly IntPtr hIcon;
            private readonly IntPtr hProcess;
        }
        #endregion

        #region DllImport
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames, uint nApplications, [In] RmUniqueProcess[] rgApplications, uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In, Out] RmProcessInfo[] rgAffectedApps, ref uint lpdwRebootReasons);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref Shellexecuteinfo lpExecInfo);
        #endregion

        /// <summary>
        /// Find the processes that are locking a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="language">Current language to implement localized messages</param>
        /// <returns>A collection of processes that are locking a file.</returns>
        internal static IEnumerable<Process> FindLockingProcesses(string path)
        {
            uint handle;
            string key = Guid.NewGuid().ToString();
            List<Process> processes = new List<Process>();

            int res = RmStartSession(out handle, 0, key);
            if (res != 0)
            {
                return null;
            }

            try
            {
                const int errorMoreData = 234;
                uint pnProcInfoNeeded;
                uint pnProcInfo = 0;
                uint lpdwRebootReasons = RmRebootReasonNone;

                string[] resources = { path };
                res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (res != 0)
                {
                    return null;
                }

                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (res == errorMoreData)
                {
                    RmProcessInfo[] processInfo = new RmProcessInfo[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if (res == 0)
                    {
                        processes = new List<Process>((int)pnProcInfo);

                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            try
                            {
                                processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                            }
                            catch (ArgumentException) { }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (res != 0)
                {
                    return null;
                }
            }
            finally
            {
                RmEndSession(handle);
            }
            return processes;
        }

        /// <summary>
        /// Show the properties dialog of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns></returns>
        internal static void ShowFileProperties(string path)
        {
            Shellexecuteinfo info = new Shellexecuteinfo();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = path;
            info.nShow = 5;
            info.fMask = 12;
            ShellExecuteEx(ref info);
        }
    }
}