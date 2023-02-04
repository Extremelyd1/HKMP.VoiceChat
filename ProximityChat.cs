using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Hkmp.Api.Client;
using UnityEngine;
using ILogger = Hkmp.Logging.ILogger;

namespace HkmpProximityChat {
    public class ProximityChat : MonoBehaviour, IDisposable {
        private const float PositionMultiplier = 4f;
        private const float ScenePositionOffset = 10000f;

        private MemoryMappedFile _mappedFile;
        private MemoryMappedViewStream _stream;
        private FileSystemWatcher _watcher;
        private MumbleLinkData _mumbleData;

        private bool _enabled;

        public ILogger Logger { get; set; }

        public IClientApi ClientApi { get; set; }

        public void Start() {
            _enabled = true;
            _mumbleData = new MumbleLinkData {
                Name = "HKMP",
                Description = "Proximity chat for HKMP",
                Context = "HKMP"
            };

            if (Environment.OSVersion.Platform == PlatformID.Unix) {
                Logger.Info("Initialising on Unix");

                var fileName = $"/dev/shm/MumbleLink.{getuid()}";
                if (File.Exists(fileName)) {
                    OnCreated(null, null);
                } else {
                    Logger.Info("Link file does not exist");
                }

                var directoryName = Path.GetDirectoryName(fileName);
                if (directoryName == null) {
                    Logger.Error("Directory name for file: " + fileName + " is null");
                } else {
                    _watcher = new FileSystemWatcher(directoryName, Path.GetFileName(fileName));
                    _watcher.Created += OnCreated;
                    _watcher.Deleted += OnDeleted;
                    _watcher.EnableRaisingEvents = true;

                    Logger.Info("Created watcher");
                }

                void OnCreated(object sender, FileSystemEventArgs e) {
                    Logger.Info("Link established");
                    _mappedFile = MemoryMappedFile.CreateFromFile(fileName);
                    _stream = _mappedFile.CreateViewStream(0L, MumbleLinkData.Size);
                }

                void OnDeleted(object sender, FileSystemEventArgs e) {
                    Logger.Info("Link lost");
                    
                    _stream.Dispose();
                    _mappedFile.Dispose();

                    _stream = null;
                    _mappedFile = null;
                }
            } else {
                _mappedFile = MemoryMappedFile.CreateOrOpen("MumbleLink", MumbleLinkData.Size);
                _stream = _mappedFile.CreateViewStream(0L, MumbleLinkData.Size);
            }
        }

        public void Update() {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1)) {
                _enabled = !_enabled;
                var message = "Proximity voice chat is now " 
                              + (_enabled ? "enabled" : "disabled");
                Logger.Info(message);
                ClientApi.UiManager.ChatBox.AddMessage(message);
            }

            if (!_enabled || !ClientApi.NetClient.IsConnected) {
                return;
            }

            if (_stream == null) {
                return;
            }

            _mumbleData.Identity = ClientApi.ClientManager.Username;
            _mumbleData.UITick++;

            var position = GetPosition();
            var sceneIndex = GetSceneIndex();

            _mumbleData.AvatarPosition = new Vector3(
                position.x / PositionMultiplier + ScenePositionOffset * sceneIndex, 
                position.y / PositionMultiplier + ScenePositionOffset * sceneIndex, 0.0f
            );

            _mumbleData.AvatarFront = new Vector3(0.0f, 0.0f, 1f);
            _mumbleData.AvatarTop = new Vector3(0.0f, 1f, 0.0f);
            _mumbleData.CameraPosition = _mumbleData.AvatarPosition;
            _mumbleData.CameraFront = _mumbleData.AvatarFront;
            _mumbleData.CameraTop = _mumbleData.AvatarTop;

            _stream.Position = 0L;
            _mumbleData.Write(_stream);
        }

        private static Vector2 GetPosition() {
            var position = Vector2.zero;

            var instance = HeroController.instance;
            if (instance != null) {
                var heroPos = instance.transform.position;
                position.x = heroPos.x;
                position.y = heroPos.y;
            }

            return position;
        }

        private static int GetSceneIndex() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        public void Dispose() {
            _watcher?.Dispose();
            _stream?.Dispose();
            _mappedFile?.Dispose();
        }

        [DllImport("libc")]
        private static extern uint getuid();
    }
}