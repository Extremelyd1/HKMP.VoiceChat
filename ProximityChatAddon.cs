using Hkmp.Api.Client;
using JetBrains.Annotations;
using UnityEngine;

namespace HkmpProximityChat {
    [PublicAPI]
    public class ProximityChatAddon : ClientAddon {
        public override void Initialize(IClientApi clientApi) {
            var target = new GameObject();
            Object.DontDestroyOnLoad(target);

            var proximityChat = target.AddComponent<ProximityChat>();
            proximityChat.Logger = Logger;
            proximityChat.ClientApi = ClientApi;
        }

        protected override string Name => "ProximityChat";
        protected override string Version => "0.0.1";
        public override bool NeedsNetwork => false;
    }
}