using LiteNetLibManager;

namespace NightBlade
{
    public partial interface IClientCharacterHandlers
    {
        bool RequestIncreaseAttributeAmount(RequestIncreaseAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseAttributeAmountMessage> callback);
        bool RequestIncreaseSkillLevel(RequestIncreaseSkillLevelMessage data, ResponseDelegate<ResponseIncreaseSkillLevelMessage> callback);
        bool RequestRespawn(RequestRespawnMessage data, ResponseDelegate<ResponseRespawnMessage> callback);
        bool RequestPlayerCharacterTransform(ResponseDelegate<ResponsePlayerCharacterTransformMessage> callback);
        void SubscribePlayerCharacter(IPlayerCharacterData playerCharacter);
        void UnsubscribePlayerCharacter(IPlayerCharacterData playerCharacter);
        bool TryGetSubscribedPlayerCharacterById(string characterId, out IPlayerCharacterData playerCharacter);
        bool TryGetSubscribedPlayerCharacterByName(string characterId, out IPlayerCharacterData playerCharacter);
        void ClearSubscribedPlayerCharacters();
    }
}







