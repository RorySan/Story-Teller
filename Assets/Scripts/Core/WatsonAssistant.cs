/*
* 
* Copyright Rodrigo J. Sánchez. 2020, 2021. 
*
* This code has been modified from example code in 
* IBM Watson SDK examples and is still under Apache License, Version 2.0
* as noted below. 
*
*/

/*
* 
* (C) Copyright IBM Corp. 2018, 2020. 
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

using System.Collections;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;
using UnityEngine;
using UnityEngine.Events;
using MessageInput = IBM.Watson.Assistant.V2.Model.MessageInput;
using MessageResponse = IBM.Watson.Assistant.V2.Model.MessageResponse;


namespace Rory.WatsonStoryTeller.Core
{
    public class WatsonAssistant : IbmService
    {
        #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
        [Space(10)]
        [Tooltip("The service URL (optional). This defaults to \"https://api.us-south.assistant.watson.cloud.ibm.com\"")]
        [SerializeField]
        private string serviceUrl;
        [Tooltip("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
        [SerializeField]
        private string versionDate;
        [Tooltip("The assistantId to run the example.")]
        [SerializeField]
        private string assistantId;
        #endregion

        // API Support Fields
        private AssistantService _watsonAssistant;
        private bool _sessionCreated;
        private string _sessionId;
        
        // Events Raised
        [Space(10)]
        [Header("Events Raised")]
        [SerializeField] public UnityEvent<DetailedResponse<MessageResponse>> onWatsonResponse;

        
        protected override IEnumerator CreateService()
        {
            var apiKeyHolder = new WatsonKeyHolder();
            iamApikey = apiKeyHolder.APIKey;
            
            CheckIfApiKeyExists();
            var authenticator = CreateAuthenticatorCredential();

            while (WaitForTokenData(authenticator))
                yield return null;

            CreateWatsonSession(authenticator);

            while (!_sessionCreated)
                yield return null;
            
            GetFirstWatsonNode();
        }

        private void CreateWatsonSession(IamAuthenticator authenticator)
        {
            _watsonAssistant = new AssistantService(versionDate, authenticator);
            if (!string.IsNullOrEmpty(serviceUrl))
                _watsonAssistant.SetServiceUrl(serviceUrl);

            _watsonAssistant.CreateSession(OnCreateSession, assistantId);
        }
        
        private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
        {
            Log.Debug("ExampleAssistantV2.OnCreateSession()", "Session: {0}", response.Result.SessionId);
            _sessionId = response.Result.SessionId;
            _sessionCreated = true;
        }

        private void GetFirstWatsonNode()
        {
            _watsonAssistant.Message(OnMessageSent, assistantId, _sessionId);
        }
        
        public void SendWatsonCommand(string command)
        {
            var watsonCommand = new MessageInput()
            {
                Text = command,
                Options = new MessageInputOptions()
                {
                    ReturnContext = true
                }
            };
            
            _watsonAssistant.Message(OnMessageSent, assistantId, _sessionId, input: watsonCommand);
        }

        private void OnMessageSent(DetailedResponse<MessageResponse> response, IBMError error)
        {
            onWatsonResponse.Invoke(response);
        }
    }
}
