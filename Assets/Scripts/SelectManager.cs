using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectManager : MonoBehaviour {
	public int selected_country_id;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CountrySelected()
	{
		Global.location_country_id = selected_country_id;
		GameObject.Find ("UI Root/LoginWnd/LocationInput/LocationList").transform.gameObject.SetActive (false);
		GameObject.Find ("UI Root/LoginWnd/LocationInput/txtLocation").transform.FindChild ("Label").transform.GetComponent<UILabel> ().text = Global.location_texts[Global.location_country_id];
		GameObject.Find ("UI Root/LoginWnd/LocationInput/txtLocation").transform.FindChild ("Label").transform.GetComponent<UILabel> ().color = new Color (70/255, 70/255, 70/255);
	}
}
