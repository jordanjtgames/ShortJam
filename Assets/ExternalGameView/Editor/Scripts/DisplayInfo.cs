using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR_OSX
using RenderHeads.MacOSSupport.AppKit;
#endif

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	///
	/// Enumeration of multiple displays
	///
	public class DisplayInfo
	{
		public bool IsPrimary { get; private set; }
		public string DeviceName { get; private set; }
		public float ScaleFactor { get; private set; }
		public Rect Area { get; private set; }

		static DisplayInfo()
		{
			Refresh();
		}

		public static List<DisplayInfo> Displays { get; private set; }
		public static DisplayInfo Primary { get; private set; }
		public static Rect VirtualArea { get; private set; }

		public static void Refresh()
		{
			Displays = EnumerateDisplays();
			Primary = FindPrimaryDisplay();
			VirtualArea = CalcVirtualArea();
		}

		public static DisplayInfo GetByNameOrDefault(string name)
		{
			if (Displays.Count == 0) { Refresh(); }
			DisplayInfo result = Primary;
			foreach (DisplayInfo display in Displays)
			{
				if (display.DeviceName == name)
				{
					result = display;
					break;
				}
			}
			return result;
		}

		private static DisplayInfo FindPrimaryDisplay()
		{
			DisplayInfo result = null;
			foreach (DisplayInfo display in Displays)
			{
				if (display.IsPrimary)
				{
					result = display;
					break;
				}
			}
			return result;
		}

		private static Rect CalcVirtualArea()
		{
			Rect result = Rect.zero;
			result.min = new Vector2(float.MaxValue, float.MaxValue);
			result.max = new Vector2(float.MinValue, float.MinValue);
			foreach (DisplayInfo display in Displays)
			{
				result.xMin = Mathf.Min(result.xMin, display.Area.xMin);
				result.yMin = Mathf.Min(result.yMin, display.Area.yMin);
				result.xMax = Mathf.Max(result.xMax, display.Area.xMax);
				result.yMax = Mathf.Max(result.yMax, display.Area.yMax);
			}
			return result;
		}

		private static List<DisplayInfo> EnumerateDisplays()
		{
			var displays = new List<DisplayInfo>();

#if UNITY_EDITOR_WIN
			WindowsNative.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
				delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
				{
					WindowsNative.MonitorInfo monitor = new WindowsNative.MonitorInfo();
					monitor.Size = (uint)Marshal.SizeOf(monitor);
					monitor.DeviceName = null;
					if (WindowsNative.GetMonitorInfo(hMonitor, ref monitor))
					{
						DisplayInfo displayinfo = new DisplayInfo();
						displayinfo.IsPrimary = WindowsNative.IsPrimaryMonitor(hMonitor);
						displayinfo.Area = new Rect(monitor.Monitor.Left, monitor.Monitor.Top,  monitor.Monitor.Right - monitor.Monitor.Left, monitor.Monitor.Bottom - monitor.Monitor.Top);
						displayinfo.DeviceName = monitor.DeviceName;
						uint scaleFac = 0;
						WindowsNative.GetScaleFactorForMonitor(hMonitor, out scaleFac);
						displayinfo.ScaleFactor = (float)scaleFac / 100f;
						displays.Add(displayinfo);
						// NOTE: In some versions of Unity the values returns here are scaled by the DPI scaling factor
						// TODO: figure out which versions this is and fix
						// NOTE: Unity 2018.4.0 and above seem fine.  5.6 is not good...
						//Debug.Log(monitor.Monitor.Left + " " +  monitor.Monitor.Top + " " + monitor.Monitor.Right + " " + monitor.Monitor.Bottom);
					}
					return true;
				}, IntPtr.Zero);

				// Find more accurate names if possible
				{
					int index = 0;
					IEnumerable<string> names = ScreenInterrogatory.GetAllMonitorsFriendlyNames();
					foreach (string name in names)
					{
						if (index < displays.Count)
						{
							if (!string.IsNullOrEmpty(name))
							{
								displays[index].DeviceName = name;
							}
							index++;
						}
					}
				}
#elif UNITY_EDITOR_OSX
			foreach (NSScreen screen in NSScreen.Screens)
			{
				DisplayInfo displayInfo = new DisplayInfo();
				displayInfo.IsPrimary = displays.Count == 0;
				displayInfo.Area = screen.Frame;
				displayInfo.DeviceName = screen.LocalizedName;
				displayInfo.ScaleFactor = screen.BackingScaleFactor;
				displays.Add(displayInfo);
			}
			// Need to convert coordinates into unity's
			DisplayInfo primary = displays[0];
			foreach (DisplayInfo displayInfo in displays)
			{
				if (displayInfo.IsPrimary) { continue; }
				Rect area = displayInfo.Area;
				area.y = -area.y + primary.Area.height - displayInfo.Area.height;
				displayInfo.Area = area;
			}
#else

			#if UNITY_2022_1_OR_NEWER
			// TODO: In Unity 2022 can use reflection to access built-in functions for display enumeration:
			//Debug.Log("displays: " + UnityEditor.EditorDisplayUtility.GetNumberOfConnectedDisplays());
			#endif

			var primaryDisplay = new DisplayInfo()
			{
				IsPrimary = true,
				Area = new Rect(0f, 0f, Screen.currentResolution.width / EditorGUIUtility.pixelsPerPoint, Screen.currentResolution.height / EditorGUIUtility.pixelsPerPoint),
				DeviceName = "Default",
				ScaleFactor = 1f,
			};
			displays.Add(primaryDisplay);
#endif

			// Sometimes device can have the same name, and we don't have GUIDs yet, so we need to make sure the
			// name are unique as we search by name.
			// Rename displays that have the same name to make them unique by changing them to format "{DeviceName}.{Count+1}"
			for (int i = 0; i < displays.Count; i++)
			{
				DisplayInfo a = displays[i];
				string baseName = a.DeviceName;
				int count = 0;
				for (int j = i + 1; j < displays.Count; j++)
				{
					DisplayInfo b = displays[j];
					if (baseName == b.DeviceName)
					{
						if (count == 0)
						{
							a.DeviceName = string.Format("{0}.{1}", baseName, count + 1);
						}
						count++;
						b.DeviceName = string.Format("{0}.{1}", baseName, count + 1);
					}
				}
			}

			return displays;
		}
	}
}