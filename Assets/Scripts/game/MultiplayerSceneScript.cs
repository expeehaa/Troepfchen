using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerSceneScript : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(onClick);
    }

    void onClick()
    {
        SceneManager.LoadScene("multiplayergame");
    }
}
