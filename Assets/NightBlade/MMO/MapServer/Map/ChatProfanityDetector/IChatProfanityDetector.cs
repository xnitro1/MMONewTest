using Cysharp.Threading.Tasks;

namespace NightBlade.MMO
{
    public interface IChatProfanityDetector
    {
        UniTask<ProfanityDetectResult> Proceed(string message);
    }
}







