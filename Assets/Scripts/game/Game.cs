using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Game : DelayedActionScript {
    
    public GameEscapeMenuScript gems;
    public Button BtnStart1;
    public Dropdown Dropdown;
    public GameObject MenuPanel;
    public ArenaBoundaries ArenaBoundaries;
    public Scrollbar SelectionScrollbar;

    public GameObject NucleiNode;
    public GameObject BotNode;
    public GameObject PlayerNode;
    public Vector3 PlayerSpawn = new Vector3(-300, 100, 0);
    public Vector2 AISpawn = new Vector2(300, 100);

    public Material MatNeutron;
    public Material MatProton;
    public Material MatElectron;

    private List<GameObject> nucleonNodes = new List<GameObject>();
    public int Kills = 0;

    void Awake()
    {
        Data.MatNeutron = MatNeutron;
        Data.MatProton = MatProton;
        Data.MatElectron = MatElectron;
    }

    void Start () {
        Data.Paused = gems.escaped;
        gems.Callbacks.Add(onPauseEvent);
        BtnStart1.onClick.AddListener(onStart1);
    }
    
    // Update is called once per frame
    void Update () {
        if (Cursor.lockState != CursorLockMode.Locked && !gems.escaped && !MenuPanel.activeSelf) Cursor.lockState = CursorLockMode.Locked;
        else if (Cursor.lockState != CursorLockMode.None && gems.escaped && !MenuPanel.activeSelf) Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        foreach (var node in nucleonNodes)
        {
            Destroy(node);
        }
        nucleonNodes.Clear();
        MenuPanel.SetActive(true);
    }
    
    private void onPauseEvent(bool value){
        Data.Paused = value;
    }

    private void onStart1(){
        var selected = Dropdown.options[Dropdown.value].text;
        if (selected.Equals("Easy"))
        {
            Kills = 0;
            spawnPlayer();
            spawnBot(AISpawn, new GameAtomData(60, 176, 0.9f, 1.1f, 0.7f, 2.5f, 1.4f, 1.5f, 1.5f));
            MenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (selected.Equals("Default"))
        {
            Kills = 0;
            spawnPlayer();
            var botCount = 2;
            for (int i = 0; i < botCount; i++)
            {
                spawnBot(getBotPosition(i+1, botCount), new GameAtomData(118, 176, 0.9f, 1.1f, 0.7f, 2.5f, 1.4f, 1.5f, 1.5f));
            }
            MenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (selected.Equals("Hard"))
        {
            Kills = 0;
            spawnPlayer();
            var botCount = 4;
            for (int i = 0; i < botCount; i++)
            {
                spawnBot(getBotPosition(i + 1, botCount), new GameAtomData(150, 176, 0.9f, 1.1f, 0.7f, 2.5f, 1.4f, 1.5f, 1.5f));
            }
            MenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void spawnPlayer()
    {
        var playernode = Instantiate(PlayerNode);
        playernode.GetComponent<NucleonNode>().GameAtomData = new GameAtomData(26, 30, 0, 0.4f, 0.1f, 1, 0, 0.2f, 0.1f);
        playernode.GetComponent<NucleonNode>().ArenaBoundaries = ArenaBoundaries;
        playernode.GetComponent<NucleonNode>().OnDestruction.Add(OnNuclideDestruction);
        playernode.GetComponent<PlayerNode>().ArenaBoundaries = ArenaBoundaries;
        playernode.GetComponent<PlayerNode>().SelectionScrollbar = SelectionScrollbar;
        playernode.transform.SetParent(NucleiNode.transform);
        playernode.transform.localPosition = PlayerSpawn;
        playernode.GetComponent<NucleonNode>().spawnNukleons(playernode.GetComponent<NucleonNode>().GameAtomData);
        nucleonNodes.Add(playernode);
    }

    private void spawnBot(Vector3 pos, GameAtomData gad)
    {
        var botnode = Instantiate(BotNode);
        var nn = botnode.GetComponent<NucleonNode>();
        var bn = botnode.GetComponent<BotNode>();
        nn.GameAtomData = gad.Copy();
        nn.ArenaBoundaries = ArenaBoundaries;
        nn.OnDestruction.Add(OnNuclideDestruction);
        bn.BaseGameAtomData = gad.Copy();
        bn.SpawnPosition = pos;
        botnode.transform.SetParent(NucleiNode.transform);
        botnode.transform.localPosition = pos;
        nn.spawnNukleons(botnode.GetComponent<NucleonNode>().GameAtomData);
        nucleonNodes.Add(botnode);
    }

    private void OnNuclideDestruction(GameObject nuclide, bool player)
    {
        print(nuclide.GetComponent<NucleonNode>().GameAtomData.ToString() + " | " + player);
        if (player)
        {
            Restart();
        }
        else
        {
            Kills++;
            var deathTime = 4;
            var spawnPos = nuclide.GetComponent<BotNode>().SpawnPosition;
            var gad = nuclide.GetComponent<BotNode>().BaseGameAtomData.Copy();
            nuclide.GetComponent<NucleonNode>().CreateDeathScene(deathTime);
            InvokeLater(() =>
            {
                Destroy(nuclide);
            }, deathTime);
            InvokeLater(() =>
            {
                spawnBot(spawnPos, gad);
            }, 10);
        }
    }

    private Vector3 getBotPosition(int botNumber, int totalBots)
    {
        if (botNumber < 1 || botNumber > totalBots) return Vector3.zero;
        var angle = (180f * botNumber) / (totalBots + 1f);
        var pos = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
        pos.Normalize();
        pos *= AISpawn.x;
        pos.y = AISpawn.y;
        return pos;
    }
}
