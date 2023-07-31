using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class PictureController : MonoBehaviour {
	public GameObject Vcamera;
	public GameObject loadingImage;
	public GameObject rawImage;
	WebCamTexture camTexture;
	WebCamTexture camTexture1;
	WebCamDevice[] devices;
	bool isCapturing = false;
	public GameObject uiCamera;

	void Update () {
		
	}
		
	public void OnCaptureBtn()
	{
		if (isCapturing == false) {
			isCapturing = true;		//set to false for next capture
			StartCoroutine(PlayLoadingImage());

			#if UNITY_ANDROID
			AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"); 
			AndroidJavaObject jcontext = jc.GetStatic<AndroidJavaObject> ("currentActivity");

			AndroidJavaClass cls_jni = new AndroidJavaClass ("com.coar.audiocontrol.AudioManager");
			string return_string = cls_jni.CallStatic<string> ("SetVolume", jcontext, 5);
			#endif

			StartCoroutine (ScreenShot ()); 
		}
	}

	public void OnBtnChangeCam()
	{
		if (Global.bCamFront == false) {
			camTexture.Stop();
			camTexture = new WebCamTexture(devices[1].name, 1280, 720);
			camTexture.Play();

			Global.bCamFront = true;
		} else {
			camTexture.Stop();
			camTexture = new WebCamTexture(devices[0].name, 1280, 720);
			camTexture.Play();
			Global.bCamFront = false;
		}

		rawImage.GetComponent<MeshRenderer>().material.mainTexture = camTexture;
	}

	public void OnBtnBack()
	{
		//uiCamera.SetActive (false);
		SceneManager.LoadScene (Global.work_scene);
	}

	IEnumerator ScreenShot()
	{
		yield return new WaitForEndOfFrame ();
		Camera vcamera = Vcamera.GetComponent<Camera>();
		RenderTexture curRT = RenderTexture.active;
		RenderTexture.active = vcamera.targetTexture;
		vcamera.Render ();

		int width = 0;
		int height = 0;
		if (Screen.width * 3 < Screen.height * 4) {
			width = Screen.width;
			height = Screen.width * 3 / 4;
		} else {
			height = Screen.height;
			width = Screen.height * 4 / 3;
		}
		Texture2D captureTexture = new Texture2D (width, height, TextureFormat.RGB24, true);
		captureTexture.ReadPixels (new Rect (0, 0, width, height), 0, 0, true);
		captureTexture.Apply ();
		RenderTexture.active = curRT;

		byte[] imageByte = captureTexture.EncodeToPNG ();
		Debug.Log ("before capture:");

		//Get Picture Name
		int picId = 0;
		if (PlayerPrefs.HasKey (Global.pictureIdPrefText))
			picId = PlayerPrefs.GetInt (Global.pictureIdPrefText);

		//add this image to Global variable
		Global.gPictureTextures.texture_List.Add(captureTexture);
		Global.gPictureTextures.texture_id.Add (picId);

		string dirPath = "";
		#if UNITY_IPHONE
		dirPath = Application.persistentDataPath + "/" + "NewsApp";
		#elif UNITY_ANDROID	
		dirPath = "mnt/sdcard/DCIM/" + "NewsApp";	//"mnt/sdcard/DCIM/"
		#else
		if( Application.isEditor == true ){ 
			dirPath = "mnt/sdcard/DCIM/";
		} 
		#endif

		if (!Directory.Exists(dirPath + "/picture"))
		{
			Directory.CreateDirectory(dirPath + "/picture");
		}
		string destination = dirPath + "/picture/" + picId + ".png";

		picId++;
		PlayerPrefs.SetInt (Global.pictureIdPrefText, picId);

		File.WriteAllBytes(destination, imageByte);
		isCapturing = false;
	}

	IEnumerator PlayLoadingImage()
	{
		int lot_y = 0;
		loadingImage.SetActive (true);
		while (isCapturing == true) {
			lot_y += 5;
			loadingImage.transform.FindChild ("Loading").transform.localRotation = Quaternion.Euler (0, 0, -lot_y);
			yield return new WaitForSeconds (0.03f);
		}

		loadingImage.SetActive (false);
	}

	void Start()
	{
		//uiCamera.SetActive (false);
		loadingImage.SetActive (false);
		Screen.orientation = ScreenOrientation.LandscapeLeft;

		//yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
		//if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone))
		//{
			devices = WebCamTexture.devices;

			camTexture = new WebCamTexture(devices[0].name, 1280, 720);
			camTexture.Play();

			rawImage.GetComponent<MeshRenderer>().material.mainTexture = camTexture;
		//} else {

		//}

		//uiCamera.SetActive (true);
	}

	void OnDestroy() {
		camTexture.Stop();
	}
}
