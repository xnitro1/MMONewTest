using UnityEngine;

namespace NightBlade
{
    public class ExitVehicleActivator : MonoBehaviour
    {
        public GameObject[] activateObjects = new GameObject[0];

        private void LateUpdate()
        {
            bool canExitVehicle = GameInstance.PlayingCharacterEntity != null && GameInstance.PlayingCharacterEntity.PassengingVehicleEntity != null;
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canExitVehicle);
            }
        }
    }
}







