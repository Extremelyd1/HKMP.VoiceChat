using Hkmp.Api.Client;
using Hkmp.Api.Server;
using HkmpVoiceChat.Server;
using Modding;

namespace HkmpVoiceChat.Client; 

/// <summary>
/// The voice chat mod class.
/// </summary>
public class VoiceChatMod : Mod, IGlobalSettings<ModSettings> {
    /// <summary>
    /// Statically accessible mod settings.
    /// </summary>
    public static ModSettings ModSettings = new();

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

    /// <inheritdoc />
    public void OnLoadGlobal(ModSettings modSettings) {
        ModSettings = modSettings ?? new ModSettings();
    }

    /// <inheritdoc />
    public ModSettings OnSaveGlobal() {
        return ModSettings;
    }
}