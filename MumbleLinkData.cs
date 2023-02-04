using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HkmpProximityChat {
    public class MumbleLinkData {
        private static bool IsUnix { get; } = Environment.OSVersion.Platform == PlatformID.Unix;

        public static int Size { get; } = IsUnix ? 10580 : 5460;

        private static uint UIVersion => 2;

        public uint UITick { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Identity { get; set; }

        public string Context { get; set; }

        public Vector3 AvatarPosition { get; set; } = Vector3.zero;

        public Vector3 AvatarFront { get; set; } = Vector3.zero;

        public Vector3 AvatarTop { get; set; } = Vector3.zero;

        public Vector3 CameraPosition { get; set; } = Vector3.zero;

        public Vector3 CameraFront { get; set; } = Vector3.zero;

        public Vector3 CameraTop { get; set; } = Vector3.zero;

        public void Write(Stream stream) {
            var encoding = Environment.OSVersion.Platform == PlatformID.Unix ? Encoding.UTF32 : Encoding.Unicode;
            using (var writer = new BinaryWriter(stream, encoding, true)) {
                writer.Write(UIVersion);
                writer.Write(UITick);
                WriteVector3(writer, AvatarPosition);
                WriteVector3(writer, AvatarFront);
                WriteVector3(writer, AvatarTop);
                WriteString(writer, 256, Name);
                WriteVector3(writer, CameraPosition);
                WriteVector3(writer, CameraFront);
                WriteVector3(writer, CameraTop);
                WriteString(writer, 256, Identity);
                WriteString(writer, 256, Context, Encoding.UTF8, true);
                WriteString(writer, 2048, Description);
            }
        }

        private static void WriteVector3(BinaryWriter writer, Vector3 vector) {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        private static void WriteString(
            BinaryWriter writer,
            int size,
            string value,
            Encoding encoding = null,
            bool lengthPrefixed = false
        ) {
            if (encoding == null)
                encoding = IsUnix ? Encoding.UTF32 : Encoding.Unicode;
            var numArray = new byte[encoding.GetByteCount(" ") * size];
            var bytes = encoding.GetBytes(value, 0, value.Length, numArray, 0);
            if (lengthPrefixed)
                writer.Write((uint) bytes);
            writer.Write(numArray);
        }
    }
}