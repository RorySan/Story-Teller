/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/

using UnityEngine;

namespace Rory.WatsonStoryTeller.Audio
{
    public abstract class AudioStoryPresenter : MonoBehaviour
    {
        public abstract float PlayStoryText(string audioSsml);
        public abstract float PlayError(string audioSsml);
        public abstract float PlayVictory(string audioSsml);
        public abstract float PlayGameOver(string audioSsml);
    }
}