using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HkmpVoiceChat.Common.WebRtcVad;

public class NativeMethods {
    static NativeMethods() {
        IntPtr image;
        if (PlatformDetails.IsMac) {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Mac",
                "libwebrtcvad.dylib"
            ));
        } else if (PlatformDetails.IsWindows) {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Windows",
                "webrtcvad.dll"
            ));
        } else {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Linux",
                "libwebrtcvad.so"
            ));
        }
        
        if (image != IntPtr.Zero) {
            var type = typeof(NativeMethods);
            foreach (var member in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic)) {
                var methodName = member.Name;
                var fieldType = member.FieldType;

                var ptr = LibraryLoader.ResolveSymbol(image, methodName);
                if (ptr == IntPtr.Zero) {
                    throw new Exception($"Could not resolve symbol \"{methodName}\"");
                }
                
                member.SetValue(null, Marshal.GetDelegateForFunctionPointer(ptr, fieldType));
            }
        }
    }
    
#pragma warning disable 0649
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnassignedField.Compiler

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr Vad_Create_delegate();

    internal static Vad_Create_delegate Vad_Create;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int Vad_Init_delegate(IntPtr vadInst);

    internal static Vad_Init_delegate Vad_Init;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int Vad_SetMode_delegate(IntPtr vadInst, int mode);

    internal static Vad_SetMode_delegate Vad_SetMode;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int Vad_ValidRateAndFrameLength_delegate(int rate, UIntPtr frameLength);

    internal static Vad_ValidRateAndFrameLength_delegate Vad_ValidRateAndFrameLength;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int Vad_Process_delegate(IntPtr vadInst, int fs, IntPtr audioFrame, UIntPtr frameLength);

    internal static Vad_Process_delegate Vad_Process;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void Vad_Free_delegate(IntPtr vadInst);

    internal static Vad_Free_delegate Vad_Free;
    
    // ReSharper restore UnassignedField.Compiler
    // ReSharper restore InconsistentNaming
#pragma warning restore 0649
}