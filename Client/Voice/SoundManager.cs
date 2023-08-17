using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace HkmpVoiceChat.Client.Voice;

public class SoundManager {
    public const int SampleRate = 48000;
    public const int BufferSize = SampleRate / 1000 * 20;

    private IntPtr _device;
    private ContextHandle _context;

    private readonly ConcurrentDictionary<ushort, Speaker> _speakers;

    public SoundManager() {
        _speakers = new ConcurrentDictionary<ushort, Speaker>();
    }

    private bool IsClosed => _device == IntPtr.Zero;

    public void Open(string deviceName = null) {
        _device = OpenSpeaker(deviceName);
        _context = Alc.CreateContext(_device, Array.Empty<int>());

        Alc.MakeContextCurrent(_context);
    }

    public void Close() {
        foreach (var speaker in _speakers.Values) {
            speaker.Close();
        }

        _speakers.Clear();

        if (_context != ContextHandle.Zero) {
            Alc.DestroyContext(_context);
            CheckAlcError(_device, 0);
        }

        if (_device != IntPtr.Zero) {
            Alc.CloseDevice(_device);
            CheckAlcError(_device, 1);
        }

        _context = ContextHandle.Zero;
        _device = IntPtr.Zero;
    }

    public void ChangeDevice(string deviceName) {
        var speakerIds = _speakers.Keys;

        Close();

        Open(deviceName);

        foreach (var id in speakerIds) {
            TryGetOrCreateSpeaker(id, out _);
        }
    }

    public bool TryGetOrCreateSpeaker(ushort id, out Speaker speaker) {
        if (IsClosed) {
            speaker = null;
            return false;
        }

        if (!_speakers.TryGetValue(id, out speaker)) {
            speaker = new Speaker();
            speaker.Open();

            _speakers.TryAdd(id, speaker);
        }

        return true;
    }

    public bool TryRemoveSpeaker(ushort id) {
        if (IsClosed) {
            return false;
        }

        if (_speakers.TryRemove(id, out var speaker)) {
            speaker.Close();
        }

        return true;
    }

    private IntPtr OpenSpeaker(string name) {
        try {
            return TryOpenSpeaker(name);
        } catch (Exception) {
            if (name != null) {
                ClientVoiceChat.Logger.Debug($"Failed to open audio channel '{name}', falling back to default");
            }

            try {
                return TryOpenSpeaker(GetDefaultSpeaker());
            } catch (Exception) {
                return TryOpenSpeaker(null);
            }
        }
    }

    private IntPtr TryOpenSpeaker(string name) {
        var device = Alc.OpenDevice(name);
        if (device == IntPtr.Zero) {
            throw new Exception("Failed to open audio device: Audio device not found");
        }

        CheckAlcError(device, 0);
        return device;
    }

    public static string GetDefaultSpeaker() {
        var defaultSpeaker = Alc.GetString(IntPtr.Zero, AlcGetString.DefaultDeviceSpecifier);
        CheckAlcError(IntPtr.Zero, 0);

        return defaultSpeaker;
    }

    public static List<string> GetAllSpeakers() {
        var devices = Alc.GetString(IntPtr.Zero, AlcGetStringList.AllDevicesSpecifier);
        CheckAlcError(IntPtr.Zero, 0);

        return devices == null ? new List<string>() : new List<string>(devices);
    }

    public static bool CheckAlError(int index) {
        var error = AL.GetError();
        if (error == ALError.NoError) {
            return false;
        }

        var stackFrame = new StackFrame(1);
        ClientVoiceChat.Logger.Debug(
            $"VoiceChat sound manager AL error: {stackFrame.GetMethod().DeclaringType}.{stackFrame.GetMethod().Name}[{index}] {error}");

        return true;
    }

    public static bool CheckAlcError(IntPtr device, int index) {
        var error = Alc.GetError(device);
        if (error == AlcError.NoError) {
            return false;
        }

        var stackFrame = new StackFrame(1);
        ClientVoiceChat.Logger.Debug(
            $"VoiceChat sound manager ALC error: {stackFrame.GetMethod().DeclaringType}.{stackFrame.GetMethod().Name}[{index}] {error}");

        return true;
    }
}