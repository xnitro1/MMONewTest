using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NightBlade.MMO
{
    public class DisabledChatProfanityDetector : MonoBehaviour, IChatProfanityDetector
    {
        public async UniTask<ProfanityDetectResult> Proceed(string message)
        {
            await UniTask.Yield();
            return new ProfanityDetectResult()
            {
                message = message,
                shouldMutePlayer = false,
                shouldKickPlayer = false,
                muteMinutes = 0,
            };
        }
    }
}







