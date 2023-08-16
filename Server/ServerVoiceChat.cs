using System.Collections.Generic;
using Hkmp.Api.Server;
using Hkmp.Logging;

namespace HkmpVoiceChat.Server; 

public class ServerVoiceChat {
    public static ILogger Logger { get; private set; }

    private readonly IServerApi _serverApi;
    private readonly ServerNetManager _netManager;
    private readonly ServerSettings _settings;

    private readonly HashSet<ushort> _broadcasters;

    public ServerVoiceChat(ServerAddon addon, IServerApi serverApi, ILogger logger) {
        Logger = logger;

        _serverApi = serverApi;
        _netManager = new ServerNetManager(addon, serverApi.NetServer);

        _settings = ServerSettings.LoadFromFile();

        _broadcasters = new HashSet<ushort>();
    }

    public void Initialize() {
        _serverApi.CommandManager.RegisterCommand(new VoiceChatCommand(_settings, _broadcasters));

        _netManager.VoiceEvent += OnVoice;
    }

    private void OnVoice(ushort id, byte[] data) {
        if (!_serverApi.ServerManager.TryGetPlayer(id, out var sender)) {
            Logger.Warn($"Could not find player '{id}' for received voice data");
            return;
        }

        var senderTeam = sender.Team;

        foreach (var receiver in _serverApi.ServerManager.Players) {
            if (sender == receiver) {
                continue;
            }

            if (_broadcasters.Contains(sender.Id)) {
                _netManager.SendVoiceData(receiver.Id, sender.Id, data, false);
                continue;
            }

            var sameTeam = senderTeam == receiver.Team;

            if (_settings.TeamVoicesOnly && !sameTeam) {
                continue;
            }

            if (_settings.TeamVoicesGlobally && sameTeam) {
                _netManager.SendVoiceData(receiver.Id, sender.Id, data, false);
                continue;
            }

            if (sender.CurrentScene != receiver.CurrentScene) {
                continue;
            }

            _netManager.SendVoiceData(receiver.Id, sender.Id, data, _settings.ProximityBasedVolume);
        }
    }
}