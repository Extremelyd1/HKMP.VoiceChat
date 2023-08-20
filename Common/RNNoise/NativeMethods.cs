using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using HkmpVoiceChat.Client;

namespace HkmpVoiceChat.Common.RNNoise; 

public class NativeMethods {
    static NativeMethods() {
        IntPtr image;
        if (PlatformDetails.IsMac) {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Mac",
                "librnnoise.dylib"
            ));
        } else if (PlatformDetails.IsWindows) {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Windows",
                "rnnoise.dll"
            ));
        } else {
            image = LibraryLoader.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Natives",
                "Linux",
                "librnnoise.so"
            ));
        }
        
        if (image != IntPtr.Zero) {
            ClientVoiceChat.Logger.Info("RNNoise library loaded");
            
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
        } else {
            ClientVoiceChat.Logger.Info("RNNoise library could not be loaded");
        }
    }
    
#pragma warning disable 0649
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnassignedField.Compiler
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int rnnoise_get_frame_size_delegate();

    internal static rnnoise_get_frame_size_delegate rnnoise_get_frame_size;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr rnnoise_create_delegate(IntPtr model);

    internal static rnnoise_create_delegate rnnoise_create;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr rnnoise_destroy_delegate(IntPtr denoiseState);

    internal static rnnoise_destroy_delegate rnnoise_destroy;
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate IntPtr rnnoise_process_frame_delegate(IntPtr denoiseState, float[] processed, float[] input);

    internal static rnnoise_process_frame_delegate rnnoise_process_frame;
    
    // ReSharper restore UnassignedField.Compiler
    // ReSharper restore InconsistentNaming
#pragma warning restore 0649
}