using System;

namespace HkmpVoiceChat.Common.WebRtcVad; 

public class WebRtcVad : IDisposable {
    private IntPtr _handle;

    private int _sampleRate;
    private int _frameLength;

    private OperatingMode _mode;

    public int SampleRate {
        get => _sampleRate;
        set {
            if (!ValidateRateAndFrameLength(value, _frameLength)) {
                throw new InvalidOperationException("Invalid sample rate");
            }

            _sampleRate = value;
        }
    }

    public int FrameLength {
        get => _frameLength;
        set {
            if (!ValidateRateAndFrameLength(_sampleRate, value)) {
                throw new InvalidOperationException("Invalid frame length");
            }

            _frameLength = value;
        }
    }

    public OperatingMode OperatingMode {
        get => _mode;
        set {
            var result = NativeMethods.Vad_SetMode(_handle, (int) value);
            if (result != 0) {
                throw new InvalidOperationException("Invalid operating mode specified");
            }

            _mode = value;
        }
    }

    public WebRtcVad() {
        _sampleRate = 48000;
        _frameLength = 20;

        _mode = OperatingMode.HighQuality;
        
        _handle = NativeMethods.Vad_Create();
        var result = NativeMethods.Vad_Init(_handle);
        if (result != 0) {
            throw new InvalidOperationException("Could not initialize WebRtcVad");
        }
    }

    public bool HasSpeech(short[] audioFrame) {
        return HasSpeech(audioFrame, _sampleRate, _frameLength);
    }

    private unsafe bool HasSpeech(short[] audioFrame, int sampleRate, int frameLength) {
        var samples = CalculateSamples(sampleRate, frameLength);

        int result;
        fixed (short* framePtr = audioFrame) {
            result = NativeMethods.Vad_Process(_handle, sampleRate, (IntPtr) framePtr, (UIntPtr) samples);
        }

        return result == 1;
    }

    private bool ValidateRateAndFrameLength(int sampleRate, int frameLength) {
        var samples = CalculateSamples(sampleRate, frameLength);
        
        return NativeMethods.Vad_ValidRateAndFrameLength(sampleRate, (UIntPtr) samples) == 0;
    }

    private static int CalculateSamples(int sampleRate, int frameLength) {
        return sampleRate / 1000 * frameLength;
    }

    private bool _disposed;

    public void Dispose() {
        if (_disposed) {
            return;
        }

        if (_handle != IntPtr.Zero) {
            NativeMethods.Vad_Free(_handle);
            _handle = IntPtr.Zero;
        }

        _disposed = true;
    }
}