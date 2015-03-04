using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Threading.Tasks;
using Parse;

public class SignUp : MonoBehaviour {

	Text StatusText;
	InputField UsernameInput;
	InputField EmailInput;
	InputField PasswordInput;

	// Use this for initialization
	void Start () {
		StatusText = GameObject.Find ("StatusText").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Button_Back_Clicked(){
		Application.LoadLevel ("Login");
	}

	public void Button_SignUp_Clicked(){
		UsernameInput = GameObject.Find ("UsernameInput").GetComponent<InputField> ();
		EmailInput = GameObject.Find ("EmailInput").GetComponent<InputField> ();
		PasswordInput = GameObject.Find ("PasswordInput").GetComponent<InputField> ();
		object[] parms = new object[3]{UsernameInput.text, EmailInput.text, PasswordInput.text};

		StartCoroutine ("DoSignUp", parms);
	}

	IEnumerator DoSignUp(object[] parms){
		var newUser = new ParseUser()
		{
			Username = parms[0].ToString(),
			Email = parms[1].ToString(),
			Password = parms[2].ToString()
		};
		newUser ["displayName"] = parms [0].ToString ();

		var signUpTask = newUser.SignUpAsync();
		while (!signUpTask.IsCompleted) yield return null;
		if (!signUpTask.IsCanceled && !signUpTask.IsFaulted) {
			Debug.Log ("Succesfully signed up");
			StatusText.text = "Succesfully signed up";
			AppModel.LoginWithUser(newUser);
		}
		else{
			StatusText.text = "Something went wrong";
			foreach(var e in signUpTask.Exception.InnerExceptions) 
			{
				ParseException parseException = (ParseException) e;
				Debug.Log("Error message " + parseException.Message);
				Debug.Log("Error code: " + parseException.Code);
			}
		}
	}
}
