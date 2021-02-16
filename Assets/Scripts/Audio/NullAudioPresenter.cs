/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/


namespace Rory.WatsonStoryTeller.Audio
{
    public class NullAudioPresenter : AudioStoryPresenter
    {
        public override float PlayStoryText(string audioSsml)
        {
            return 4;
        }

        public override float PlayError(string audioSsml)
        {
            return 4;
        }

        public override float PlayVictory(string audioSsml)
        {
            return 4;
        }

        public override float PlayGameOver(string audioSsml)
        {
            return 4;
        }
    }
}
