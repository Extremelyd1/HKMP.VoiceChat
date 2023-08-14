using System.Collections.Concurrent;
using Hkmp.Api.Client;
using Hkmp.Logging;
using Hkmp.Math;
using HkmpVoiceChat.Client.Voice;
using HkmpVoiceChat.Common;
using HkmpVoiceChat.Common.Opus;

namespace HkmpVoiceChat.Client; 

public class ClientVoiceChat {
    public static ILogger Logger { get; private set; }
    
    private readonly IClientApi _clientApi;
    private readonly ClientNetManager _netManager;
    private readonly MicThread _micThread;
    private readonly SoundManager _soundManager;

    private readonly OpusCodec _decoder;

    private readonly ConcurrentDictionary<ushort, Speaker> _speakers;

    public ClientVoiceChat(ClientAddon addon, IClientApi clientApi, ILogger logger) {
        Logger = logger;

        _clientApi = clientApi;
        _netManager = new ClientNetManager(addon, clientApi.NetClient);
        _micThread = new MicThread();

        _soundManager = new SoundManager(null);

        _decoder = new OpusCodec();

        _speakers = new ConcurrentDictionary<ushort, Speaker>();

        Logger.Debug("Speakers:");
        foreach (var speaker in SoundManager.GetAllSpeakers()) {
            Logger.Debug($"  {speaker}");
        }
        Logger.Debug($"Default speaker: {SoundManager.GetDefaultSpeaker()}");
        
        Logger.Debug("Microphones:");
        foreach (var mic in Microphone.GetAllMicrophones()) {
            Logger.Debug($"  {mic}");
        }
        Logger.Debug($"Default mic: {Microphone.GetDefaultMicrophone()}");
    }

    public void Initialize() {
        _soundManager.Initialize();
        
        _clientApi.ClientManager.ConnectEvent += OnConnect;
        _clientApi.ClientManager.DisconnectEvent += OnDisconnect;
        
        _clientApi.ClientManager.PlayerEnterSceneEvent += OnPlayerEnterScene;
        _clientApi.ClientManager.PlayerLeaveSceneEvent += OnPlayerLeaveScene;
        
        _netManager.VoiceEvent += OnVoiceReceived;
    }

    private void OnConnect() {
        Logger.Debug("OnConnect was called");
        
        _micThread.Start();
        _micThread.VoiceDataEvent += OnVoiceGenerated;
    }
    
    private void OnDisconnect() {
        Logger.Debug("OnDisconnect was called");
        
        _micThread.VoiceDataEvent -= OnVoiceGenerated;
        _micThread.Stop();
    }

    private void OnVoiceGenerated(byte[] data) {
        if (_clientApi.NetClient.IsConnected) {
            Logger.Debug("Voice generated, sending data");
            _netManager.SendVoiceData(data);
        }
    }
    
    private void OnPlayerEnterScene(IClientPlayer player) {
        Logger.Debug("Player entered scene, adding speaker");
        if (_speakers.TryGetValue(player.Id, out var speaker)) {
            Logger.Debug("Player had speaker, closing first");
            speaker.Close();
        }

        speaker = new Speaker();
        speaker.Open();

        _speakers.TryAdd(player.Id, speaker);
    }
    
    private void OnPlayerLeaveScene(IClientPlayer player) {
        Logger.Debug("Player left scene, closing and removing speaker");
        if (!_speakers.TryRemove(player.Id, out var speaker)) {
            return;
        }
        
        speaker.Close();
    }

    private void OnVoiceReceived(ushort id, byte[] data) {
        Logger.Debug($"Voice received from server: {id}");
        if (!_speakers.TryGetValue(id, out var speaker)) {
            Logger.Warn($"No speaker found for player '{id}', cannot play voice");
            return;
        }
        
        var decodedBytes = _decoder.Decode(data);
        var decodedShorts = Utils.BytesToShorts(decodedBytes);

        if (!_clientApi.ClientManager.TryGetPlayer(id, out var player)) {
            Logger.Warn($"No player found for '{id}', cannot play voice positionally");
            speaker.Play(decodedShorts);
            return;
        }

        var localPlayer = HeroController.instance.gameObject;
        var localPos = localPlayer.transform.position;

        var remotePos = player.PlayerObject.transform.position;

        var pos = localPos - remotePos;
        
        speaker.Play(decodedShorts, 1f, new Vector3(pos.x, pos.y, pos.z), 60f);
    }
}