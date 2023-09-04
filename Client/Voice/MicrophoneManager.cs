using System;
using System.Threading;
using HkmpVoiceChat.Common;
using HkmpVoiceChat.Common.Opus;
using HkmpVoiceChat.Common.RNNoise;
using HkmpVoiceChat.Common.WebRtcVad;

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
    private readonly RNNoise _denoiser;
    private readonly WebRtcVad _webRtcVad;

    private Thread _thread;
    private bool _isRunning;
    private Microphone _microphone;

    private bool _activating;
    private byte[] _lastBuff;

    public MicrophoneManager() {
        _encoder = new OpusCodec();
        _denoiser = new RNNoise();
        _webRtcVad = new WebRtcVad {
            SampleRate = SoundManager.SampleRate,
            FrameLength = SoundManager.FrameLength,
            OperatingMode = OperatingMode.Aggressive
        };
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
                    
                    // Adjust volume of mic data based on config value
                    buff = VolumeManager.AmplifyAudioData(buff, VoiceChatMod.ModSettings.MicrophoneAmplification);

                    // Denoise the mic data
                    buff = _denoiser.ProcessFrame(buff);

                    // Convert the mic data to bytes and check whether it contains speech with WebRTC VAD
                    var byteBuff = Utils.ShortsToBytes(buff);
                    var hasSpeech = _webRtcVad.HasSpeech(buff);

                    if (!_activating) {
                        if (hasSpeech) {
                            if (_lastBuff != null) {
                                VoiceDataEvent?.Invoke(_encoder.Encode(_lastBuff));
                            }
                            VoiceDataEvent?.Invoke(_encoder.Encode(byteBuff));
                            
                            _activating = true;
                            ClientVoiceChat.Logger.Debug("Mic buffer has speech, activating");
                        }
                    } else {
                        if (!hasSpeech) {
                            _activating = false;
                            
                            ClientVoiceChat.Logger.Debug("Mic buffer does not have speech, de-activating");
                        } else {
                            VoiceDataEvent?.Invoke(_encoder.Encode(byteBuff));
                        }
                    }

                    _lastBuff = byteBuff;
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