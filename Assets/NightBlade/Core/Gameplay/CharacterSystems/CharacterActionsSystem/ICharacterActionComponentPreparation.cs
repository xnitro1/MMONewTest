namespace NightBlade
{
    public interface ICharacterActionComponentPreparation
    {
        void OnPrepareActionDurations(float[] triggerDurations, float totalDuration, float remainsDurationWithoutSpeedRate, float endTime);
    }
}







