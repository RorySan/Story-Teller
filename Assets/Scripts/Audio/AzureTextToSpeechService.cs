using UnityEngine;
using Microsoft.CognitiveServices.Speech;

namespace Rory.WatsonStoryTeller.Audio
{
    public class AzureTextToSpeechService : TextToSpeechService
    {
        private SpeechSynthesizer _synthesizer;

        private void Start()
        {
            var apiKeyHolder = new AzureTextToSpeechKeyHolder();
            // Creates an instance of a speech config with specified subscription key and service region.
            var config = SpeechConfig.FromSubscription(apiKeyHolder.APIKey, "westeurope");
            config.SpeechSynthesisVoiceName = "es-ES-AlvaroNeural";
            
            _synthesizer = new SpeechSynthesizer(config, null);
        }

        public override AudioClip SynthesizeText(string text)
        {
            // Starts speech synthesis, and returns after a single utterance is synthesized.
            
            // todo speak ssml or plain text depending on input.
            var result = _synthesizer.SpeakSsmlAsync(text).Result;
        
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Since native playback is not yet supported on Unity yet (currently only supported on Windows/Linux Desktop),
                // use the Unity API to play audio here as a short term solution.
                // Native playback support will be added in the future release.
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; ++i)
                {
                    audioData[i] = (short) (result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                }

                // The default output audio format is 16K 16bit mono
                var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
                audioClip.SetData(audioData, 0);
                return audioClip;
            }

            if (result.Reason != ResultReason.Canceled) return null;
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            Debug.Log(
                $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?");
            return null;
        }
    }
}