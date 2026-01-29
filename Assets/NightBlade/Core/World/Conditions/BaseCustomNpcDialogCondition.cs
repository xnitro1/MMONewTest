using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NightBlade
{
    public abstract class BaseCustomNpcDialogCondition : ScriptableObject
    {
        public abstract UniTask<bool> IsPass(IPlayerCharacterData player);
    }
}







