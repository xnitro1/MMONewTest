using System.Collections.Generic;

namespace NightBlade.MMO
{
#nullable enable
    public partial struct UpdateStorageItemsReq
    {
        public StorageType StorageType { get; set; }
        public string StorageOwnerId { get; set; }
        public List<CharacterItem> StorageItems { get; set; }
        public bool DeleteStorageReservation { get; set; }
    }
}







