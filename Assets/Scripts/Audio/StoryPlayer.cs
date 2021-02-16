/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/

using System.Collections;
using UnityEngine;

namespace Rory.WatsonStoryTeller.Audio
{
    public class StoryPlayer : AudioStoryPresenter
    {
        [Space(10)]
        [Header("CORE COMPONENTS")]
        [SerializeField] private TextToSpeechService voiceSynthesizer;
        [SerializeField] private AudioSource storySpeaker;
        [SerializeField] private AudioSource errorSpeaker;

        // Support Fields
        private Coroutine _playAndContinueCoroutine;

        public override float PlayStoryText(string audioSsml)
        {
            var speechTime = SpeakText(audioSsml, storySpeaker);
            return speechTime;
        }
        
        private float SpeakText(string audioSsml, AudioSource audioSource)
        {
            var clip = voiceSynthesizer.SynthesizeText(audioSsml);
            StopErrorCoroutine();
            audioSource.clip = clip;
            audioSource.Play();
            return clip.length;
        }

        public override float PlayError(string audioSsml)
        {
            if (NotInterruptingSpeech()) return SpeakText(audioSsml, errorSpeaker);
            
            var clip = voiceSynthesizer.SynthesizeText(audioSsml);
            _playAndContinueCoroutine = StartCoroutine(PlayErrorAndContinueStory(clip));
            var speechTime = clip.length;
            return speechTime;
        }

        private bool NotInterruptingSpeech()
        {
            return !storySpeaker.isPlaying;
        }

        private IEnumerator PlayErrorAndContinueStory(AudioClip clip)
        {
            storySpeaker.Pause();
            errorSpeaker.clip = clip;
            errorSpeaker.Play();

            yield return new WaitForSeconds(clip.length);

            storySpeaker.Play();
            _playAndContinueCoroutine = null;
        }
        
        public override float PlayVictory(string audioSsml)
        {
            // todo add victory sound effect?
            return PlayStoryText(audioSsml);
        }

        public override float PlayGameOver(string audioSsml)
        {
            // todo add defeat sound effect?
            return PlayStoryText(audioSsml);
        }

        private void StopErrorCoroutine()
        {
            if (_playAndContinueCoroutine == null) return;
            errorSpeaker.Stop();
            StopCoroutine(_playAndContinueCoroutine);
            _playAndContinueCoroutine = null;
        }
    }
}