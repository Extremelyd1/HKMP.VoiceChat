using System;
using System.Threading;
using HkmpVoiceChat.Common;
using HkmpVoiceChat.Common.Opus;

namespace HkmpVoiceChat.Client.Voice;

public class MicThread {
    public event Action<byte[]> VoiceDataEvent;

    private readonly OpusCodec _encoder;

    private Thread _thread;
    private bool _isRunning;

    private Microphone _mic;

    public MicThread() {
        _encoder = new OpusCodec();
    }

    public void Start() {
        ClientVoiceChat.Logger.Debug("Starting mic thread");
        
        if (_isRunning) {
            ClientVoiceChat.Logger.Debug("Was running, stopping first");
            Stop();
        }

        ClientVoiceChat.Logger.Debug("Creating thread");
        _thread = new Thread(() => {
            ClientVoiceChat.Logger.Debug("Thread start");
            _mic = new Microphone(null);
            ClientVoiceChat.Logger.Debug("Created microphone");
            _mic.Open();
            ClientVoiceChat.Logger.Debug("Opened microphone");

            _isRunning = true;
            while (_isRunning) {
                ClientVoiceChat.Logger.Debug("Inside while loop");
                try {
                    if (!_mic.IsStarted) {
                        ClientVoiceChat.Logger.Debug("Mic was not started, starting now");
                        _mic.Start();
                    }

                    if (_mic.Available() < SoundManager.BufferSize) {
                        ClientVoiceChat.Logger.Debug($"Not enough available samples: {_mic.Available()}");
                        Thread.Sleep(5);
                        continue;
                    }

                    var buff = _mic.Read();
                    if (buff == null) {
                        ClientVoiceChat.Logger.Debug("Mic buffer is null");
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
        
        ClientVoiceChat.Logger.Debug("Stopping mic thread");

        _isRunning = false;

        _thread.Join(50);
        _thread = null;

        _mic.Close();
    }
}