using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WriteController : MonoBehaviour {
	public GameObject locationTextObj;
	public GameObject userNameObj;

	public GameObject subjectInput;
	public GameObject contentInput;
	public GameObject summaryInput;
	public GameObject pictureTexture;

	public GameObject cancelWnd;
	public GameObject saveWnd;
	public GameObject sendingWnd;
	public GameObject internetWnd;
	public GameObject popupwnd;
	public GameObject helpWnd;
	public GameObject loadingImage;

	public GameObject PictureWnd;
	public GameObject firstPicLine;

	public GameObject m_PicturePrefeb;
	public GameObject[] m_PictureMixes;
	public GameObject scrollWnd;

    public GameObject quitPopup;

	private bool LoadingPictureFlag = false;
	private bool getResposeAfterUploadFlag = false;
	bool ResponseStatus = false;
	private int AfterUploadFlag = 0;




	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;

		//initialize the location and name
		InitFields ();
	}

	IEnumerator ReadServerStatus()
	{
		while(true)
		{
			ResponseStatus = false;

			GetStatusFromServer ();

			//wait while the request finish
			while (ResponseStatus == false) {
				yield return new WaitForSeconds (0.3f);
			}

			if (AfterUploadFlag == 1) {						//re write the experiences
				loadingImage.SetActive (false);
			} else if (AfterUploadFlag != 0)
				break;
			//sleep 2 seconds after request finish
			yield return new WaitForSeconds (2.0f);
		}

		if (AfterUploadFlag == 2) {				//편집회의 진행중 기다림
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.meeting_scene);
		} else if (AfterUploadFlag == 3) {				//back button clicked
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.splash_scene);
		}
	}

	public void InitFields()
	{
        quitPopup.SetActive(false);
		cancelWnd.SetActive (false);
		saveWnd.SetActive (false);
		loadingImage.SetActive (false);
		sendingWnd.SetActive (false);
		internetWnd.SetActive (false);
		popupwnd.SetActive (false);
		helpWnd.SetActive (false);
		PictureWnd.SetActive (false);

		m_PicturePrefeb = (Resources.Load<GameObject> ("Prefabs/PictureMix"));

		locationTextObj.transform.GetComponent<UILabel>().text = PlayerPrefs.GetString(Global.userlocationPrefText);
		userNameObj.transform.GetComponent<UILabel>().text = PlayerPrefs.GetString(Global.usernamePrefText);

		subjectInput.transform.GetComponent<UIInput> ().value = Global.m_Title;
		contentInput.transform.GetComponent<UIInput> ().value = Global.m_Content;
		summaryInput.transform.GetComponent<UIInput> ().value = Global.m_Summary;

		if (Global.selectedPictureName != "") {
			LoadingPictureFlag = true;
			StartCoroutine ("PlayLoadingImage");
			StartCoroutine (LoadPictureToTexture());
		}

		if (Global.WorkSceneLoadFlag == 1) {
			onPictureIcon ();
			Global.WorkSceneLoadFlag = 0;
		}

		if (Global.WorkSceneLoadFlag == 2) {
			//code for load saved information to all input fields except picture field
			onCancelBtnOk();
			//StartCoroutine ("ReadServerStatus");
		}

		StartCoroutine ("ReadServerStatus");
	}

    IEnumerator ClosePictureWnd()
    {
        yield return new WaitForSeconds(0.5f);
        PictureWnd.SetActive(false);
    }

	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
            if (PictureWnd.activeInHierarchy == false)
            {
                if (quitPopup.activeInHierarchy == false)
                    quitPopup.SetActive(true);
            }
            else if (PictureWnd.activeInHierarchy)
            {
                StartCoroutine(ClosePictureWnd());
            }
		}
	}

    public void onQuitPopupbtnOk()
    {
        quitPopup.SetActive(false);
        Application.Quit();
    }

    public void onQuitPopupbtnCancel()
    {
        quitPopup.SetActive(false);
    }

	public void onInfoIcon()
	{
		if(PictureWnd.activeInHierarchy == false)
			helpWnd.SetActive (true);
	}

	public void onPictureIcon()
	{
		Global.m_Title = subjectInput.transform.GetComponent<UIInput> ().value;
		Global.m_Content = contentInput.transform.GetComponent<UIInput> ().value;
		Global.m_Summary = summaryInput.transform.GetComponent<UIInput> ().value;

		PictureWnd.SetActive (true);
		//load Textures to Screen
		int line_count = 0;
		if (Global.gPictureTextures.texture_List.Count <= 3) {
            line_count = 1;
		} else {
            line_count = (Global.gPictureTextures.texture_List.Count - 3) / 4 + 1;
            if ((Global.gPictureTextures.texture_List.Count - 3) % 4 != 0)
                line_count += 1;
		}
		m_PictureMixes = new GameObject[line_count];

        Debug.Log("-------------dddd--------------");
        int dummy = 0;
		for(int i=0; i<m_PictureMixes.Length;i++){
			if (i == 0) {
				m_PictureMixes [i] = firstPicLine;
				for (int k = 0; k < (Global.gPictureTextures.texture_List.Count > 3 ? 3 : Global.gPictureTextures.texture_List.Count); k++) {
					m_PictureMixes [i].transform.FindChild ("Texture" + k).transform.GetComponent<UITexture> ().mainTexture = Global.gPictureTextures.texture_List [k];
					m_PictureMixes [i].transform.FindChild ("Texture" + k).transform.GetComponent<PictureSelect> ().selected_picture_id = Global.gPictureTextures.texture_id[k];
				}
				dummy++;
			} else {
				m_PictureMixes [dummy] = (GameObject)UnityEngine.Object.Instantiate (m_PicturePrefeb, new Vector3 (0f, 640f-dummy*272f, 0f), Quaternion.identity);
				m_PictureMixes [dummy].transform.parent = scrollWnd.transform;

				m_PictureMixes [dummy].transform.GetComponent<Transform> ().localPosition = new Vector3 (0f, 640f-dummy*272f, 0f);
				m_PictureMixes [dummy].transform.GetComponent<Transform>().localScale = new Vector3 (1.0f, 1.0f, 1.0f);

				for (int k = 0; k < 4; k++) {
					int textureId = 3 + (dummy - 1) * 4 + k;
					if (Global.gPictureTextures.texture_List.Count > textureId) {
						m_PictureMixes [i].transform.FindChild ("Texture" + k).transform.GetComponent<UITexture> ().mainTexture = Global.gPictureTextures.texture_List [textureId];
						m_PictureMixes [i].transform.FindChild ("Texture" + k).transform.GetComponent<PictureSelect> ().selected_picture_id = Global.gPictureTextures.texture_id [textureId];
					}
				}
				dummy++;
			}
		}

	}

	public void onPictureBtn()
	{
        //GameObject.Find ("UI Root/Camera").transform.gameObject.SetActive (false);
        Debug.Log("-------------eee------------");
		UnityEngine.SceneManagement.SceneManager.LoadScene (Global.picture_scene);
        Debug.Log("-------------ffff------------");
        Global.WorkSceneLoadFlag = 1;
	}

	public void onSaveBtnOk()
	{
		saveWnd.SetActive (false);
		string subject = "";
		string content = "";
		string summary = "";

		subject = subjectInput.transform.GetComponent<UIInput> ().value;
		content = contentInput.transform.GetComponent<UIInput> ().value;
		summary = summaryInput.transform.GetComponent<UIInput> ().value;

		PlayerPrefs.SetInt (Global.flagPrefText, 1);
		PlayerPrefs.SetString (Global.subjectPrefText, subject);
		PlayerPrefs.SetString (Global.contentPrefText, content);
		PlayerPrefs.SetString (Global.summaryPrefText, summary);
		PlayerPrefs.SetString (Global.picturenamePrefText, Global.selectedPictureName);

		popupwnd.SetActive (true);
		popupwnd.transform.FindChild ("title").transform.GetComponent<UILabel> ().text = "저장완료";
		popupwnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "임시 저장 되었습니다.";
	}

	public void onSaveBtnCancel()
	{
		saveWnd.SetActive (false);
	}

	public void onSaveBtn()
	{
		internetWnd.SetActive (false);
		saveWnd.SetActive (true);
	}

	public void onSendBtn()
	{
		if (subjectInput.transform.GetComponent<UIInput> ().value == "" || contentInput.transform.GetComponent<UIInput> ().value == "" ||
		    summaryInput.transform.GetComponent<UIInput> ().value == "") {
			popupwnd.SetActive (true);
			popupwnd.transform.FindChild ("title").transform.GetComponent<UILabel> ().text = "경고";
			popupwnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "빈칸을 채워주세요.";
		} else if (subjectInput.transform.GetComponent<UIInput> ().value.Length > 30) {
			popupwnd.SetActive (true);
			popupwnd.transform.FindChild ("title").transform.GetComponent<UILabel> ().text = "경고";
			popupwnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "제목은 30자 이하로 입력하세요.";
		} else if (contentInput.transform.GetComponent<UIInput> ().value.Length > 500) {
			popupwnd.SetActive (true);
			popupwnd.transform.FindChild ("title").transform.GetComponent<UILabel> ().text = "경고";
			popupwnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "내용은 500자 이하로 입력하세요.";
		} else if (summaryInput.transform.GetComponent<UIInput> ().value.Length > 250) {
			popupwnd.SetActive (true);
			popupwnd.transform.FindChild ("title").transform.GetComponent<UILabel> ().text = "경고";
			popupwnd.transform.FindChild ("content").transform.GetComponent<UILabel> ().text = "요약문은 250자 이하로 입력하세요.";
		}
		else {
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				internetWnd.SetActive (true);
			} else {
				sendingWnd.SetActive (true);
			}
		}
	}

	//send the info to server
	public void onSendOkBtn()
	{
		//save the sending data to local db
		string subject = "";
		string content = "";
		string summary = "";

		subject = subjectInput.transform.GetComponent<UIInput> ().value;
		content = contentInput.transform.GetComponent<UIInput> ().value;
		summary = summaryInput.transform.GetComponent<UIInput> ().value;

		PlayerPrefs.SetInt (Global.flagPrefText, 1);
		PlayerPrefs.SetString (Global.subjectPrefText, subject);
		PlayerPrefs.SetString (Global.contentPrefText, content);
		PlayerPrefs.SetString (Global.summaryPrefText, summary);
		PlayerPrefs.SetString (Global.picturenamePrefText, Global.selectedPictureName);

		StopCoroutine ("ReadServerStatus");
		StartCoroutine ("UploadDataToServer");
	}

	public void onSendCancelBtn()
	{
		sendingWnd.SetActive (false);
		internetWnd.SetActive (false);
	}

	public void onCancelBtnOk()
	{
		cancelWnd.SetActive (false);

		if (PlayerPrefs.HasKey (Global.flagPrefText)) {
			if (PlayerPrefs.GetInt (Global.flagPrefText) == 1) {
				subjectInput.transform.GetComponent<UIInput> ().value = PlayerPrefs.GetString (Global.subjectPrefText);
				contentInput.transform.GetComponent<UIInput> ().value = PlayerPrefs.GetString (Global.contentPrefText);
				summaryInput.transform.GetComponent<UIInput> ().value = PlayerPrefs.GetString (Global.summaryPrefText);

				if (PlayerPrefs.HasKey (Global.picturenamePrefText)) {
					if (PlayerPrefs.GetString (Global.picturenamePrefText) != "") {
						LoadingPictureFlag = true;
						StartCoroutine ("PlayLoadingImage");
						StartCoroutine (LoadPictureToTexture(PlayerPrefs.GetString (Global.picturenamePrefText)));
					}
				}

				if (Global.WorkSceneLoadFlag != 2) {
					PlayerPrefs.SetInt (Global.flagPrefText, 0);

				}
			} else {
				subjectInput.transform.GetComponent<UIInput> ().value = "";
				contentInput.transform.GetComponent<UIInput> ().value = "";
				summaryInput.transform.GetComponent<UIInput> ().value = "";
				pictureTexture.transform.GetComponent<UITexture>().mainTexture = null;
			}
		}
		else {
			subjectInput.transform.GetComponent<UIInput> ().value = "";
			contentInput.transform.GetComponent<UIInput> ().value = "";
			summaryInput.transform.GetComponent<UIInput> ().value = "";
			pictureTexture.transform.GetComponent<UITexture>().mainTexture = null;
		}

		if(Global.WorkSceneLoadFlag == 2)
			Global.WorkSceneLoadFlag = 0;
	}

	public void onCancelBtnCancel()
	{
		cancelWnd.SetActive (false);
	}

	public void onCancelBtn()
	{
		cancelWnd.SetActive (true);
	}

	public void onPopupBtnOk()
	{
		popupwnd.SetActive (false);
	}

	public void onHelpWndBtnOk()
	{
		helpWnd.SetActive (false);
	}

	IEnumerator UploadDataToServer()
	{
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

		string prePath = "";
		#if UNITY_IPHONE
		prePath = @"file://" + Application.persistentDataPath;
		#elif UNITY_ANDROID
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#endif



		WWWForm postForm = new WWWForm ();
		postForm.headers.Add ("Host", Global.HostName);
		postForm.headers.Add ("Origin", "null");
		postForm.headers.Add ("Connecton", "keep-alive");
		postForm.headers.Add ("Cache-Control", "max-age=0");
		postForm.headers.Add ("Upgrade-Insecure-Requests", "1");
		postForm.headers.Add ("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
		postForm.headers.Add ("Accept", "image/jpeg, application/x-ms-application, image/gif, application/xaml+xml, image/pjpeg, application/x-ms-xbap, application/x-shockwave-flash, */*");
		postForm.headers.Add ("Accept-Encoding", "gzip, deflate");
		postForm.headers.Add ("Accept-Language", "ko-KR,ko;q=0.8,en-US;q=0.6,en;q=0.4");
		if(Global.selectedPictureName != "")
		{
			WWW localFile = new WWW (prePath + path + "/picture/" + Global.selectedPictureName);
			yield return localFile;

			if (localFile.isDone)
				Debug.Log ("Loaded file successfully");
			else {
				Debug.Log ("Open file error: " + localFile.error);
				yield break;
			}
			string now_Time = System.DateTime.Now.ToString ("yyyyMMddHHmmss");
			postForm.AddBinaryData ("file", localFile.bytes, now_Time + "_" + Global.app_id.ToString () + "_" + Global.selectedPictureName, "image/png");
		}

		postForm.AddField ("type", "0");
		postForm.AddField ("id", Global.app_id.ToString());
		postForm.AddField ("value", "2");
		postForm.AddField ("title", subjectInput.transform.GetComponent<UIInput> ().value);
		postForm.AddField ("content", contentInput.transform.GetComponent<UIInput> ().value);
		postForm.AddField ("summary", summaryInput.transform.GetComponent<UIInput> ().value);

		WWW upload = new WWW (Global.DOMAIN + Global.FileUploadUrl, postForm);

		yield return upload;

		sendingWnd.SetActive (false);
		getResposeAfterUploadFlag = true;
		StartCoroutine (PlayLoadingImage ());

		//Get the response from server --------------2-------------
		while(true)
		{
			ResponseStatus = false;

			GetStatusFromServer ();

			//wait while the request finish
			while (ResponseStatus == false) {
				yield return new WaitForSeconds (0.3f);
			}

			if (AfterUploadFlag == 1) {						//re write the experiences
				loadingImage.SetActive (false);
			} else if (AfterUploadFlag != 0)
				break;
			//sleep 2 seconds after request finish
			yield return new WaitForSeconds (2.0f);
		}

		getResposeAfterUploadFlag = false;

		if (AfterUploadFlag == 2) {				//편집회의 진행중 기다림
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.meeting_scene);
		} else if (AfterUploadFlag == 3) {				//back button clicked
			UnityEngine.SceneManagement.SceneManager.LoadScene (Global.splash_scene);
		}
	}

	public void GetStatusFromServer()
	{
		WWWForm form = new WWWForm ();
		form.AddField ("type", "2");
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
				if (www.text == "4") {					//cancel button clicked in page
					AfterUploadFlag = 1;
				} else if (www.text == "3") {			//go to meeting scene
					AfterUploadFlag = 2;
				} else if (www.text == "1") {			//back button clicked in page
					AfterUploadFlag = 3;
				} else 
					AfterUploadFlag = 0;
			} else {
				AfterUploadFlag = 0;
			}
		} else {
			AfterUploadFlag = 0;
		}

		ResponseStatus = true;
	}

	IEnumerator LoadPictureToTexture(string name = "")
	{
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

		string prePath = "";
		#if UNITY_IPHONE
		prePath = @"file://";
		#elif UNITY_ANDROID	
		//prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		prePath = @"file:///";
		#else
		prePath = @"file://" + Application.dataPath.Replace("/Assets","/");
		#endif

		string langPath = "";
		if(name == "")
			langPath = path + "/picture/" + Global.selectedPictureName;
		else
			langPath = path + "/picture/" + name;
		WWW pictureWWW = new WWW(prePath + langPath);
		yield return pictureWWW;

		Texture2D texTmp = new Texture2D (400, 300, TextureFormat.ARGB32, false);
		pictureWWW.LoadImageIntoTexture (texTmp);

		pictureTexture.transform.GetComponent<UITexture>().mainTexture = texTmp;

		LoadingPictureFlag = false;
	}

	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (LoadingPictureFlag == true || getResposeAfterUploadFlag == true) {
			lot_y += 5;
			loadingImage.transform.FindChild ("Loading").transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}

		loadingImage.SetActive (false);
	}
}
