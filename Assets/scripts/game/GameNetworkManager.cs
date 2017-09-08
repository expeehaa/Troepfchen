using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class GameNetworkManager : NetworkManager {

    public GameEscapeMenuScript gems;
    public GameObject MenuPanel;
    public ArenaBoundaries ArenaBoundaries;
    public Scrollbar SelectionScrollbar;

    public GameObject NucleiNode;
    public GameObject BotNode;
    public GameObject PlayerNode;
    public Vector3 PlayerSpawn = new Vector3(-300, 100, 0);

    public Material MatNeutron;
    public Material MatProton;
    public Material MatElectron;

    public InputField InputServername;
    public InputField InputNickname;
    public InputField InputMaxConns;
    public Button BtnHost;
    public Button BtnJoin;

    public string password;

    private List<GameObject> nucleonNodes = new List<GameObject>();

    private Dictionary<PlayerController, int> playerKills = new Dictionary<PlayerController, int>();

    void Awake()
    {
        Data.MatNeutron = MatNeutron;
        Data.MatProton = MatProton;
        Data.MatElectron = MatElectron;
    }

    void Start()
    {
        Data.Paused = gems.escaped;
        gems.Callbacks.Add(onPauseEvent);
        BtnHost.onClick.AddListener(createHost);
        BtnJoin.onClick.AddListener(joinServer);
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked && !gems.escaped && !MenuPanel.activeSelf) Cursor.lockState = CursorLockMode.Locked;
        else if (Cursor.lockState != CursorLockMode.None && gems.escaped && !MenuPanel.activeSelf) Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        foreach (var node in nucleonNodes)
        {
            Destroy(node);
        }
        MenuPanel.SetActive(true);
    }

    private void onPauseEvent(bool value)
    {
        Data.Paused = value;
    }

    private void createHost()
    {
        var servername = InputServername.text;
        var nickname = InputNickname.text;
        var maxConns = InputMaxConns.text == null || InputMaxConns.text.Equals(string.Empty) ? 0 : uint.Parse(InputMaxConns.text);
        StartMatchMaker();
        matchMaker.CreateMatch(servername, maxConns, false, password, "", "", 0, 0, OnMatchCreate);
    }

    private void joinServer()
    {
        var servername = InputServername.text;
        var nickname = InputNickname.text;
        if (matchMaker == null) StartMatchMaker();
        matchMaker.ListMatches(0, 10, servername, true, 0, 0, OnMatchList);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        MenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        MenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        base.OnClientDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        var playernode = Instantiate(PlayerNode);
        playernode.AddComponent<NetworkIdentity>();
        playernode.GetComponent<NucleonNode>().GameAtomData = new GameAtomData(26, 30, 0.1f, 0.4f, 0.1f, 1, 0, 0.2f, 0.1f);
        playernode.GetComponent<NucleonNode>().ArenaBoundaries = ArenaBoundaries;
        playernode.GetComponent<PlayerNode>().ArenaBoundaries = ArenaBoundaries;
        playernode.GetComponent<PlayerNode>().SelectionScrollbar = SelectionScrollbar;
        playernode.transform.SetParent(NucleiNode.transform);
        playernode.transform.localPosition = PlayerSpawn;
        NetworkServer.AddPlayerForConnection(conn, playernode, playerControllerId);
        playernode.GetComponent<NucleonNode>().spawnNukleons(playernode.GetComponent<NucleonNode>().GameAtomData);
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            StartHost(matchInfo);
            MenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            InputServername.GetComponent<Image>().color = Color.white;
            playerKills.Clear();
        }
        else
        {
            InputServername.GetComponent<Image>().color = new Color(255 / 255, 120 / 255, 120 / 255);
        }
        base.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
    }

    public void OnMatchListJoinMatch(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        if (success)
        {
            var match = matchList.FirstOrDefault();
            if (match == null) return;
            matchMaker.JoinMatch(match.networkId, password, "", "", 0, 0, OnMatchCreate);
        }
    }


}
