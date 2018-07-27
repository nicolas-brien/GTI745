using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour {
    public GameObject defaultButton;

	// Use this for initialization
	void Start () {
        EventSystem.current.SetSelectedGameObject(defaultButton);
	}

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
}
