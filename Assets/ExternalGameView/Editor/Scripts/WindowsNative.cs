#if UNITY_EDITOR_WIN
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.ExternalGameView.Editor
{
	///
	/// Native Windows functions
	///
	internal static class WindowsNative
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetActiveWindow();

		[Flags]
		public enum SetWindowPosFlags : uint
		{
			SWP_NOZORDER = 0x0004,
			SWP_NOSIZE = 0x0001,
			SWP_NOACTIVATE = 0x0010,
		}

		[DllImport("user32.dll", SetLastError=true)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);		

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

		public enum MonitorOptions : uint
		{
			MONITOR_DEFAULTTONULL = 0x00000000,
			MONITOR_DEFAULTTOPRIMARY = 0x00000001,
			MONITOR_DEFAULTTONEAREST = 0x00000002
		}

		public static bool IsPrimaryMonitor(IntPtr hMonitor)
		{
			return (hMonitor == MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY));
		}

		public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

		[Flags()]
		public enum DisplayDeviceStateFlags : int
		{
			/// <summary>The device is part of the desktop.</summary>
			AttachedToDesktop = 0x1,
			MultiDriver = 0x2,
			/// <summary>The device is part of the desktop.</summary>
			PrimaryDevice = 0x4,
			/// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
			MirroringDriver = 0x8,
			/// <summary>The device is VGA compatible.</summary>
			VGACompatible = 0x10,
			/// <summary>The device is removable; it cannot be the primary display.</summary>
			Removable = 0x20,
			/// <summary>The device has more display modes than its output devices support.</summary>
			ModesPruned = 0x8000000,
			Remote = 0x4000000,
			Disconnect = 0x2000000
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public struct DISPLAY_DEVICE
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cb;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
			public string DeviceName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			public string DeviceString;
			[MarshalAs(UnmanagedType.U4)]
			public DisplayDeviceStateFlags StateFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			public string DeviceID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			public string DeviceKey;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

		[StructLayout(LayoutKind.Sequential)]
		public struct RectNative
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct MonitorInfo
		{
			public uint Size;
			public RectNative Monitor;
			public RectNative WorkArea;
			public uint Flags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DeviceName;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo monitorinfo);

		[DllImport("shcore.dll")]
		public static extern void GetScaleFactorForMonitor(IntPtr hMon, out uint pScale);
	}
}

public static class ScreenInterrogatory
{
	public const int ERROR_SUCCESS = 0;

	#region enums

	public enum QUERY_DEVICE_CONFIG_FLAGS : uint
	{
		QDC_ALL_PATHS = 0x00000001,
		QDC_ONLY_ACTIVE_PATHS = 0x00000002,
		QDC_DATABASE_CURRENT = 0x00000004
	}

	public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
	{
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 0xFFFFFFFF,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = 0x80000000,
		DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCE_UINT32 = 0xFFFFFFFF
	}

	public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
	{
		DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
		DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
		DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
		DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
		DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
		DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32 = 0xFFFFFFFF
	}

	public enum DISPLAYCONFIG_ROTATION : uint
	{
		DISPLAYCONFIG_ROTATION_IDENTITY = 1,
		DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
		DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
		DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
		DISPLAYCONFIG_ROTATION_FORCE_UINT32 = 0xFFFFFFFF
	}

	public enum DISPLAYCONFIG_SCALING : uint
	{
		DISPLAYCONFIG_SCALING_IDENTITY = 1,
		DISPLAYCONFIG_SCALING_CENTERED = 2,
		DISPLAYCONFIG_SCALING_STRETCHED = 3,
		DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
		DISPLAYCONFIG_SCALING_CUSTOM = 5,
		DISPLAYCONFIG_SCALING_PREFERRED = 128,
		DISPLAYCONFIG_SCALING_FORCE_UINT32 = 0xFFFFFFFF
	}

	public enum DISPLAYCONFIG_PIXELFORMAT : uint
	{
		DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
		DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
		DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
		DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
		DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
		DISPLAYCONFIG_PIXELFORMAT_FORCE_UINT32 = 0xffffffff
	}

