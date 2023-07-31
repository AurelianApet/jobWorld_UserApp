using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeetingController : MonoBehaviour {
	private bool ResponseStatus =false;
	private bool GoToSplashFlag = false;
	private bool BackButtonClicked = false;
	public GameObject loadingImage;
    public GameObject QuitPopup;

	// Use this for initialization
	void Start () {
        QuitPopup.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;

		StartCoroutine ("PlayLoadingImage");
		StartCoroutine (ResponseRoutine ());
	}

	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (true) {
			lot_y += 5;
			loadingImage.transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}

		loadingImage.SetActive (false);
	}

	IEnumerator ResponseRoutine()
	{
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

			if (GoToSplashFlag == true) {
				break;
			}
		}

		StopCoroutine ("PlayLoadingImage");
		if (BackButtonClicked == true) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.work_scene);
		} else {
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.splash_scene);
		}
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
				if (www.text == "0") {						//un check the starting Experience
					GoToSplashFlag = true;
				} else if (www.text == "2") {				//back button clicked
					GoToSplashFlag = true;
					BackButtonClicked = true;
				}
				else{				//check the starting Experience and go to login
					GoToSplashFlag = false;

				}
			} else {
				GoToSplashFlag = false;
			}
		} else {
			GoToSplashFlag = false;
		}

		ResponseStatus = true;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
            if (QuitPopup.activeInHierarchy == false)
                QuitPopup.SetActive(true);
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
}
