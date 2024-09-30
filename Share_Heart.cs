using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Share_Heart : MonoBehaviour
{

	public string share_subject = "Invite";
	public string share_msg = "";
	public Button heartButton;
	public Button shareButton;
	public TextMeshProUGUI heart_Count, share_Count;
	private bool isFocus = false;
	private bool isProcessing = false;
	void Start()
	{
		
		heartButton.onClick.AddListener(RateMarket);
		heart_Count.text = Random.Range(1000, 2000).ToString();

		shareButton.onClick.AddListener(ShareText);
		share_Count.text = Random.Range(500, 1000).ToString();
		
	}
	void OnApplicationFocus(bool focus)
	{
		isFocus = focus;
	}
	private void ShareText()
	{

#if UNITY_ANDROID
		if (!isProcessing)
		{
			StartCoroutine(ShareTextInAnroid());
		}
#else
		Debug.Log("No sharing set up for this platform.");
#endif
	}
	//#if UNITY_ANDROID
	public IEnumerator ShareTextInAnroid()
	{
		isProcessing = true;
		if (!Application.isEditor)
		{
			//Create intent for action send
			AndroidJavaClass intentClass =
				new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject =
				new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>
				("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			//put text and subject extra
			intentObject.Call<AndroidJavaObject>("setType", "text/plain");
			intentObject.Call<AndroidJavaObject>
				("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), share_subject);
			intentObject.Call<AndroidJavaObject>
				("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), share_msg + "https://play.google.com/store/apps/details?id=" + Application.identifier);
			//call createChooser method of activity class
			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity =
				unity.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject chooser =
				intentClass.CallStatic<AndroidJavaObject>
				("createChooser", intentObject, "Share your high score");
			currentActivity.Call("startActivity", chooser);
		}
		yield return new WaitUntil(() => isFocus);
		isProcessing = false;
	}



	public void RateMarket()
	{
		// open your app website on Google Play
		Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
		Debug.Log("https://play.google.com/store/apps/details?id=" + Application.identifier);
	}




	//#endif
}