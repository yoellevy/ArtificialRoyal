using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameManager : MonoBehaviour
{
    #region Members

    public static GameManager Instance { get; private set; }

    [SerializeField]
    public int playerAmount = 50;
    [SerializeField]
    public int randomPlayerAmount = 5;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private PlayerControllerScriptable aiController;
    [SerializeField]
    private PlayerControllerScriptable humenController;
    [SerializeField]
    private PlayerControllerScriptable randomController;

    [SerializeField]
    private int gameTime = 2 * 60;
    private float startTime;
    private float timeRemain;
    [SerializeField]
    private Text messageText;

    [SerializeField]
    private Text AliveText;


    [SerializeField]
    private GameObject ground;

    [SerializeField]
    private Vector2 initialBoardSize = new Vector2(160, 90);


    [HideInInspector]
    public List<BulletData> bullets = new List<BulletData>();
    [HideInInspector]
    public List<PlayerScript> players = new List<PlayerScript>();

    /// <summary>
    /// The amount of agents that are currently alive.
    /// </summary>
    public int PlayersAliveCount
    {
        get;
        set;
    }

    private bool inEndGame = false;
    #endregion

    public struct EvaluationData
    {
        public int rankInGame;
        public float timeRemain;
        public float timeSurvived;

        public EvaluationData(int rankInGame, float timeRemain, float timeSurvived)
        {
            this.rankInGame = rankInGame;
            this.timeRemain = timeRemain;
            this.timeSurvived = timeSurvived;
        }
    }

    public EvaluationData GetPlayerEvaluationData()
    {
        return new EvaluationData(PlayersAliveCount, timeRemain, gameTime - timeRemain);
    }

    public void RemovePlayer(PlayerScript player)
    {
        players.Remove(player);
    }

    public IEnumerator<PlayerScript> GetPlayerEnumerator()
    {
        for (int i = 0; i < players.Count; i++)
            yield return players[i];
    }

    private void RemoveBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        for (int i = bullets.Length - 1; i >= 0; i--)
        {
            Destroy(bullets[i].gameObject);
        }
    }

    public void RestartPlayers()
    {
        float hv = initialBoardSize.y / 2 - 1;
        float hh = initialBoardSize.x / 2 - 1;
        
        foreach (PlayerScript player in players)
        {
            player.Restart();
            player.transform.position = new Vector3(UnityEngine.Random.Range(-hh, hh), UnityEngine.Random.Range(-hv, hv));
        }

        PlayersAliveCount = players.Count;
    }


    public void CreatePlayers(int amount, int group_B_amount)
    {
        players.Clear();

        CreateAIPlayers(amount, CompareBattleManager.GroupName.A);
        if (CompareBattleManager.Instance != null)
            CreateAIPlayers(group_B_amount, CompareBattleManager.GroupName.B);
        if (GameData.instance.toAddHumanPlayer)
        {
            CreateHumanPlayer();
        }

        if (EvolutionManager.Instance != null)
        {// we are in training
            CreateRandomPlayers();
            //add random player
        }
    }

    private void CreateRandomPlayers()
    {
        for (int i = 0; i < randomPlayerAmount; i++)
        {
            PlayerScript playerScript= CreatePlayer(randomController);
            playerScript.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void CreateAIPlayers(int amount, CompareBattleManager.GroupName group)
    {
        for (int i = 0; i < amount; i++)
        {
            PlayerScript playerScript = CreatePlayer(aiController);
            playerScript.group = group;
            if (playerScript.group == CompareBattleManager.GroupName.B)
            {
                playerScript.GetComponent<SpriteRenderer>().color = Color.cyan;
            }
        }        
    }

    private void CreateHumanPlayer()
    {
        PlayerScript playerScript = CreatePlayer(humenController);
        playerScript.GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    private PlayerScript CreatePlayer(PlayerControllerScriptable controller)
    {
        GameObject playerClone = Instantiate(player, transform);
        PlayerScript playerScript = playerClone.GetComponent<PlayerScript>();
        playerScript.controller = Instantiate(controller);
        playerClone.SetActive(true);
        players.Add(playerScript);
        return playerScript;
    }

    private void AssignAgents(IEnumerable<Agent> agents, int id_start = 0)
    {
        foreach (Agent agent in agents)
        {
            PlayerScript playerScript = players[id_start];
            playerScript.PlayerAgent = agent;
            playerScript.id = id_start;
            playerScript.PlayerAgent.AgentDied += OnAgentDied;
            id_start++;
        }
    }

    private void ChangeAgentsAmountToPlayerAmount()
    {
        if (CompareBattleManager.Instance != null)
        {
            //Compare Battle Game

            //we have just 2 groups and no human player
            int amount = playerAmount / 2;

            ChangeAgentsAmountToPlayerAmountHelper(GameData.instance.agents, amount);
            ChangeAgentsAmountToPlayerAmountHelper(GameData.instance.agents_group_B, amount);
        }
        else if (EvolutionManager.Instance == null)
        {
            //regular game
            ChangeAgentsAmountToPlayerAmountHelper(GameData.instance.agents, playerAmount);
            GameData.instance.agents_group_B.Clear();
        }
        //other cases should be OK.

    }

    private void ChangeAgentsAmountToPlayerAmountHelper(List<Agent> agents, int amount)
    {
        //Check arguments
        if (amount < 0) throw new ArgumentException("Amount may not be less than zero.");

        if (amount == agents.Count) return;

        if (amount > agents.Count)
        {
            for (int i = 0; agents.Count < amount; i++)
            {
                agents.Add(agents[i]);
            }
        }
        else if (amount < agents.Count)
        {
            //Remove existing agents
            for (int toBeRemoved = agents.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                agents.RemoveAt(agents.Count - 1);
            }
        }
    }
    

    public void RestartTheGame()
    {
        if (players.Count == 0)
        {
            ChangeAgentsAmountToPlayerAmount();
            CreatePlayers(GameData.instance.agents.Count, GameData.instance.agents_group_B.Count);
        }

        AssignAgents(GameData.instance.agents);
        if (CompareBattleManager.Instance != null && GameData.instance.agents_group_B != null)
            AssignAgents(GameData.instance.agents_group_B, GameData.instance.agents.Count);

        RestartPlayers();

        RemoveBullets();

        UpdatePlayerAliveGUI();

        startTime = Time.timeSinceLevelLoad;

        SetCurrentBoardSize(initialBoardSize);

        Observation.Instant.InitMaps();

        inEndGame = false;
    }

    // Callback for when an agent died.
    private void OnAgentDied(Agent agent)
    {
        if (PlayersAliveCount == 1 && EvolutionManager.Instance != null)
            EvolutionManager.Instance.EndTheGame();

    }

    private IEnumerator EndGame()
    {
        if (EvolutionManager.Instance != null)
        {
            EvolutionManager.Instance.EndTheGame();
        }
        else if (CompareBattleManager.Instance != null)
        {
            CompareBattleManager.Instance.EndCompareGame();
            RestartTheGame();
        }
        else
        {
            //todo - add regular game ending.
            messageText.text = "Game Over";
            yield return new WaitForSeconds(5);
            messageText.text = "";
            RestartTheGame();
        }
        
    }


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager in the Scene.");
            return;
        }
        Instance = this;
    }


    private void Start()
    {
        if (EvolutionManager.Instance == null && CompareBattleManager.Instance == null)
        {
            RestartTheGame();
        }
    }

    private void LateUpdate()
    {
        timeRemain = (gameTime - (Time.timeSinceLevelLoad - startTime));
        if (PlayersAliveCount <= 1 && !inEndGame)
        {
            inEndGame = true;
            StartCoroutine(EndGame());
        }
    }

    public void EvalOfEndGame()
    {
        int playersThatDiedAmount = (playerAmount - PlayersAliveCount);
        var maxKills = GetMaxKills();
        foreach (PlayerScript player in players)
        {
            if (player.PlayerAgent == null)
            {
                continue;
            }
            if (player.isAlive)
            {
                player.UpdatePlayerData();
            }
            player.NormalizePlayerData(gameTime, PlayersAliveCount, playersThatDiedAmount,maxKills);
            player.EvalSelf();
        }
    }

    private float GetMaxKills()
    {
        return players.Max((script => script.KillCount));
    }


    private void SetCurrentBoardSize(Vector2 size)
    {
        ground.transform.localScale = size;
    }

    public Vector2 getCurrentBoardSize()
    {
        return ground.transform.localScale;
    }

    public void UpdatePlayerAliveGUI()
    {
        AliveText.text = ((int)(PlayersAliveCount)).ToString();
    }
}