using System;
using System.Collections.Generic;
using System.Linq;
using Hkmp.Api.Client;
using Hkmp.Api.Command.Client;
using HkmpVoiceChat.Client.Voice;

namespace HkmpVoiceChat.Client; 

public class VoiceChatCommand : IClientCommand {
    /// <inheritdoc />
    public string Trigger => "/voicechat";

    /// <inheritdoc />
    public string[] Aliases => new[] { "/vc", "/voice" };

    public event Action<string> SetMicrophoneEvent;
    public event Action<string> SetSpeakerEvent;

    private readonly IChatBox _chatBox;

    private readonly Dictionary<int, string> _microphoneNames;
    private readonly Dictionary<int, string> _speakerNames;

    public VoiceChatCommand(IChatBox chatBox) {
        _chatBox = chatBox;
        _microphoneNames = new Dictionary<int, string>();
        _speakerNames = new Dictionary<int, string>();
    }

    /// <inheritdoc />
    public void Execute(string[] args) {
        void SendUsage() {
            _chatBox.AddMessage($"Invalid usage: {Trigger} <list|set>");
        }
        
        if (args.Length < 2) {
            SendUsage();
            return;
        }

        var action = args[1];
        if (action == "list") {
            HandleList(args);
        } else if (action == "set") {
            HandleSet(args);
        }
    }

    private void HandleList(string[] args) {
        void SendUsage() {
            _chatBox.AddMessage($"Invalid usage: {Trigger} <list> <mics|speakers>");
        }
        
        if (args.Length < 3) {
            SendUsage();
            return;
        }

        var type = args[2];
        if (type is "mics" or "mic") {
            var mics = Microphone.GetAllMicrophones();
            if (mics.Count == 0) {
                _chatBox.AddMessage("No microphones could be found");
                return;
            }
            
            _microphoneNames.Clear();
            
            _chatBox.AddMessage("Microphones (id, name):");
            
            var index = 1;

            foreach (var mic in mics) {
                _chatBox.AddMessage($"{index}: {mic}");

                _microphoneNames[index++] = mic;
            }
        } else if (type is "speakers" or "speaker") {
            var speakers = SoundManager.GetAllSpeakers();
            if (speakers.Count == 0) {
                _chatBox.AddMessage("No speakers could be found");
                return;
            }
            
            _speakerNames.Clear();
            
            _chatBox.AddMessage("Speakers (id, name):");
            
            var index = 1;

            foreach (var speaker in speakers) {
                _chatBox.AddMessage($"{index}: {speaker}");

                _speakerNames[index++] = speaker;
            }
        } else {
            SendUsage();
        }
    }

    private void HandleSet(string[] args) {
        void SendUsage() {
            _chatBox.AddMessage($"Invalid usage: {Trigger} <mic|speaker> <value>");
        }

        if (args.Length < 4) {
            SendUsage();
            return;
        }

        var type = args[2];
        var value = args[3];
        if (type is "mic" or "speaker") {
            var isInt = int.TryParse(value, out var intValue);

            if (type is "mic") {
                if (isInt) {
                    if (_microphoneNames.TryGetValue(intValue, out var micName)) {
                        SetMicrophoneEvent?.Invoke(micName);
                        
                        _chatBox.AddMessage($"Set microphone to \"{micName}\"");

                        return;
                    }
                }

                var micNames = _microphoneNames.Values;
                if (micNames.Contains(value)) {
                    SetMicrophoneEvent?.Invoke(value);

                    _chatBox.AddMessage($"Set microphone to \"{value}\"");
                    return;
                }
                
                _chatBox.AddMessage($"Could not find microphone with ID or name: \"{value}\"");
            } else if (type is "speaker") {
                if (isInt) {
                    if (_speakerNames.TryGetValue(intValue, out var speakerName)) {
                        SetSpeakerEvent?.Invoke(speakerName);
                        
                        _chatBox.AddMessage($"Set speaker to \"{speakerName}\"");

                        return;
                    }
                }

                var speakerNames = _speakerNames.Values;
                if (speakerNames.Contains(value)) {
                    SetSpeakerEvent?.Invoke(value);

                    _chatBox.AddMessage($"Set speaker to \"{value}\"");
                    return;
                }
                
                _chatBox.AddMessage($"Could not find speaker with ID or name: \"{value}\"");
            }
        }
    }
}