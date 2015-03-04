using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;
using System.IO;
using Parse; 

public class SplashScreen : MonoBehaviour {
	
	private string checkUrl = "http://google.com";
	private Text infoText;
	
	void Start()
	{	
		infoText = GameObject.Find ("InfoText").GetComponent<Text>();
		StartCoroutine("CheckConnection");
	}
	
	public IEnumerator CheckConnection(){
		string result = GetHtmlFromUri(checkUrl);
		if (!result.Contains ("schema.org/WebPage")) 
		{
			infoText.text = "Check your internet connection and try again...";
		}
		else
		{
			if (ParseUser.CurrentUser != null) {
				AppModel.LoginWithUser(ParseUser.CurrentUser);
			}
			else
			{
				Application.LoadLevel("login");
			}
		}
		//wait a couple of seconds before checking connection again
		yield return new WaitForSeconds (5f);
		StartCoroutine ("CheckConnection");
	}
	
	public string GetHtmlFromUri(string url)
	{
		string html = string.Empty;
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		//set request timeout
		request.Timeout = 4500;
		request.ReadWriteTimeout = 4500;
		
		try
		{
			using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
			{
				bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
				if (isSuccess)
				{
					using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
					{
						//Limit the array to 80 so we dont have to parse the entire html document 
						char[] cs = new char[80];
						reader.Read(cs, 0, cs.Length);
						foreach(char ch in cs)
						{
							html +=ch;
						}
					}
				}
			}
		}
		catch
		{
			return "";
		}
		return html;
	}
}