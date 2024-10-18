#if UNITY_EDITOR_OSX && NET_4_6
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2021-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.MacOSSupport
{

	namespace ObjC
	{
		public static class Runtime
		{
			private const string libobjc = "/usr/lib/libobjc.A.dylib";

			private static Architecture _architecture;

			static Runtime()
			{
				_architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
			}

			[DllImport(libobjc)]
			public static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPStr)] string name);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern void objc_msgSend_void(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern void objc_msgSend_void(IntPtr instance, IntPtr sel, CoreGraphics.CGRect arg0, [MarshalAs(UnmanagedType.I1)] bool arg1, [MarshalAs(UnmanagedType.I1)] bool arg2);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern IntPtr objc_msgSend_intptr(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern IntPtr objc_msgSend_intptr(IntPtr instance, IntPtr sel, UInt64 arg0);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern Int32 objc_msgSend_int32(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern Int32 objc_msgSend_int32(IntPtr instance, IntPtr sel, IntPtr arg0);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern UInt32 objc_msgSend_uint32(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern Int64 objc_msgSend_int64(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern UInt64 objc_msgSend_uint64(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern float objc_msgSend_float(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			public static extern double objc_msgSend_double(IntPtr instance, IntPtr sel);

			public static CoreGraphics.CGRect objc_msgSend_cgrect(IntPtr instance, IntPtr sel)
			{
				if (_architecture == Architecture.Arm64)
				{
					return _objc_msgSend_cgrect(instance, sel);
				}
				else
				{
					return _objc_msgSend_st_cgrect(instance, sel);
				}
			}

			[DllImport(libobjc, EntryPoint = "objc_msgSend")]
			private static extern CoreGraphics.CGRect _objc_msgSend_cgrect(IntPtr instance, IntPtr sel);

			[DllImport(libobjc, EntryPoint = "objc_msgSend_stret")]
			private static extern CoreGraphics.CGRect _objc_msgSend_st_cgrect(IntPtr instance, IntPtr sel);

			[DllImport(libobjc)]
			public static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string name);
		}
	}

	namespace Foundation
	{
		using static ObjC.Runtime;

		public class NSObject
		{
			private const string _className = "NSObject";
			private static IntPtr _class;
			private static IntPtr _selRespondsToSelector;

			static NSObject()
			{
				_class = objc_getClass(_className);
				_selRespondsToSelector = sel_registerName("respondsToSelector:");
			}

			protected IntPtr _instance;
			internal IntPtr Instance
			{
				get { return _instance; }
				set { _instance = value; }
			}

			public NSObject()
			{

			}

			protected NSObject(IntPtr instance)
			{
				_instance = instance;
			}

			protected bool RespondsToSelector(IntPtr sel)
			{
				return objc_msgSend_int32(_instance, _selRespondsToSelector, sel) != 0;
			}
		}

		public class NSEnumerator<T> : NSObject, IEnumerator<T> where T : NSObject, new()
		{
			private static IntPtr _selNextObject;
			private static IntPtr _selObjectEnumerator;

			static NSEnumerator()
			{
				_selNextObject = sel_registerName("nextObject");
				_selObjectEnumerator = sel_registerName("objectEnumerator");
			}

			private IntPtr _collectionInstance;
			private T _current = new T();

			internal NSEnumerator(IntPtr collectionInstance)
			{
				_collectionInstance = collectionInstance;
				Reset();
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public T Current
			{
				get
				{
					return _current;
				}
			}

			public bool MoveNext()
			{
				IntPtr nextObject = objc_msgSend_intptr(_instance, _selNextObject);
				_current.Instance = nextObject;
				return nextObject != IntPtr.Zero;
			}

			public void Reset()
			{
				_instance = objc_msgSend_intptr(_collectionInstance, _selObjectEnumerator);
			}

			public void Dispose()
			{

			}
		}

		public class NSArray<T> : NSObject, IEnumerable<T> where T : NSObject, new()
		{
			internal NSArray(IntPtr instance) : base(instance) { }

			IEnumerator IEnumerable.GetEnumerator()
			{
				return (IEnumerator)GetEnumerator();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new NSEnumerator<T>(_instance);
			}
		}

		public class NSString : NSObject
		{
			public enum Encoding : UInt64
			{
				ASCII = 1,
				UTF8 = 4,
				Unicode = 10,
				UTF16BE = 0x90000100,
				UTF16LE = 0x94000100
			}

			private static IntPtr _selCStringUsingEncoding;

			static NSString()
			{
				_selCStringUsingEncoding = sel_registerName("cStringUsingEncoding:");
			}

			internal NSString(IntPtr instance) : base(instance) { }

			public override string ToString()
			{
				IntPtr ptr = objc_msgSend_intptr(_instance, _selCStringUsingEncoding, (UInt64)Encoding.UTF16LE);
				return Marshal.PtrToStringUni(ptr);
			}
		}
	}

	namespace CoreGraphics
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct CGPoint
		{
			public double x;
			public double y;
			public CGPoint(double x, double y) { this.x = x; this.y = y; }
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CGSize
		{
			public double width;
			public double height;
			public CGSize(double width, double height) { this.width = width; this.height = height; }
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CGRect
		{
			public CGPoint Origin;
			public CGSize Size;
			public CGRect(Rect rect)
			{
				Origin = new CGPoint(rect.x, rect.y);
				Size = new CGSize(rect.width, rect.height);
			}
			public Rect ToRect() { return new Rect((float)Origin.x, (float)Origin.y, (float)Size.width, (float)Size.height); }
		}
	}

	namespace AppKit
	{
		using static ObjC.Runtime;
		using Foundation;
		using NSRect = CoreGraphics.CGRect;
		
		public class NSApplication : NSObject
		{
			private static IntPtr _class;
			private static IntPtr _selSharedApplication;
			private static IntPtr _selKeyWindow;

			static NSApplication()
			{
				_class = objc_getClass("NSApplication");
				_selSharedApplication = sel_registerName("sharedApplication");
				_selKeyWindow = sel_registerName("keyWindow");
			}

			private static NSApplication _sharedApplication;
			public static NSApplication SharedApplication
			{
				get
				{
					if (_sharedApplication != null) { return _sharedApplication; }
					IntPtr instance = objc_msgSend_intptr(_class, _selSharedApplication);
					_sharedApplication = new NSApplication(instance);
					return _sharedApplication;
				}
			}

			NSApplication(IntPtr instance) : base(instance) { }

			public NSWindow KeyWindow
			{
				get
				{
					IntPtr window = objc_msgSend_intptr(_instance, _selKeyWindow);
					return new NSWindow(window);
				}
			}
		}

		public class NSWindow : NSObject
		{
			private static IntPtr _selFrame;
			private static IntPtr _selSetFrameDisplayAnimate;

			static NSWindow()
			{
				_selFrame = sel_registerName("frame");
				_selSetFrameDisplayAnimate = sel_registerName("setFrame:display:animate:");
			}

			internal NSWindow(IntPtr instance) : base(instance) { }

			public Rect Frame
			{
				get
				{
					return objc_msgSend_cgrect(_instance, _selFrame).ToRect();
				}
			}

			public void SetFrame(Rect rect, bool display, bool animate)
			{
				objc_msgSend_void(_instance, _selSetFrameDisplayAnimate, new NSRect(rect), display, animate);				
			}
		}

		public class NSScreen : NSObject
		{
			private const string _className = "NSScreen";
			private static IntPtr _class;
			private static IntPtr _selScreens;
			private static IntPtr _selBackingScaleFactor;
			private static IntPtr _selFrame;
			private static IntPtr _selLocalizedName;

			static NSScreen()
			{
				_class = objc_getClass(_className);
				_selScreens = sel_registerName("screens");
				_selBackingScaleFactor = sel_registerName("backingScaleFactor");
				_selFrame = sel_registerName("frame");
				_selLocalizedName = sel_registerName("localizedName");
			}

			public NSScreen()
			{

			}

			public static NSArray<NSScreen> Screens
			{
				get
				{
					IntPtr array = objc_msgSend_intptr(_class, _selScreens);
					return new NSArray<NSScreen>(array);
				}
			}

			public float BackingScaleFactor
			{
				get
				{
					return (float)objc_msgSend_double(_instance, _selBackingScaleFactor);
				}
			}

			public Rect Frame
			{
				get
				{
					return objc_msgSend_cgrect(_instance, _selFrame).ToRect();
				}
			}

			public string LocalizedName
			{
				get
				{
					if (RespondsToSelector(_selLocalizedName))
					{
						NSString nsstring = new NSString(objc_msgSend_intptr(_instance, _selLocalizedName));
						return nsstring.ToString();
					}
					else
					{
						return "Unknown";
					}
				}
			}
		}
	}

}
#endif