	public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
	{
		DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
		DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
		DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xFFFFFFFF
	}

	public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
	{
		DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
		DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
		DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
		DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
		DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
		DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
		DISPLAYCONFIG_DEVICE_INFO_FORCE_UINT32 = 0xFFFFFFFF
	}

	#endregion

	#region structs

	[StructLayout(LayoutKind.Sequential)]
	public struct LUID
	{
		public uint LowPart;
		public int HighPart;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_PATH_SOURCE_INFO
	{
		public LUID adapterId;
		public uint id;
		public uint modeInfoIdx;
		public uint statusFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_PATH_TARGET_INFO
	{
		public LUID adapterId;
		public uint id;
		public uint modeInfoIdx;
		private DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
		private DISPLAYCONFIG_ROTATION rotation;
		private DISPLAYCONFIG_SCALING scaling;
		private DISPLAYCONFIG_RATIONAL refreshRate;
		private DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
		public bool targetAvailable;
		public uint statusFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_RATIONAL
	{
		public uint Numerator;
		public uint Denominator;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_PATH_INFO
	{
		public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
		public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
		public uint flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_2DREGION
	{
		public uint cx;
		public uint cy;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
	{
		public ulong pixelRate;
		public DISPLAYCONFIG_RATIONAL hSyncFreq;
		public DISPLAYCONFIG_RATIONAL vSyncFreq;
		public DISPLAYCONFIG_2DREGION activeSize;
		public DISPLAYCONFIG_2DREGION totalSize;
		public uint videoStandard;
		public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_TARGET_MODE
	{
		public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct POINTL
	{
		private int x;
		private int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_SOURCE_MODE
	{
		public uint width;
		public uint height;
		public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
		public POINTL position;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct DISPLAYCONFIG_MODE_INFO_UNION
	{
		[FieldOffset(0)]
		public DISPLAYCONFIG_TARGET_MODE targetMode;

		[FieldOffset(0)]
		public DISPLAYCONFIG_SOURCE_MODE sourceMode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_MODE_INFO
	{
		public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
		public uint id;
		public LUID adapterId;
		public DISPLAYCONFIG_MODE_INFO_UNION modeInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
	{
		public uint value;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
	{
		public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
		public uint size;
		public LUID adapterId;
		public uint id;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
	{
		public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
		public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
		public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
		public ushort edidManufactureId;
		public ushort edidProductCodeId;
		public uint connectorInstance;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string monitorFriendlyDeviceName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string monitorDevicePath;
	}

	#endregion

	#region DLL-Imports

	[DllImport("user32.dll")]
	public static extern int GetDisplayConfigBufferSizes(
		QUERY_DEVICE_CONFIG_FLAGS flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

	[DllImport("user32.dll")]
	public static extern int QueryDisplayConfig(
		QUERY_DEVICE_CONFIG_FLAGS flags,
		ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray,
		ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray,
		IntPtr currentTopologyId
		);

	[DllImport("user32.dll")]
	public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName);

	#endregion

	public static string MonitorFriendlyName(LUID adapterId, uint targetId)
	{
		var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
		{
			header =
			{
				size = (uint)Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
				adapterId = adapterId,
				id = targetId,
				type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
			}
		};
		var error = DisplayConfigGetDeviceInfo(ref deviceName);
		if (error != ERROR_SUCCESS)
			throw new Win32Exception(error);
		return deviceName.monitorFriendlyDeviceName;
	}

	public static IEnumerable<string> GetAllMonitorsFriendlyNames()
	{
		uint pathCount, modeCount;
		var error = GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
		if (error != ERROR_SUCCESS)
			throw new Win32Exception(error);

		var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
		var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
		error = QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
			ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);
		if (error != ERROR_SUCCESS)
			throw new Win32Exception(error);

		for (var i = 0; i < modeCount; i++)
			if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
				yield return MonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id);
	}
}
#endif