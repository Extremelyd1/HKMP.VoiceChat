using System;
using System.Threading;
using HkmpVoiceChat.Common;
using HkmpVoiceChat.Common.Opus;

namespace HkmpVoiceChat.Client.Voice;

public class MicrophoneManager {
    public event Action<byte[]> VoiceDataEvent;

    public Microphone Microphone {
        private get => _microphone;
        set {
            var oldMic = _microphone;
            _microphone = value;

            oldMic?.Close();
        }
    }

    private readonly OpusCodec _encoder;

    private Thread _thread;
    private bool _isRunning;
    private Microphone _microphone;

    public MicrophoneManager() {
        _encoder = new OpusCodec();
    }

    public void Start() {
        if (_isRunning) {
            Stop();
        }

        _thread = new Thread(() => {
            _isRunning = true;
            while (_isRunning) {
                try {
                    if (Microphone == null) {
                        Thread.Sleep(50);
                        continue;
                    }

                    if (!Microphone.IsOpen) {
                        Microphone.Open();
                    }

                    if (!Microphone.IsStarted) {
                        Microphone.Start();
                    }

                    if (Microphone.Available() < SoundManager.BufferSize) {
                        Thread.Sleep(5);
                        continue;
                    }

                    var buff = Microphone.Read();
                    if (buff == null) {
                        Thread.Sleep(5);
                        continue;
                    }

                    var byteBuff = Utils.ShortsToBytes(buff);
                    var encodedBuff = _encoder.Encode(byteBuff);

                    VoiceDataEvent?.Invoke(encodedBuff);
                } catch (Exception e) {
                    ClientVoiceChat.Logger.Debug($"Error in mic thread:\n{e}");
                }
            }
        });
        _thread.Start();
    }

    public void Stop() {
        if (!_isRunning) {
            return;
        }

        _isRunning = false;

        _thread.Join(50);
        _thread = null;

        Microphone?.Close();
    }
}