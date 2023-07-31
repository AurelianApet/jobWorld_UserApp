using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SplashController : MonoBehaviour {
	private bool ResponseStatus =false;
	private bool LoadImagesToTexture = false;
	private int LoadSceneFlag = -1;

	public GameObject maskObject;

	// Use this for initialization
	public void Start () {
		Screen.orientation = ScreenOrientation.Portrait;

		maskObject.SetActive (true);
		Global.m_Title = "";
		Global.m_Summary = "";
		Global.m_Content = "";

		Global.gPictureTextures.texture_List = new List<Texture2D> ();
		Global.gPictureTextures.texture_id = new List<int> ();

		GameObject.Find ("LoginManage").transform.GetComponent<MainController> ().nameInputObj.transform.FindChild ("Label").transform.GetComponent<UILabel> ().text = "이름";
		GameObject.Find ("LoginManage").transform.GetComponent<MainController> ().locationInputObj.transform.FindChild ("Label").transform.GetComponent<UILabel> ().text = "출입처";
		GameObject.Find ("LoginManage").transform.GetComponent<MainController> ().emailInputObj.transform.FindChild ("Label").transform.GetComponent<UILabel> ().text = "이메일";

		StartCoroutine ("ResponseRoutine");
	}
	
	// Update is called once per frame
	void Update () {
		
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

	IEnumerator ProcessCapturedImages(int type)
	{
		Global.gPictureTextures.texture_List.Clear();
		Global.gPictureTextures.texture_id.Clear ();

		string path = "";
		#if UNITY_IPHONE
		path = Application.persistentDataPath + "/" + "NewsApp";

		#elif UNITY_ANDROID	
		path = "mnt/sdcard/DCIM/" + "NewsApp";	//"mnt/sdcard/DCIM/"

		#else
		if( Application.isEditor == true ){ 
			path = "mnt/sdcard/DCIM/";
		} 
		#endif

		if (!Directory.Exists(path + "/picture"))
		{
			Directory.CreateDirectory(path + "/picture");
		}
		string dirPath = path + "/picture/";

		var fileInfo =Directory.GetFiles(dirPath);
		int file_count = fileInfo.Length;

		string prePath = "";
		#if UNITY_IPHONE
		prePath = @"file://";
		#elif UNITY_ANDROID	
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#else
		prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		#endif

		if (type == 1) {						//delete all the images
			for(int i=0;i<file_count;i++)
			{
				File.Delete (fileInfo [i]);
			}


		} else if (type == 2) {					//read all images to global Texture variable
			if (!PlayerPrefs.HasKey (Global.usernamePrefText)) {
                Debug.Log("------------user name pref may you-----------");
				for(int i=0;i<file_count;i++)
				{
					File.Delete (fileInfo [i]);
				}
			} else {
                Debug.Log("------------user name pref  you-----------");
                for (int i = 0; i < file_count; i++) {
					string langPath = fileInfo [i];
					WWW pictureWWW = new WWW (prePath + langPath);
					yield return pictureWWW;

					Texture2D texTmp = new Texture2D (400, 300, TextureFormat.ARGB32, false);
					pictureWWW.LoadImageIntoTexture (texTmp);
					Global.gPictureTextures.texture_List.Add (texTmp);
					Debug.Log ("-------------------" + i + ":" + fileInfo [i] + "---------------------");
					string[] temp = fileInfo [i].Split ('/');
					string fileName = temp [temp.Length - 1];
					fileName = fileName.Split ('.') [0];
					Global.gPictureTextures.texture_id.Add (System.Convert.ToInt32 (fileName));
				}
			}
		}
		yield return new WaitForSeconds (0.1f);
		LoadImagesToTexture = false;
	}

	IEnumerator ResponseRoutine()
	{
		if (Global.isStartedExperience == true) {
			yield return new WaitForSeconds (0.1f);
			LoadSceneFlag = 0;
		}
		else {
			while (true) {
				ResponseStatus = false;

				GetStatusFromServer ();

				//wait while the request finish
				while (ResponseStatus == false) {
					yield return new WaitForSeconds (0.3f);
				}

				//sleep 2 seconds after request finish
				yield return new WaitForSeconds (1.0f);

				if (Global.isStartedExperience == true) {
					break;
				}
			}
		}
		UnlockWaitingScreen ();
	}

	public IEnumerator ResponseBackState()
	{
		StopCoroutine ("ResponseRoutine");
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

			if (Global.isStartedExperience == false) {
				break;
			}
		}

		Start ();
	}

	public void UnlockWaitingScreen()
	{
		if (LoadSceneFlag == 0) {
			maskObject.SetActive (false);
			StartCoroutine ("ResponseBackState");
		} else if (LoadSceneFlag == 1) {
			Global.WorkSceneLoadFlag = 2;
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.work_scene);
		} else if (LoadSceneFlag == 2) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.meeting_scene);
		}
	}

	public void OnConfirmBtn()			//on confirm button clicked for stop the coroutine
	{
		StopCoroutine ("ResponseBackState");
	}

	IEnumerator GetResponse(WWW www)
	{
		yield return www;
		if (www.error == null) {
			if (!string.IsNullOrEmpty (www.text)) {
				if (www.text == "0") {						//un check the starting Experience
					Global.isStartedExperience = false;
				} else if (www.text == "1") {				//check the starting Experience and go to login
					Global.isStartedExperience = true;
					StartCoroutine(ProcessCapturedImages (1));

					//code for Load scene -------"Login Scene"-----------
					LoadSceneFlag = 0;

					LoadImagesToTexture = true;
					while (LoadImagesToTexture == true) {
						yield return new WaitForSeconds (0.2f);
					}
				} else if (www.text == "2" || www.text == "3") {				//check the starting Experience and go to login
					Global.isStartedExperience = true;
					StartCoroutine(ProcessCapturedImages (2));

					//code for Load scene -------"Work Scene"-----------
					if(www.text == "2")
						LoadSceneFlag = 1;
					else 
						LoadSceneFlag = 2;

					LoadImagesToTexture = true;
					while (LoadImagesToTexture == true) {
						yield return new WaitForSeconds (0.2f);
					}
				}
			} else {
				Debug.Log ("-------splash------Get status fail-------------");
				Global.isStartedExperience = false;
			}
		} else {
			Debug.Log ("-------splash------Error on Request to server-------------");
			Global.isStartedExperience = false;
		}

		ResponseStatus = true;
	}
}
