using Newtonsoft.Json;

namespace HkmpVoiceChat.Client; 

public class ModSettings {
    [JsonProperty("microphone_device_name")]
    public string MicrophoneDeviceName { get; set; }

    [JsonProperty("speaker_device_name")]
    public string SpeakerDeviceName { get; set; }

    [JsonProperty("microphone_amplification")]
    public float MicrophoneAmplification { get; set; } = 1f;

    [JsonProperty("voice_chat_volume")]
    public float VoiceChatVolume { get; set; } = 1f;
}