


using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rory.WatsonStoryTeller.Control
{
    public class InputFieldToText : MonoBehaviour
    {
        [Space(10)]
        [Header("CORE COMPONENTS")]
        [SerializeField] private InputField commandInput;
        
        // Events raised
        [Space(10)]
        [Header("EVENTS RAISED")]
        [SerializeField] public UnityEvent<string> onCommandInput;
     

        private void Start()
        {
            commandInput.ActivateInputField();
        }

        private void Update()
        {
            GetInputboxText();
        }

        private void GetInputboxText()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                onCommandInput.Invoke(commandInput.text);
                commandInput.text = "";
                commandInput.ActivateInputField();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                commandInput.ActivateInputField();
            }
        }
    }
}