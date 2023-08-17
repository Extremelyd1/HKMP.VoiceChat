using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Audio.OpenAL;

namespace HkmpVoiceChat.Client.Voice;

public class Microphone {
    private readonly string _deviceName;

    private IntPtr _device;
    private bool _started;

    public bool IsOpen => _device != IntPtr.Zero;
    public bool IsStarted => _started;

    public Microphone(string deviceName) {
        _device = IntPtr.Zero;
        _deviceName = deviceName;
    }

    public void Open() {
        if (IsOpen) {
            throw new Exception("Microphone already open");
        }

        _device = OpenMic(_deviceName);
    }

    public void Start() {
        if (!IsOpen) {
            return;
        }

        if (_started) {
            return;
        }

        Alc.CaptureStart(_device);
        SoundManager.CheckAlcError(_device, 0);
        _started = true;
    }

    public void Stop() {
        if (!IsOpen) {
            return;
        }

        if (!_started) {
            return;
        }

        Alc.CaptureStop(_device);
        SoundManager.CheckAlcError(_device, 0);
        _started = false;

        var available = Available();
        var buff = new short[available];
        var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);

        try {
            Alc.CaptureSamples(_device, handle.AddrOfPinnedObject(), buff.Length);
            SoundManager.CheckAlcError(_device, 1);
        } catch (Exception e) {
            ClientVoiceChat.Logger.Debug($"Exception while capturing samples:\n{e}");
        } finally {
            handle.Free();
        }
    }

    public void Close() {
        if (!IsOpen) {
            return;
        }

        Stop();
        Alc.CaptureCloseDevice(_device);
        SoundManager.CheckAlcError(_device, 0);
        _device = IntPtr.Zero;
    }

    public int Available() {
        Alc.GetInteger(_device, AlcGetInteger.CaptureSamples, 1, out var samples);
        SoundManager.CheckAlcError(_device, 0);

        return samples;
    }

    public short[] Read() {
        var available = Available();
        if (available < SoundManager.BufferSize) {
            throw new InvalidOperationException(
                $"Failed to read from microphone: Capacity {SoundManager.BufferSize}, available {available}");
        }

        var buff = new short[SoundManager.BufferSize];
        var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);

        try {
            Alc.CaptureSamples(_device, handle.AddrOfPinnedObject(), buff.Length);
            SoundManager.CheckAlcError(_device, 0);
        } catch (Exception e) {
            ClientVoiceChat.Logger.Debug($"Exception while capturing samples:\n{e}");
        } finally {
            handle.Free();
        }

        return buff;
    }

    private IntPtr OpenMic(string name) {
        try {
            return TryOpenMic(name);
        } catch (Exception) {
            if (name != null) {
                ClientVoiceChat.Logger.Debug($"Failed to open microphone '{name}', falling back to default microphone");
            }

            try {
                return TryOpenMic(GetDefaultMicrophone());
            } catch (Exception) {
                return TryOpenMic(null);
            }
        }
    }

    private IntPtr TryOpenMic(string name) {
        var device = Alc.CaptureOpenDevice(name, SoundManager.SampleRate, ALFormat.Mono16, SoundManager.BufferSize);
        if (device == IntPtr.Zero) {
            SoundManager.CheckAlcError(IntPtr.Zero, 0);
            throw new Exception("Failed to open microphone");
        }

        return device;
    }

    public static string GetDefaultMicrophone() {
        var mic = Alc.GetString(IntPtr.Zero, AlcGetString.CaptureDefaultDeviceSpecifier);
        SoundManager.CheckAlcError(IntPtr.Zero, 0);

        return mic;
    }

    public static List<string> GetAllMicrophones() {
        var devices = Alc.GetString(IntPtr.Zero, AlcGetStringList.CaptureDeviceSpecifier);
        SoundManager.CheckAlcError(IntPtr.Zero, 0);

        return devices == null ? new List<string>() : new List<string>(devices);
    }
}