using System.Collections;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using UnityEngine;

namespace Rory.WatsonStoryTeller.Core
{
    public abstract class IbmService : MonoBehaviour
    {
        [Header("IAM Authentication")]
        [Tooltip("The IAM apikey.")]
        [SerializeField] protected string iamApikey;

        
        public virtual void Start()
        {
            
            LogSystem.InstallDefaultReactors();
            Runnable.Run(CreateService());
        }

        protected abstract IEnumerator CreateService();

        protected void CheckIfApiKeyExists()
        {
            if (string.IsNullOrEmpty(iamApikey))
            {
                throw new IBMException("Plesae provide IAM ApiKey for the service.");
            }
        }

        protected IamAuthenticator CreateAuthenticatorCredential()
        {
            var authenticator = new IamAuthenticator(apikey: iamApikey);
            return authenticator;
        }

        protected static bool WaitForTokenData(IamAuthenticator authenticator)
        {
            return !authenticator.CanAuthenticate();
        }
    }
}