using UnityEngine;

namespace NightBlade
{
    public interface ICharacterModelFactory
    {
        string Name { get; }
        bool ValidateSourceObject(GameObject obj);
        BaseCharacterModel Setup(GameObject obj);
    }
}







