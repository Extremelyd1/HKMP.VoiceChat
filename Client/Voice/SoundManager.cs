using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace HkmpVoiceChat.Client.Voice;

public class SoundManager {
    public const int SampleRate = 48000;
    public const int BufferSize = SampleRate / 1000 * 20;
    
    private readonly string _deviceName;

    private IntPtr _device;
    private ContextHandle _context;

    public bool IsClosed => _device == IntPtr.Zero;

    public SoundManager(string deviceName) {
        _deviceName = deviceName;
    }

    public void Initialize() {
        _device = OpenSpeaker(_deviceName);
        _context = Alc.CreateContext(_device, Array.Empty<int>());

        Alc.MakeContextCurrent(_context);
    }

    public void Close() {
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
        
        ClientVoiceChat.Logger.Debug($"GetDefaultSpeaker result: {defaultSpeaker}");

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
        ClientVoiceChat.Logger.Debug($"Voicechat sound manager AL error: {stackFrame.GetMethod().DeclaringType}.{stackFrame.GetMethod().Name}[{stackFrame.GetFileLineNumber()}/{index}] {error}");
        
        return true;
    }
    
    public static bool CheckAlcError(IntPtr device, int index) {
        var error = Alc.GetError(device);
        if (error == AlcError.NoError) {
            return false;
        }

        var stackFrame = new StackFrame(1);
        ClientVoiceChat.Logger.Debug($"Voicechat sound manager ALC error: {stackFrame.GetMethod().DeclaringType}.{stackFrame.GetMethod().Name}[{stackFrame.GetFileLineNumber()}/{index}] {error}");
        
        return true;
    }
}