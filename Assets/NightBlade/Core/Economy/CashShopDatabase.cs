using UnityEngine;
using UnityEngine.Serialization;

namespace NightBlade
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.CASH_SHOP_DATABASE_FILE, menuName = GameDataMenuConsts.CASH_SHOP_DATABASE_MENU, order = GameDataMenuConsts.CASH_SHOP_DATABASE_ORDER)]
    public class CashShopDatabase : ScriptableObject
    {
        [FormerlySerializedAs("cashStopItems")]
        public CashShopItem[] cashShopItems;
        public CashPackage[] cashPackages;
    }
}







