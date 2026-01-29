using LiteNetLib;
using UnityEngine;

namespace NightBlade
{
    public partial class DefaultClientChatHandlers : MonoBehaviour, IClientChatHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void SendChatMessage(ChatMessage message)
        {
            Manager.ClientSendPacket(0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, message);
        }
    }
}







