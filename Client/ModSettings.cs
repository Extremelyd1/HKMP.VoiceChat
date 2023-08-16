using Newtonsoft.Json;

namespace HkmpVoiceChat.Client; 

public class ModSettings {
    [JsonProperty("microphone_device_name")]
    public string MicrophoneDeviceName { get; set; }

    [JsonProperty("speaker_device_name")]
    public string SpeakerDeviceName { get; set; }
}