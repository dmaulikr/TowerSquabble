using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestionPanel : MonoBehaviour {
	// UI varibles
	public Button yesButton;
	public Button noButton;
	public Button maybeButton;
	public Text questionText;

	// Implementation details
	// This flag will be set to true when the dissable animation is playing
	private bool dissableAnimationInProgress = false;

	private void Dissable()
	{
		gameObject.GetComponent<Animator>().Play("QuestionPanel_Deactivate");
	}

	private void Enable(bool enable)
	{
		//TODO: Animation...
		gameObject.SetActive(enable);
	}

	private bool IsEnabled()
	{
		return gameObject.activeSelf;
	}

	public void YesButtonClicked()
	{

	}

	public void AskQuestion(string question, UnityAction yesEvent, UnityAction noEvent, UnityAction maybeEvent)
	{
		// Is question panel already active?
		if(IsEnabled())
		{
			return; // Do nothing...
		}
		// Enable question panel
		Enable(true);
		// Set question text
		questionText.text = question;

		// Add remove/listeners for all buttons
		yesButton.onClick.RemoveAllListeners();
		yesButton.onClick.AddListener(yesEvent);
		yesButton.onClick.AddListener(Dissable); // We always want to dissable the panel after we have clicked the button
	
		noButton.onClick.RemoveAllListeners();
		noButton.onClick.AddListener(noEvent);
		noButton.onClick.AddListener(Dissable); // We always want to dissable the panel after we have clicked the button

		maybeButton.onClick.RemoveAllListeners();
		maybeButton.onClick.AddListener(maybeEvent);
		maybeButton.onClick.AddListener(Dissable); // We always want to dissable the panel after we have clicked the button
	}

	// Use this for initialization
	void Start() 
	{
		// Dissable question panel 
		Enable(false);
	}
	
	// Update is called once per frame
	void Update() 
	{
		if(!dissableAnimationInProgress && gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("QuestionPanel_Deactivate"))
		{
			dissableAnimationInProgress = true;
		}
		// If dissable animation has finished playing, dissable the control.
		else if(dissableAnimationInProgress && gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
		{
			dissableAnimationInProgress = false;
			Enable(false);
		}
	}
}
