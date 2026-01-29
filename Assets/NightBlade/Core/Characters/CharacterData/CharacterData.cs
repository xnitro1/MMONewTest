namespace NightBlade
{
    public partial class CharacterData
    {
        /// <summary>
        /// Validates character data integrity
        /// </summary>
        public virtual bool ValidateData()
        {
            // Validate character name
            if (!DataValidation.IsValidCharacterName(CharacterName))
                return false;

            return true;
        }
    }
}







