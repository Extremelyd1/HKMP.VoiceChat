using System;

namespace HkmpVoiceChat.Common;

public static class Utils {
    private const float FloatShortScale = short.MaxValue;
    private const float FloatClip = FloatShortScale - 1;
    private const float FloatShortScalingFactor = 1f / FloatShortScale;
    
    public static short[] FloatsToShortsNormalized(float[] audioData) {
        var shortAudioData = new short[audioData.Length];
        for (var i = 0; i < audioData.Length; i++) {
            shortAudioData[i] = (short) Math.Max(Math.Min(audioData[i] * FloatShortScale, FloatClip), -FloatShortScale);
        }

        return shortAudioData;
    }

    public static byte[] ShortsToBytes(short[] shorts) {
        var bytes = new byte[shorts.Length * 2];
        for (var i = 0; i < shorts.Length; i++) {
            var s = shorts[i];
            bytes[i * 2] = (byte) (s & 0xFF);
            bytes[i * 2 + 1] = (byte) ((s >> 8) & 0xFF);
        }

        return bytes;
    }

    public static short[] BytesToShorts(byte[] bytes) {
        if (bytes.Length % 2 != 0) {
            throw new ArgumentException("Byte array length must be even", nameof(bytes));
        }

        var shorts = new short[bytes.Length / 2];
        for (var i = 0; i < bytes.Length; i += 2) {
            var b1 = bytes[i];
            var b2 = bytes[i + 1];
            shorts[i / 2] = (short) (((b2 & 0xFF) << 8) | (b1 & 0xFF));
        }

        return shorts;
    }
}