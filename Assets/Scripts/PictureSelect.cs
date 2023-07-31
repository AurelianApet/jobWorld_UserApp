using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureSelect : MonoBehaviour {
	public int selected_picture_id = -1;
	public GameObject highLight;

	// Use this for initialization
	void Start () {
		highLight.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onSelectPicture()
	{
		if (selected_picture_id > -1) {
			Global.selectedPictureName = selected_picture_id.ToString () + ".png";
			StartCoroutine (PictureSelectRoutine ());
		}
	}

	IEnumerator PictureSelectRoutine()
	{
		highLight.SetActive (true);

		yield return new WaitForSeconds (1.0f);
		highLight.SetActive (false);
		GameObject.Find ("WritingManage").transform.GetComponent<WriteController> ().InitFields ();
	}
}
