using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Rory.WatsonStoryTeller.UI
{
     public class ScreenStoryWriter : TextStoryPresenter
     {
          [Space(10)]
          [Header("Core Components")]
          [SerializeField] private Text storyText;
          [SerializeField] private PopupText errorPopup;
          [SerializeField] private PopupText defeatPopup;
          [SerializeField] private PopupText victoryPopup;


          private const string VICTORY_TEXT =
               "Di JUGAR DE NUEVO para explorar una historia alternativa o TERMINAR para salir";

          private const string DEFEAT_TEXT =
               "Di JUGAR DE NUEVO para intentarlo otra vez o TERMINAR para rendirte";
          
          // Support Fields
          private Coroutine _errorCoroutine;
          private Coroutine _typingCoroutine;
          private Coroutine _gameOverCoroutine;
          private bool _pauseTyping;
          private bool _skipPresentation;

          public override void PresentStory(string text, float speechTime)
          {
               ResetView();
               _typingCoroutine = StartCoroutine(TypeText(text, speechTime));
          }

          private IEnumerator TypeText(string text, float speechTime)
          {
               _pauseTyping = false;
               foreach (var c in text)
               {
                    while (_pauseTyping)
                         yield return null;

                    storyText.text += c;
                    if (_skipPresentation)
                         PrintImmediately(text);

                    yield return new WaitForSeconds(speechTime/text.Length);
               }
          }

          private void PrintImmediately(string text)
          {
               StopTypingCoroutine();
               storyText.text = text;
               _skipPresentation = false;
          }

          public override void PresentVictory(string text, float speechTime)
          {
               PresentStory(text, speechTime);
               _gameOverCoroutine = StartCoroutine(
                    DisplayPopupAfterSeconds(victoryPopup, VICTORY_TEXT, speechTime));
          }

          public override void PresentGameOver(string text, float speechTime)
          {
               PresentStory(text, speechTime);
               _gameOverCoroutine = StartCoroutine(
                    DisplayPopupAfterSeconds(defeatPopup, DEFEAT_TEXT, speechTime));
          }

          public override void PresentError(string text, float speechTime)
          {
               StopErrorCoroutine();
               _errorCoroutine = StartCoroutine(
                    DisplayErrorForSeconds(text, speechTime));
          }
          
          private IEnumerator DisplayErrorForSeconds(string text, float speechTime)
          {
               _pauseTyping = true;
               errorPopup.DisplayPopupText(text);

               yield return new WaitForSeconds(speechTime);

               _pauseTyping = false;
               errorPopup.HidePopupText();
          }

          private IEnumerator DisplayPopupAfterSeconds(PopupText popup, string text, float speechTime)
          {
               var popupTime = Time.time + speechTime;

               while (popupTime > Time.time)
               {
                    if (_skipPresentation) break;
                    yield return null;
               }
               popup.DisplayPopupText(text);
          }
          
          private void ResetView()
          {
               StopErrorCoroutine();
               StopTypingCoroutine();
               StopGameOverCoroutine();

               storyText.text = string.Empty;
               errorPopup.HidePopupText();
               defeatPopup.HidePopupText();
               victoryPopup.HidePopupText();
          }

          public override void SkipCurrentPresentation()
          {
               _skipPresentation = true;
          }
          
          private void StopErrorCoroutine()
          {
               if (_errorCoroutine == null) return;
               StopCoroutine(_errorCoroutine);
               _errorCoroutine = null;
          }
          
          private void StopGameOverCoroutine()
          {
               if (_gameOverCoroutine == null) return;
               StopCoroutine(_gameOverCoroutine);
               _gameOverCoroutine = null;
          }

          private void StopTypingCoroutine()
          {
               if (_typingCoroutine == null) return;
               StopCoroutine(_typingCoroutine);
               _typingCoroutine = null;
          }
     }
}