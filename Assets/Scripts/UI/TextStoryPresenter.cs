/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/

using UnityEngine;

namespace Rory.WatsonStoryTeller.UI
{
    public abstract class TextStoryPresenter : MonoBehaviour
    {
        public abstract void PresentStory(string text, float speechTime);
        public abstract void PresentError(string text, float speechTime);
        public abstract void PresentVictory(string text, float speechTime);
        public abstract void PresentGameOver(string text, float speechTime);
        public abstract void SkipCurrentPresentation();
    }
}