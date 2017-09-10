using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(onClick);
	}
	
    void onClick()
    {
        SceneManager.LoadScene("game");
    }
}
