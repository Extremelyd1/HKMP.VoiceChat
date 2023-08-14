using Hkmp.Api.Server;
using ILogger = Hkmp.Logging.ILogger;

namespace HkmpVoiceChat.Server;

public class VoiceChatServerAddon : ServerAddon {
    public override void Initialize(IServerApi serverApi) {
        new ServerVoiceChat(this, serverApi, Logger).Initialize();
    }

    protected override string Name => Identifier.AddonName;
    protected override string Version => Identifier.AddonVersion;
    public override bool NeedsNetwork => true;
}