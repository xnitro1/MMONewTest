using UnityEngine;

namespace NightBlade
{
    public class UIGuildMessage2Updater : MonoBehaviour
    {
        public InputFieldWrapper inputField;

        private void OnEnable()
        {
            if (inputField == null || GameInstance.JoinedGuild == null) return;
            inputField.text = GameInstance.JoinedGuild.guildMessage2;
        }

        public void UpdateData()
        {
            if (inputField == null) return;
            GameInstance.ClientGuildHandlers.RequestChangeGuildMessage2(new RequestChangeGuildMessageMessage()
            {
                message = inputField.text,
            }, ClientGuildActions.ResponseChangeGuildMessage2);
        }
    }
}







