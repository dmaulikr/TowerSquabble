using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Parse;
using System.Threading.Tasks;

public class Login : MonoBehaviour {
	InputField UsernameInput;
	InputField PasswordInput;
	Text StatusText;

	// Use this for initialization
	void Start () {
		StatusText = GameObject.Find ("StatusText").GetComponent<Text> ();
		if (ParseUser.CurrentUser != null) {
			AppModel.LoginWithUser(ParseUser.CurrentUser);
		}
	}

	public void Button_SignOut_Clicked(){
		try{
			ParseUser.LogOut ();
		}
		catch(UnityException e){
			Debug.Log ("Error logging out: " + e);
		}
	}

	public void Button_SignIn_Clicked(){
		UsernameInput = GameObject.Find ("UsernameInput").GetComponent<InputField> ();
		PasswordInput = GameObject.Find ("PasswordInput").GetComponent<InputField> ();
		object[] parms = new object[2]{UsernameInput.text, PasswordInput.text};
		StartCoroutine("SignIn", parms);
	}

	public void Button_SignUp_Clicked(){
		Application.LoadLevel ("SignUp");
	}

	IEnumerator SignIn(object[] parms) {
		var user = ParseUser.LogInAsync (parms[0].ToString(), parms[1].ToString());

		while (!user.IsCompleted) yield return null;
		if (user.IsCanceled || user.IsFaulted) {
			Debug.Log(user.Exception.InnerExceptions[0]);
			StatusText.text = user.Exception.InnerExceptions[0].ToString();
		}
		if (ParseUser.CurrentUser != null) {
			AppModel.LoginWithUser(ParseUser.CurrentUser);
		} else {
			StatusText.text = "Wrong user or password";
		}
	}
}
