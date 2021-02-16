/*
* 
* Copyright Rodrigo J. Sánchez. 2020, 2021. 
*
* This code has been modified from example code in 
* IBM Watson SDK examples and is still under Apache License, Version 2.0
* as noted below. 
*
*/

/**
* (C) Copyright IBM Corp. 2015, 2020.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
#pragma warning disable 0649

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using Rory.WatsonStoryTeller.Core;
using UnityEngine.Events;

namespace Rory.WatsonStoryTeller.Control
{
    public class IbmSpeechToText : IbmService
    {
        #region PLEASE SET THESE VARIABLES IN THE INSPECTOR

        [Space(10)]
        [Tooltip(
            "The service URL (optional). This defaults to \"https://api.us-south.speech-to-text.watson.cloud.ibm.com\"")]
        [SerializeField] private string _serviceUrl;
        
        [Header("Parameters")] [Tooltip("The Model to use. This defaults to en-US_BroadbandModel")] [SerializeField]
        private string _recognizeModel;

        #endregion

        // Support Fields
        private SpeechToTextService _service;
        private const string MIC_ERROR = "Reconocimiento de voz suspendido por inactividad. Click para reconectar";
        private AudioClip _recording = null;
        private int _recordingRoutine = 0;
        private string _microphoneID = null;
        private int _recordingBufferSize = 1;
        private int _recordingHZ = 22050;

        [Space(10)] [Header("Support")] 
        [SerializeField] private Text commandRecognizedText;
        [SerializeField] private Image micOkIndicator;
        [SerializeField] private Button micReconnectButton;
        
        
        
        // Events raised
        [Space(10)]
        [Header("EVENTS RAISED")]
        [SerializeField] private UnityEvent<string> onSpeechRecognized;

        protected override IEnumerator CreateService()
        {
            var apiKeyHolder = new IBMSpeechToTextKeyHolder();
            iamApikey = apiKeyHolder.APIKey;
            
            CheckIfApiKeyExists();
            var authenticator = CreateAuthenticatorCredential();

            while (WaitForTokenData(authenticator))
                yield return null;

            CreateSpeechToTextSession(authenticator);
            _service.OnError = ServiceError;

            StartRecording();
            ServiceIndicatorOk();
        }

        private void ServiceIndicatorOk()
        {
            micOkIndicator.color = Color.green;
            micReconnectButton.enabled = false;
        }

        private void ServiceError(string error)
        {
            commandRecognizedText.text = MIC_ERROR;
            micReconnectButton.enabled = true;
            micOkIndicator.color = Color.red;
        }

        public void ReconnectSpeechToTextService()
        {
            Runnable.Run(CreateService());
        }

        private void CreateSpeechToTextSession(IamAuthenticator authenticator)
        {
            _service = new SpeechToTextService(authenticator);
            if (!string.IsNullOrEmpty(_serviceUrl))
                _service.SetServiceUrl(_serviceUrl);

            _service.StreamMultipart = true;
            Active = true;
        }


        public bool Active
        {
            get { return _service.IsListening; }
            set
            {
                if (value && !_service.IsListening)
                {
                    _service.RecognizeModel =
                        (string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel);
                    _service.DetectSilence = true;
                    _service.EnableWordConfidence = true;
                    _service.EnableTimestamps = true;
                    _service.SilenceThreshold = 0.05f;
                    _service.MaxAlternatives = 1;
                    _service.EnableInterimResults = true;
                    _service.OnError = OnError;
                    _service.InactivityTimeout = -1;
                    _service.ProfanityFilter = false;
                    _service.SmartFormatting = true;
                    _service.SpeakerLabels = false;
                    _service.WordAlternativesThreshold = null;
                    _service.EndOfPhraseSilenceTime = null;
                    _service.StartListening(OnRecognize);
                }
                else if (!value && _service.IsListening)
                {
                    _service.StopListening();
                }
            }
        }

        private void StartRecording()
        {
            if (_recordingRoutine == 0)
            {
                UnityObjectUtil.StartDestroyQueue();
                _recordingRoutine = Runnable.Run(RecordingHandler());
            }
        }

        private void StopRecording()
        {
            if (_recordingRoutine != 0)
            {
                Microphone.End(_microphoneID);
                Runnable.Stop(_recordingRoutine);
                _recordingRoutine = 0;
            }
        }

        private void OnError(string error)
        {
            Active = false;

            Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
        }

        private IEnumerator RecordingHandler()
        {
            Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
            _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
            yield return null; // let _recordingRoutine get set..

            if (_recording == null)
            {
                StopRecording();
                yield break;
            }

            bool bFirstBlock = true;
            int midPoint = _recording.samples / 2;
            float[] samples = null;

            while (_recordingRoutine != 0 && _recording != null)
            {
                int writePos = Microphone.GetPosition(_microphoneID);
                if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
                {
                    Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                    StopRecording();
                    yield break;
                }

                if ((bFirstBlock && writePos >= midPoint)
                    || (!bFirstBlock && writePos < midPoint))
                {
                    // front block is recorded, make a RecordClip and pass it onto our callback.
                    samples = new float[midPoint];
                    _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                    AudioData record = new AudioData();
                    record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                    record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                    record.Clip.SetData(samples, 0);

                    _service.OnListen(record);

                    bFirstBlock = !bFirstBlock;
                }
                else
                {
                    // calculate the number of samples remaining until we ready for a block of audio, 
                    // and wait that amount of time it will take to record.
                    int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                    float timeRemaining = (float) remaining / (float) _recordingHZ;

                    yield return new WaitForSeconds(timeRemaining);
                }
            }
        }

        private void OnRecognize(SpeechRecognitionEvent result)
        {
            if (result == null || result.results.Length <= 0) return;
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    if (!res.final) continue;
                    commandRecognizedText.text = alt.transcript;
                    onSpeechRecognized.Invoke(alt.transcript);
                }
            }
        }
    }
}
