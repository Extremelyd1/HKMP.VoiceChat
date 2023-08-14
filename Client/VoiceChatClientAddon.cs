using Hkmp.Api.Client;

namespace HkmpVoiceChat.Client;

public class VoiceChatClientAddon : ClientAddon {
    public override void Initialize(IClientApi clientApi) {
        new ClientVoiceChat(this, clientApi, Logger).Initialize();
    }

    protected override string Name => Identifier.AddonName;
    protected override string Version => Identifier.AddonVersion;
    public override bool NeedsNetwork => true;
}