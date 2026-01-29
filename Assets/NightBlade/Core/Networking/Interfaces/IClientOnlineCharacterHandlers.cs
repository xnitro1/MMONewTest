using LiteNetLibManager;

namespace NightBlade
{
    public partial interface IClientOnlineCharacterHandlers
    {
        bool IsCharacterOnline(string characterId);
        int GetCharacterOfflineOffsets(string characterId);
        void RequestOnlineCharacter(string characterId);
        void HandleNotifyOnlineCharacter(MessageHandlerData messageHandler);
        void ClearOnlineCharacters();
    }
}







