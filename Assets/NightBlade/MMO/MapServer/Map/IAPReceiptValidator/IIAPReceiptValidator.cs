using Cysharp.Threading.Tasks;

namespace NightBlade.MMO
{
    public interface IIAPReceiptValidator
    {
        UniTask<IAPReceiptValidateResult> ValidateIAPReceipt(CashPackage cashPackage, string userId, string characterId, string receipt);
    }
}







