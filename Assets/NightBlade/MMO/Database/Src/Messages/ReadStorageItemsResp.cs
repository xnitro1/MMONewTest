using System.Collections.Generic;

namespace NightBlade.MMO
{
#nullable enable
    public partial struct GetStorageItemsResp
    {
        public UITextKeys Error { get; set; }
        public List<CharacterItem> StorageItems { get; set; }
    }
}







