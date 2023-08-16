using System;
using Hkmp.Api.Server;
using Hkmp.Api.Server.Networking;
using Hkmp.Networking.Packet;
using Hkmp.Networking.Packet.Data;
using HkmpVoiceChat.Common.Net;
using ServerPacketId = HkmpVoiceChat.Common.Net.ServerPacketId;

namespace HkmpVoiceChat.Server;

public class ServerNetManager {
    public event Action<ushort, byte[]> VoiceEvent;

    private readonly IServerAddonNetworkSender<ClientPacketId> _netSender;

    public ServerNetManager(ServerAddon addon, INetServer netServer) {
        _netSender = netServer.GetNetworkSender<ClientPacketId>(addon);

        var netReceiver = netServer.GetNetworkReceiver<ServerPacketId>(addon, InstantiatePacket);
        
        netReceiver.RegisterPacketHandler<ServerVoicePacket>(ServerPacketId.Voice, (id, packet) => {
            VoiceEvent?.Invoke(id, packet.VoiceData);
        });
    }

    public void SendVoiceData(ushort receiver, ushort sender, byte[] data, bool proximity) {
        if (data.Length > ServerVoicePacket.MaxSize) {
            ServerVoiceChat.Logger.Info("Voice data exceeds max size!");
            return;
        }
        
        _netSender.SendCollectionData(ClientPacketId.Voice, new ClientVoicePacket {
            Id = sender,
            Proximity = proximity,
            VoiceData = data
        }, receiver);
    }

    private static IPacketData InstantiatePacket(ServerPacketId packetId) {
        switch (packetId) {
            case ServerPacketId.Voice:
                return new PacketDataCollection<ServerVoicePacket>();
        }

        return null;
    }
}