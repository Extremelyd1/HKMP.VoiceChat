using Hkmp.Api.Client;
using Hkmp.Api.Server;
using HkmpVoiceChat.Server;
using Modding;

namespace HkmpVoiceChat.Client; 

public class VoiceChatMod : Mod {
    /// <inheritdoc />
    public VoiceChatMod() : base("HKMP.VoiceChat") {
    }

    /// <inheritdoc />
    public override string GetVersion() {
        return Identifier.AddonVersion;
    }

    /// <inheritdoc />
    public override void Initialize() {
        ClientAddon.RegisterAddon(new VoiceChatClientAddon());
        ServerAddon.RegisterAddon(new VoiceChatServerAddon());
    }
}