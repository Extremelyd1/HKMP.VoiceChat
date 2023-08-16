using System;
using Hkmp.Api.Client;
using Hkmp.Api.Client.Networking;
using Hkmp.Networking.Packet;
using Hkmp.Networking.Packet.Data;
using HkmpVoiceChat.Common.Net;
using ServerPacketId = HkmpVoiceChat.Common.Net.ServerPacketId;

namespace HkmpVoiceChat.Client;

public class ClientNetManager {
    public event Action<ushort, byte[]> VoiceEvent;

    private readonly IClientAddonNetworkSender<ServerPacketId> _netSender;

    public ClientNetManager(ClientAddon addon, INetClient netClient) {
        _netSender = netClient.GetNetworkSender<ServerPacketId>(addon);

        var netReceiver = netClient.GetNetworkReceiver<ClientPacketId>(addon, InstantiatePacket);

        netReceiver.RegisterPacketHandler<ClientVoicePacket>(ClientPacketId.Voice,
            packet => { VoiceEvent?.Invoke(packet.Id, packet.VoiceData); });
    }

    public void SendVoiceData(byte[] data) {
        if (data.Length > ServerVoicePacket.MaxSize) {
            ClientVoiceChat.Logger.Error("Voice data exceeds max size!");
            return;
        }

        _netSender.SendCollectionData(ServerPacketId.Voice, new ServerVoicePacket {
            VoiceData = data
        });
    }

    private static IPacketData InstantiatePacket(ClientPacketId packetId) {
        switch (packetId) {
            case ClientPacketId.Voice:
                return new PacketDataCollection<ClientVoicePacket>();
        }

        return null;
    }
}