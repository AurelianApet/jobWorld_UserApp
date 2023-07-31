using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class MainController : MonoBehaviour {
	public GameObject nameInputObj;
	public GameObject locationInputObj;
	public GameObject emailInputObj;
	public GameObject popupWnd;
	public GameObject loadingImage;
    public GameObject QuitPopup;

	bool LoadingPictureFlag = false;
	bool ResponseStatus = false;
	bool BackButtonClicked = false;

	// Use this for initialization
	void Start()
	{
		loadingImage.SetActive (false);
		popupWnd.SetActive (false);
        QuitPopup.SetActive(false);

    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
            if (loadingImage.activeInHierarchy == false)
            {
                if (QuitPopup.activeInHierarchy == false)
                    QuitPopup.SetActive(true);
            }
        }
	}
		
    public void onQuitPopupbtnOk()
    {
        Application.Quit();
    }

    public void onQuitPopupbtnCancel()
    {
        QuitPopup.SetActive(false);
    }

	public void OnConfirmBtn()
	{
		string user_name = "";
		string user_location = "";
		string user_email = "";
		user_name = nameInputObj.transform.GetComponent<UIInput> ().value;
		user_location = locationInputObj.transform.GetComponent<UIInput> ().value;
		user_email = emailInputObj.transform.GetComponent<UIInput> ().value;

		if (user_name == "" || user_location == "" || user_email == "") {
			//please input all fields
			popupWnd.SetActive (true);
			popupWnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "입력칸을 채워 주십시오.";
		} else if (Regex.IsMatch (user_email, @"^[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_\.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,3}$") == false) {			//
			popupWnd.SetActive (true);
			popupWnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "이메일을 정확히 입력하세요.";
		} else if(user_location.Length > 10)
		{
			popupWnd.SetActive (true);
			popupWnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "출입처는 10자 이하로 입력하세요.";
		}
		else {
			Global.user_name = user_name;
			Global.user_location = user_location;
			Global.user_email = user_email;

			PlayerPrefs.SetString (Global.usernamePrefText, Global.user_name);
			PlayerPrefs.SetString (Global.userlocationPrefText, Global.user_location);
			PlayerPrefs.SetString (Global.useremailPrefText, Global.user_email);

			StartCoroutine (SendInfoToServer());
		}
	}

	public void OnOkPopupBtn()
	{
		popupWnd.SetActive (false);
	}

	IEnumerator SendInfoToServer()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("type", "0");
		form.AddField ("id", Global.app_id);
		form.AddField ("name", Global.user_name);
		form.AddField ("place", Global.user_location);
		form.AddField ("email", Global.user_email);

		string requestURL = Global.DOMAIN + Global.UserInfoUrl;
		WWW www = new WWW (requestURL, form);

		yield return www;

		LoadingPictureFlag = true;
		StartCoroutine (PlayLoadingImage ());

		//Get the response from server --------------2-------------
		Global.isStartedExperience = false;
		while(true)
		{
			ResponseStatus = false;

			GetStatusFromServer ();

			//wait while the request finish
			while (ResponseStatus == false) {
				yield return new WaitForSeconds (0.3f);
			}

			//sleep 2 seconds after request finish			
			yield return new WaitForSeconds (2.0f);

			if (Global.isStartedExperience == true || BackButtonClicked == true) {
				break;
			}
		}

		LoadingPictureFlag = false;
		if (Global.isStartedExperience == true) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.work_scene);
		} else {
			GameObject.Find ("SplashManage").transform.GetComponent<SplashController> ().Start ();		//back button clicked
		}
	}

	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (LoadingPictureFlag == true) {
			lot_y += 5;
			loadingImage.transform.FindChild ("Loading").transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}

		loadingImage.SetActive (false);
	}

	public void GetStatusFromServer()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("type", "0");
		form.AddField ("id", Global.app_id);

		string requestURL = Global.DOMAIN + Global.StatusReturnUrl;
		WWW www = new WWW (requestURL, form);

		StartCoroutine (GetResponse (www));
	}

	IEnumerator GetResponse(WWW www)
	{
		yield return www;
		if (www.error == null) {
			if (!string.IsNullOrEmpty (www.text)) {
				if (www.text == "2") {				//check the starting Experience and go to login
					Global.isStartedExperience = true;
					BackButtonClicked = false;
				} else if(www.text == "0"){
					BackButtonClicked = true;
					Global.isStartedExperience = false;
				}else
					Global.isStartedExperience = false;
			} else {
				Debug.Log ("-------main------Get status fail-------------");
				Global.isStartedExperience = false;
			}
		} else {
			Debug.Log ("-------main------Error on Request to server-------------");
			Global.isStartedExperience = false;
		}

		ResponseStatus = true;
	}
}
