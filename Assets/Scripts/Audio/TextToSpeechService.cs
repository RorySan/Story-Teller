/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/

using UnityEngine;

namespace Rory.WatsonStoryTeller.Audio
{
    public abstract class TextToSpeechService : MonoBehaviour
    {
        public abstract AudioClip SynthesizeText(string text);
    }
}