using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour {
	public struct PictureInfo {
		public List<Texture2D> texture_List;
		public List<int> texture_id;
	}


    public static string HostName = "10.132.17.71:19785";
    public static string DOMAIN = "http://10.132.17.71:19785";

    //public static string HostName = "110.10.129.41:19785";
    //public static string DOMAIN = "http://110.10.129.41:19785";

    //public static string HostName = "192.168.1.42:19785";
	//public static string DOMAIN = "http://192.168.1.42:19785";

	public static string StatusReturnUrl = "/Account/getStatueAPI.aspx";
	public static string UserInfoUrl = "/Account/setLoginInformation.aspx";
	public static string FileUploadUrl = "/Account/getStatueAPI.aspx";

	//scene names
	public static string splash_scene = "splash";
	public static string meeting_scene = "meeting";
	public static string work_scene = "work";
	public static string picture_scene = "picture";

	//apk id ( 1 - 6 )
	public static int app_id = 6;

	//login user information
	public static string user_name = "";
	public static string user_location = "";
	public static string user_email = "";

	//Experience flag
	public static bool isStartedExperience = false;

	//variables for location lists
	public static int location_country_id = 0;
	public static string[] location_texts;

	//variables for picture texture
	public static string selectedPictureName = "";

	//public static List<Texture2D> gPictureTextures = new List<Texture2D> ();
	public static PictureInfo gPictureTextures;
	public static int WorkSceneLoadFlag = 0;

	public static bool bCamFront = false;

	//variables for Player Pref
	public static string flagPrefText = "saveflag";
	public static string usernamePrefText = "username";
	public static string userlocationPrefText = "userlocation";
	public static string useremailPrefText = "useremail";
	public static string subjectPrefText = "subject";
	public static string contentPrefText = "content";
	public static string summaryPrefText = "summary";
	public static string picturenamePrefText = "picture";
	public static string pictureIdPrefText = "picId";


	public static string m_Title = "";
	public static string m_Content = "";
	public static string m_Summary = "";
}
