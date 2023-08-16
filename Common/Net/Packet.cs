using System;
using Hkmp.Networking.Packet;

namespace HkmpVoiceChat.Common.Net;

public class ServerVoicePacket : IPacketData {
    public const ushort MaxSize = ushort.MaxValue;

    public byte[] VoiceData { get; set; }
    
    /// <inheritdoc />
    public virtual void WriteData(IPacket packet) {
        if (VoiceData.Length > MaxSize) {
            throw new InvalidOperationException($"Voice data exceeds maximum size of {MaxSize} bytes");
        }

        var length = (ushort) VoiceData.Length;
        packet.Write(length);
        for (var i = 0; i < length; i++) {
            packet.Write(VoiceData[i]);
        }
    }

    /// <inheritdoc />
    public virtual void ReadData(IPacket packet) {
        var length = packet.ReadUShort();
        VoiceData = new byte[length];
        for (var i = 0; i < length; i++) {
            VoiceData[i] = packet.ReadByte();
        }
    }

    /// <inheritdoc />
    public bool IsReliable => false;

    /// <inheritdoc />
    public bool DropReliableDataIfNewerExists => false;
}

public class ClientVoicePacket : ServerVoicePacket {
    public ushort Id { get; set; }
    
    public bool Proximity { get; set; }

    /// <inheritdoc />
    public override void WriteData(IPacket packet) {
        packet.Write(Id);
        packet.Write(Proximity);
        
        base.WriteData(packet);
    }
    
    /// <inheritdoc />
    public override void ReadData(IPacket packet) {
        Id = packet.ReadUShort();
        Proximity = packet.ReadBool();
        
        base.ReadData(packet);
    }
}