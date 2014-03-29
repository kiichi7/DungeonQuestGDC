// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Helpers
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:42

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
#if !XBOX360
using System.Security.Principal;
#endif
using System.Globalization;
#endregion

namespace DungeonQuest.Helpers
{
	/// <summary>
	/// Windows helper
	/// </summary>
	class WindowsHelper
	{
		#region NativeMethods helper class (required for FxCop checks)
#if DEBUG && !XBOX360
		/// <summary>
		/// NativeMethods helper class (required for FxCop checks)
		/// </summary>
		internal class NativeMethods
		{
			/// <summary>
			/// Don't allow instantiating this class.
			/// </summary>
			private NativeMethods()
			{
			} // NativeMethods()

			/// <summary>
			/// Win32 function: Get foreground window handle (hwnd)
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern int GetForegroundWindow();

			/// <summary>
			/// Win32 function: Set foreground window handle
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern int SetForegroundWindow(int hWnd);

			/// <summary>
			/// Win 32 function: Get window thread process id
			/// </summary>
			/// <param name="window">Window handle</param>
			/// <param name="processId">Process id</param>
			/// <returns>Thread id</returns>
			[DllImport("user32.dll")]
			internal static extern int GetWindowThreadProcessId(
				int window, int processId);

			/// <summary>
			/// Win32 function: Attaches input to a thread for controlling windows
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern int AttachThreadInput(
				int idAttach, int idAttachTo, int fAttach);

			/// <summary>
			/// Win32 function: Checks if a window is iconic (on the task bar)
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern int IsIconic(int hWnd);
			/// <summary>
			/// Win32 function: Show window with nCmdShow parameters.
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern int ShowWindow(int hWnd, int nCmdShow);

			/// <summary>
			/// Win32 function: Get device context of window handle (hwnd).
			/// </summary>
			[DllImport("User32.dll")]
			internal static extern IntPtr GetWindowDC(IntPtr hwnd);
		} // class NativeMethods
#endif
		#endregion

		#region Constructor
		/// <summary>
		/// Don't allow instantiating this class.
		/// </summary>
		private WindowsHelper()
		{
		} // WindowsHelper()
		#endregion

		#region Get and set foreground window
#if DEBUG && !XBOX360
		// Only supported in debug mode and in windows

		/// <summary>
		/// Win32 constants
		/// </summary>
		public const int
			SwShow = 5,
			SwRestore = 9;

		/// <summary>
		/// Helper function to force a window into foreground, will
		/// even work if our process is not the same as the other windows.
		/// We will try to get the others thread input process and
		/// attach it to our thread for setting the window to the foreground.
		/// </summary>
		public static bool ForceForegroundWindow(int hWnd)
		{
			int foregroundWnd = NativeMethods.GetForegroundWindow();
			// Do nothing if already in foreground.
			if (hWnd == foregroundWnd)
				return true;

			// First need to get the thread responsible for this window,
			// and the thread for the foreground window.
			int ret = 0,
				threadID1 = NativeMethods.GetWindowThreadProcessId(foregroundWnd, 0),
				threadID2 = NativeMethods.GetWindowThreadProcessId(hWnd, 0);

			// By sharing input state, threads share their concept of
			// the active window.
			if (threadID1 != threadID2)
			{
				NativeMethods.AttachThreadInput(threadID1, threadID2, 1);//true
				ret = NativeMethods.SetForegroundWindow(hWnd);
				NativeMethods.AttachThreadInput(threadID1, threadID2, 0);//false
			} // if (threadID1)
			else
				ret = NativeMethods.SetForegroundWindow(hWnd);

			// Restore and repaint
			if (NativeMethods.IsIconic(hWnd) != 0)
				NativeMethods.ShowWindow(hWnd, SwRestore);
			else
				NativeMethods.ShowWindow(hWnd, SwShow);

			// Succeeded
			return ret != 0;
		} // ForceForegroundWindow(hWnd)
#endif
		#endregion
		
		#region Get default player name
		/// <summary>
		/// Get default player name from windows identity, and if
		/// this failes use computer name!
		/// </summary>
		static public string GetDefaultPlayerName()
		{
			string defaultPlayerName = "Player";
#if !XBOX360
			try
			{
				defaultPlayerName = WindowsIdentity.GetCurrent().Name;

				if (String.IsNullOrEmpty(defaultPlayerName))
				{
					defaultPlayerName = "Player";
				} // if (defaultPlayerName)
				else
				{
					// Windows will return name in format <computername>\<username>,
					// we just want the username!
					string[] nameInfo = defaultPlayerName.Split(
						new char[] { '\\' }, 2);
					if (nameInfo.Length >= 2)
					{
						defaultPlayerName = nameInfo[1];
					} // if
				} // else
			} // try
			catch { } // Ignore any error
#endif

			return defaultPlayerName;
		} // GetDefaultPlayerName()
		#endregion
	} // class WindowsHelper
} // namespace DungeonQuest.Helpers
