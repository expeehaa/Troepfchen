using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEscapeMenuScript : MonoBehaviour {

    public Button btnBack;
    public Button btnMenu;
    public Button btnMainMenu;
    public GameObject panel;
    public Game game;

    public bool StartEscaped = false;

    private bool _escaped;
    public bool escaped
    {
        get
        {
            return _escaped;
        }
        set
        {
            if (_escaped != value) executeCallbacks(value);
            _escaped = value;
        }
    }

    private List<UnityAction<bool>> callbacks = new List<UnityAction<bool>>();
    public List<UnityAction<bool>> Callbacks
    {
        get
        {
            return callbacks;
        }
    }

    void Awake()
    {
        _escaped = StartEscaped;
    }

    // Use this for initialization
    void Start()
    {
        btnBack.onClick.AddListener(OnBtnBack);
        btnMenu.onClick.AddListener(onBtnMenu);
        btnMainMenu.onClick.AddListener(onBtnMainMenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) escaped = !escaped;
        if (panel.activeSelf != escaped) panel.SetActive(escaped);
    }

    void OnBtnBack()
    {
        escaped = false;
    }

    void onBtnMenu()
    {
        escaped = false;
        game.Restart();
    }

    void onBtnMainMenu()
    {
        SceneManager.LoadScene("mainmenu");
    }

    private void executeCallbacks(bool value)
    {
        foreach (var cb in callbacks)
        {
            cb.Invoke(value);
        }
    }
}
