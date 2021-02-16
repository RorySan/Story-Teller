/*
 * Copyright Rodrigo J. Sánchez. 2020, 2021. 
*/

using System.Collections;
using System.Collections.Generic;
using IBM.Cloud.SDK;
using IBM.Watson.Assistant.V2.Model;
using Rory.WatsonStoryTeller.Audio;
using Rory.WatsonStoryTeller.UI;
using UnityEngine.Events;
using UnityEngine;

namespace Rory.WatsonStoryTeller.Core
{
    public class WatsonStoryTeller : MonoBehaviour
    {
        [Space(10)]
        [Header("Core Components")]
        [SerializeField] private WatsonAssistant watsonAssistant;
        [SerializeField] private AudioStoryPresenter storyPlayer;
        [SerializeField] private TextStoryPresenter storyWriter;
        [SerializeField] private AudioSource audioSource;
        
        [Space(10)]
        [Header("Misc Configuration")]
        [SerializeField] private KeyCode skipPresentationKey = KeyCode.Escape;

        [Space(10)]
        [Header("Sound Effects")]
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip cooldownSound;
        [SerializeField] private float soundEffectVolume = 0.2f;
        
        [Space(10)]
        [Header("Events Raised")]
        [SerializeField] public UnityEvent onStoryUpdate;
        [SerializeField] public UnityEvent onError;
        [SerializeField] public UnityEvent onDefeat;
        [SerializeField] public UnityEvent onVictory;
       
        
        // Support Fields
        private bool _isGameOver;
        private bool _watsonReturnedStoryError;
        private bool _watsonCommandBlocked;
        private string _watsonResponseText;
        private string _watsonResponseAudioSsml;
        private Dictionary<string, object> _watsonResponseVariables;

        private void Start()
        {
            watsonAssistant.onWatsonResponse.AddListener(ProcessWatsonResponse);
        }

        private void ProcessWatsonResponse(DetailedResponse<MessageResponse> watsonResponse)
        {
            GetWatsonResponseDetails(watsonResponse);
           
            ManageGameOver();
            if (_isGameOver) return;
            
            if (_watsonReturnedStoryError)
                ErrorActions();
            else
                ContinueStory();
        }

        private void ContinueStory()
        {
            audioSource.PlayOneShot(successSound, soundEffectVolume);
            var speechTime = storyPlayer.PlayStoryText(_watsonResponseAudioSsml);
            storyWriter.PresentStory(_watsonResponseText, speechTime);
            onStoryUpdate.Invoke();
        }

        private void ErrorActions()
        {
            audioSource.PlayOneShot(errorSound, soundEffectVolume);
            var speechTime = storyPlayer.PlayError(_watsonResponseAudioSsml);
            storyWriter.PresentError(_watsonResponseText, speechTime);
            StartCoroutine(WatsonCommandCooldown(speechTime));
            onError.Invoke();
        }

        private void GetWatsonResponseDetails(DetailedResponse<MessageResponse> watsonResponse)
        {
            _watsonResponseText = watsonResponse.Result.Output.Generic[0].Text;
            _watsonResponseVariables = watsonResponse.Result.Context?
                .Skills["main skill"].UserDefined;
            
            bool audioResponseAvailable = watsonResponse.Result.Output.Generic.Count > 1;
            if (audioResponseAvailable)
                _watsonResponseAudioSsml = watsonResponse.Result.Output.Generic[1].Text;

            // Our Watson Assistant responds with 4 outputs in case of story error
            // (errortext + erroraudio + last storytext + last storyaudio).
            // If story response is valid we get 2 outputs (storytext + storyaudio)
            // Therefore:
            _watsonReturnedStoryError = watsonResponse.Result.Output.Generic.Count == 4;
        }

        private void ManageGameOver()
        {
            if (_watsonResponseVariables == null) return;

            if (_watsonResponseVariables["quit"].Equals(true))
                Application.Quit();

            if (GameOverActionsAlreadyExecuted()) return;
            _isGameOver = _watsonResponseVariables["gameover"].Equals(true);

            if (_watsonResponseVariables["victory"].Equals(true))
                VictoryActions();
            else if (_isGameOver)
                DefeatActions();
        }
        
        private void VictoryActions()
        {
            var speechTime = storyPlayer.PlayVictory(_watsonResponseAudioSsml);
            storyWriter.PresentVictory(_watsonResponseText, speechTime);
            onVictory.Invoke();
        }

        private void DefeatActions()
        {
            var speechTime = storyPlayer.PlayGameOver(_watsonResponseAudioSsml);
            storyWriter.PresentGameOver(_watsonResponseText, speechTime);
            onDefeat.Invoke();
        }

        private bool GameOverActionsAlreadyExecuted()
        {
            return _isGameOver == _watsonResponseVariables["gameover"].Equals(true);
        }
        
        public void SendWatsonCommand(string command)
        {
            if (_watsonCommandBlocked)
            {
                audioSource.PlayOneShot(cooldownSound, soundEffectVolume);
                return;
            }
            watsonAssistant.SendWatsonCommand(command);
        }

        private IEnumerator WatsonCommandCooldown(float cooldown)
        {
            _watsonCommandBlocked = true;
            yield return new WaitForSeconds(cooldown);
            _watsonCommandBlocked = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(skipPresentationKey))
            {
                storyWriter.SkipCurrentPresentation();
            }
        }
    }
}