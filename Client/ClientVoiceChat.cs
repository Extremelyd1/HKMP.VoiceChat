using Hkmp.Api.Client;
using Hkmp.Logging;
using Hkmp.Math;
using HkmpVoiceChat.Client.Voice;

namespace HkmpVoiceChat.Client;

public class ClientVoiceChat {
    public static ILogger Logger { get; private set; }

    private readonly IClientApi _clientApi;
    private readonly ClientNetManager _netManager;
    private readonly MicrophoneManager _micManager;
    private readonly SoundManager _soundManager;

    public ClientVoiceChat(ClientAddon addon, IClientApi clientApi, ILogger logger) {
        Logger = logger;

        _clientApi = clientApi;
        _netManager = new ClientNetManager(addon, clientApi.NetClient);
        _micManager = new MicrophoneManager();

        _soundManager = new SoundManager();
    }

    public void Initialize() {
        var voiceChatCommand = new ClientVoiceChatCommand(_clientApi.UiManager.ChatBox);
        _clientApi.CommandManager.RegisterCommand(voiceChatCommand);

        voiceChatCommand.SetMicrophoneEvent += micName => {
            VoiceChatMod.ModSettings.MicrophoneDeviceName = micName;
            _micManager.Microphone = new Microphone(micName);
        };
        voiceChatCommand.SetSpeakerEvent += speakerName => {
            VoiceChatMod.ModSettings.SpeakerDeviceName = speakerName;
            _soundManager.ChangeDevice(speakerName);
        };

        _soundManager.Open(VoiceChatMod.ModSettings.SpeakerDeviceName);
        _micManager.Microphone = new Microphone(VoiceChatMod.ModSettings.MicrophoneDeviceName);

        _clientApi.ClientManager.ConnectEvent += OnConnect;
        _clientApi.ClientManager.DisconnectEvent += OnDisconnect;

        _clientApi.ClientManager.PlayerEnterSceneEvent += OnPlayerEnterScene;
        _clientApi.ClientManager.PlayerLeaveSceneEvent += OnPlayerLeaveScene;

        _netManager.VoiceEvent += OnVoiceReceived;
    }

    private void OnConnect() {
        Logger.Debug("Client is connected, starting mic capture");

        _micManager.Start();
        _micManager.VoiceDataEvent += OnVoiceGenerated;
    }

    private void OnDisconnect() {
        Logger.Debug("Client is disconnected, stopping mic capture");

        _micManager.VoiceDataEvent -= OnVoiceGenerated;
        _micManager.Stop();
    }

    private void OnVoiceGenerated(byte[] data) {
        if (_clientApi.NetClient.IsConnected) {
            _netManager.SendVoiceData(data);
        }
    }

    private void OnPlayerEnterScene(IClientPlayer player) {
        Logger.Debug("Player entered scene, adding speaker");
        _soundManager.TryGetOrCreateSpeaker(player.Id, out _);
    }

    private void OnPlayerLeaveScene(IClientPlayer player) {
        Logger.Debug("Player left scene, closing and removing speaker");
        _soundManager.TryRemoveSpeaker(player.Id);
    }

    private void OnVoiceReceived(ushort id, byte[] data, bool proximity) {
        if (!_soundManager.TryGetOrCreateSpeaker(id, out var speaker)) {
            Logger.Warn($"Could not get or create speaker for player '{id}', cannot play voice");
            return;
        }

        // Debug volume
        const float volume = 2f;

        if (!proximity) {
            speaker.Play(data, volume);
            return;
        }

        var hc = HeroController.instance;
        if (hc == null || hc.gameObject == null) {
            Logger.Warn("Local player could not be found, cannot play voice positionally");
            speaker.Play(data, volume);
            return;
        }

        if (!_clientApi.ClientManager.TryGetPlayer(id, out var player)) {
            Logger.Warn($"No player found for '{id}', cannot play voice positionally");
            speaker.Play(data, volume);
            return;
        }

        var localPlayer = hc.gameObject;
        var localPos = localPlayer.transform.position;

        var remotePos = player.PlayerObject.transform.position;

        var pos = remotePos - localPos;

        speaker.Play(data, volume, new Vector3(pos.x, pos.y, pos.z));
    }
}