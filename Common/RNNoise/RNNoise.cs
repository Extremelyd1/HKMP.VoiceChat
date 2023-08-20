using System;

namespace HkmpVoiceChat.Common.RNNoise; 

public class RNNoise : IDisposable {
    private IntPtr _handle;

    public RNNoise() {
        _handle = NativeMethods.rnnoise_create(IntPtr.Zero);
    }

    public short[] ProcessFrame(short[] data) {
        var frameSize = NativeMethods.rnnoise_get_frame_size();
        
        var floatData = new float[data.Length];
        for (var i = 0; i < data.Length; i++) {
            floatData[i] = data[i];
        }
        
        var processedFloatData = new float[floatData.Length];

        for (var i = 0; i < floatData.Length; i += frameSize) {
            var input = new float[frameSize];
            for (var j = 0; j < frameSize; j++) {
                input[j] = floatData[i + j];
            }

            var output = new float[frameSize];

            NativeMethods.rnnoise_process_frame(_handle, output, input);

            for (var j = 0; j < frameSize; j++) {
                processedFloatData[i + j] = output[j];
            }
        }

        var max = float.MinValue;
        var min = float.MaxValue;

        foreach (var f in processedFloatData) {
            if (f > max) {
                max = f;
            }

            if (f < min) {
                min = f;
            }
        }

        const float FloatShortScale = short.MaxValue - 1; 
        var scale = Math.Min(1f, FloatShortScale / Math.Max(Math.Abs(max), Math.Abs(min)));

        var processedShortData = new short[processedFloatData.Length];
        for (var i = 0; i < processedFloatData.Length; i++) {
            processedShortData[i] = (short) (processedFloatData[i] * scale);
        }

        return processedShortData;
    }
    
    private bool _disposed;

    public void Dispose() {
        if (_disposed) {
            return;
        }

        if (_handle != IntPtr.Zero) {
            NativeMethods.rnnoise_destroy(_handle);
            _handle = IntPtr.Zero;
        }

        _disposed = true;
    }
}