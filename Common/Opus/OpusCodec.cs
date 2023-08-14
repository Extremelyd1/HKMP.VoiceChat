using System;
using System.Collections.Generic;

namespace HkmpVoiceChat.Common.Opus;

public class OpusCodec {
    private readonly OpusDecoder _decoder;
    private readonly OpusEncoder _encoder;
    private readonly int _sampleRate;
    private readonly ushort _frameSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpusCodec"/> class.
    /// </summary>
    /// <param name="sampleRate">The sample rate in Hertz (samples per second).</param>
    /// <param name="channels">The sample channels (1 for mono, 2 for stereo).</param>
    /// <param name="frameSize">Size of the frame in samples.</param>
    public OpusCodec(
        int sampleRate = Constants.DEFAULT_AUDIO_SAMPLE_RATE,
        byte channels = Constants.DEFAULT_AUDIO_SAMPLE_CHANNELS,
        ushort frameSize = Constants.DEFAULT_AUDIO_FRAME_SIZE
    ) {
        _sampleRate = sampleRate;
        _frameSize = frameSize;
        _decoder = new OpusDecoder(sampleRate, channels) { EnableForwardErrorCorrection = true };
        _encoder = new OpusEncoder(sampleRate, channels) { EnableForwardErrorCorrection = true };
    }

    public byte[] Decode(byte[] encodedData) {
        if (encodedData == null) {
            _decoder.Decode(null, 0, 0, new byte[_sampleRate / _frameSize], 0);
            return null;
        }

        var samples = OpusDecoder.GetSamples(encodedData, 0, encodedData.Length, _sampleRate);
        if (samples < 1)
            return null;

        var dst = new byte[samples * sizeof(ushort)];
        var length = _decoder.Decode(encodedData, 0, encodedData.Length, dst, 0);
        if (dst.Length != length)
            Array.Resize(ref dst, length);
        return dst;
    }

    public IEnumerable<int> PermittedEncodingFrameSizes => _encoder.PermittedFrameSizes;

    public byte[] Encode(byte[] data) {
        var samples = data.Length / sizeof(ushort);
        var numberOfBytes = _encoder.FrameSizeInBytes(samples);

        var dst = new byte[numberOfBytes];
        var encodedBytes = _encoder.Encode(data, 0, dst, 0, samples);

        Array.Resize(ref dst, encodedBytes);

        return dst;
    }
}