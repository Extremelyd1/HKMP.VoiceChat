using Hkmp.Math;
using OpenTK.Audio.OpenAL;

namespace HkmpVoiceChat.Client.Voice;

public class Speaker {
    public const float DefaultDistance = 60f;
    private const int NumBuffers = 32;

    private int _source;
    private int[] _buffers;

    private int _bufferIndex;

    public void Open() {
        if (HasValidSource()) {
            return;
        }

        _source = AL.GenSource();
        SoundManager.CheckAlError(0);
        AL.Source(_source, ALSourceb.Looping, false);
        SoundManager.CheckAlError(1);

        AL.DistanceModel(ALDistanceModel.LinearDistance);
        SoundManager.CheckAlError(2);
        AL.Source(_source, ALSourcef.MaxDistance, DefaultDistance);
        SoundManager.CheckAlError(3);
        AL.Source(_source, ALSourcef.ReferenceDistance, 0f);
        SoundManager.CheckAlError(4);

        _buffers = AL.GenBuffers(NumBuffers);
        SoundManager.CheckAlError(5);
    }

    public void Play(short[] data, float volume = 1f, Vector3 position = null, float maxDistance = DefaultDistance) {
        RemoveProcessedBuffers();

        Write(data, volume, position, maxDistance);

        var buffers = GetQueuedBuffers();
        var stopped = GetState() == ALSourceState.Initial || GetState() == ALSourceState.Stopped || buffers <= 1;

        if (stopped) {
            AL.SourcePlay(_source);
            SoundManager.CheckAlError(0);
        }
    }

    private void Write(short[] data, float volume, Vector3 position, float maxDistance) {
        SetPosition(position, maxDistance);

        AL.Source(_source, ALSourcef.MaxGain, 6f);
        SoundManager.CheckAlError(0);
        AL.Source(_source, ALSourcef.Gain, volume);
        SoundManager.CheckAlError(1);
        AL.Listener(ALListenerf.Gain, 1f);
        SoundManager.CheckAlError(2);

        var queuedBuffers = GetQueuedBuffers();
        if (queuedBuffers >= _buffers.Length) {
            AL.GetSource(_source, ALGetSourcei.SampleOffset, out var sampleOffset);
            SoundManager.CheckAlError(3);
            AL.Source(_source, ALSourcei.SampleOffset, sampleOffset + SoundManager.BufferSize);
            SoundManager.CheckAlError(4);

            RemoveProcessedBuffers();
        }

        AL.BufferData(_buffers[_bufferIndex], ALFormat.Mono16, data, data.Length * sizeof(short),
            SoundManager.SampleRate);
        SoundManager.CheckAlError(5);
        AL.SourceQueueBuffer(_source, _buffers[_bufferIndex]);
        SoundManager.CheckAlError(6);

        _bufferIndex = (_bufferIndex + 1) % _buffers.Length;
    }

    private void LinearAttenuation(float maxDistance) {
        AL.DistanceModel(ALDistanceModel.LinearDistance);
        SoundManager.CheckAlError(0);
        AL.Source(_source, ALSourcef.MaxDistance, maxDistance);
        SoundManager.CheckAlError(1);
    }

    private void SetPosition(Vector3 soundPos, float maxDistance) {
        AL.Listener(ALListener3f.Position, 0f, 0f, 0f);
        SoundManager.CheckAlError(0);

        var orientation = new[] { 0f, 0f, -1f, 0f, 1f, 0f };
        AL.Listener(ALListenerfv.Orientation, ref orientation);
        SoundManager.CheckAlError(1);

        if (soundPos != null) {
            LinearAttenuation(maxDistance);
            AL.Source(_source, ALSourceb.SourceRelative, false);
            SoundManager.CheckAlError(2);
            AL.Source(_source, ALSource3f.Position, soundPos.X, soundPos.Y, soundPos.Z);
            SoundManager.CheckAlError(3);
        } else {
            LinearAttenuation(DefaultDistance);
            AL.Source(_source, ALSourceb.SourceRelative, true);
            SoundManager.CheckAlError(4);
            AL.Source(_source, ALSource3f.Position, 0f, 0f, 0f);
            SoundManager.CheckAlError(5);
        }
    }

    public void Close() {
        if (HasValidSource()) {
            if (GetState() == ALSourceState.Playing) {
                AL.SourceStop(_source);
                SoundManager.CheckAlError(0);
            }

            AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out var processed);
            SoundManager.CheckAlError(1);

            if (processed > 0) {
                AL.SourceUnqueueBuffers(_source, processed);
                SoundManager.CheckAlError(2);
            }

            AL.DeleteSource(_source);
            SoundManager.CheckAlError(3);

            AL.DeleteBuffers(_buffers);
            SoundManager.CheckAlError(4);
        }

        _source = 0;
    }

    private void RemoveProcessedBuffers() {
        AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out var processed);
        SoundManager.CheckAlError(0);

        if (processed > 0) {
            AL.SourceUnqueueBuffers(_source, processed);
            SoundManager.CheckAlError(1);
        }
    }

    private ALSourceState GetState() {
        AL.GetSource(_source, ALGetSourcei.SourceState, out var state);
        SoundManager.CheckAlError(0);

        return (ALSourceState) state;
    }

    private int GetQueuedBuffers() {
        AL.GetSource(_source, ALGetSourcei.BuffersQueued, out var buffers);
        SoundManager.CheckAlError(0);

        return buffers;
    }

    private bool HasValidSource() {
        var validSource = AL.IsSource(_source);

        return validSource;
    }
}