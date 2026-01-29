using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    public partial class DefaultServerCharacterMessageHandlers : MonoBehaviour, IServerCharacterMessageHandlers
    {
        public UniTaskVoid HandleRequestIncreaseAttributeAmount(RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request, RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }

            // Validate the request message
            if (!request.IsValidForPlayer(playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return default;
            }

            if (!playerCharacter.AddAttribute(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseAttributeAmountMessage()
                {
                    message = gameMessage,
                });
                return default;
            }
            playerCharacter.StatPoint -= 1;
            result.InvokeSuccess(new ResponseIncreaseAttributeAmountMessage());
            return default;
        }

        public UniTaskVoid HandleRequestIncreaseSkillLevel(RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }

            // Validate the request message
            if (!request.IsValidForPlayer(playerCharacter))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return default;
            }
            if (!playerCharacter.AddSkill(out UITextKeys gameMessage, request.dataId))
            {
                result.InvokeError(new ResponseIncreaseSkillLevelMessage()
                {
                    message = gameMessage,
                });
                return default;
            }
            int indexOfSkill = playerCharacter.IndexOfSkill(request.dataId);
            CharacterSkill characterSkill = playerCharacter.Skills[indexOfSkill];
            BaseSkill skill = characterSkill.GetSkill();
            int learnLevel = characterSkill.level - 1;
            float requireSkillPoint = skill.GetRequireCharacterSkillPoint(learnLevel);
            int requireGold = skill.GetRequireCharacterGold(learnLevel);
            Dictionary<Currency, int> requireCurrencies = skill.GetRequireCurrencyAmounts(learnLevel);
            Dictionary<BaseItem, int> requireItems = skill.GetRequireItemAmounts(learnLevel);
            playerCharacter.SkillPoint -= requireSkillPoint;
            playerCharacter.Gold -= requireGold;
            playerCharacter.DecreaseCurrencies(requireCurrencies);
            playerCharacter.DecreaseItems(requireItems);
            result.InvokeSuccess(new ResponseIncreaseSkillLevelMessage());
            return default;
        }

        public UniTaskVoid HandleRequestRespawn(RequestHandlerData requestHandler, RequestRespawnMessage request, RequestProceedResultDelegate<ResponseRespawnMessage> result)
        {
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (playerCharacter.CurrentHp > 0)
            {
                result.InvokeError(new ResponseRespawnMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_DEAD,
                });
                return default;
            }
            GameInstance.ServerCharacterHandlers.Respawn(request.option, playerCharacter);
            result.InvokeSuccess(new ResponseRespawnMessage());
            return default;
        }

        public async UniTaskVoid HandleRequestPlayerCharacterTransform(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponsePlayerCharacterTransformMessage> result)
        {
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeSuccess(new ResponsePlayerCharacterTransformMessage()
                {
                    position = playerCharacter.CurrentPosition,
                    rotation = playerCharacter.CurrentRotation,
                });
                return;
            }

            ResponsePlayerCharacterTransformMessage response = await BaseGameNetworkManager.Singleton.RequestPlayerCharacterTransform(requestHandler.ConnectionId);
            if (response.message != UITextKeys.NONE)
            {
                result.InvokeError(response);
                return;
            }
            result.InvokeSuccess(response);
        }
    }
}







