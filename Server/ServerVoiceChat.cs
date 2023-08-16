using Hkmp.Api.Server;
using Hkmp.Logging;

namespace HkmpVoiceChat.Server; 

public class ServerVoiceChat {
    public static ILogger Logger { get; private set; }

    private readonly IServerApi _serverApi;
    private readonly ServerNetManager _netManager;

    public ServerVoiceChat(ServerAddon addon, IServerApi serverApi, ILogger logger) {
        Logger = logger;

        _serverApi = serverApi;
        _netManager = new ServerNetManager(addon, serverApi.NetServer);
    }

    public void Initialize() {
        _netManager.VoiceEvent += OnVoice;
    }

    private void OnVoice(ushort id, byte[] data) {
        if (!_serverApi.ServerManager.TryGetPlayer(id, out var player)) {
            Logger.Warn($"Could not find player '{id}' for received voice data");
            return;
        }

        foreach (var p in _serverApi.ServerManager.Players) {
            if (player == p || player.CurrentScene != p.CurrentScene) {
                continue;
            }

            _netManager.SendVoiceData(p.Id, player.Id, data);
        }
    }
}