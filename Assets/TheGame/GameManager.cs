using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField]
    private Text timeText;
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

    #region AgentsMembers
    [SerializeField]
    public bool useRNN = false;
    // Topology of the agent's FNN, to be set in Unity Editor
    [SerializeField]
    public uint[] FNNTopology;

    // The current population of agents.
    private List<Agent> agents = new List<Agent>();

    private List<Agent> agents_group_B = new List<Agent>();

    private List<Genotype> gamePopulation;

    /// <summary>
    /// The amount of agents that are currently alive.
    /// </summary>
    public int PlayersAliveCount
    {
        get;
        set;
    }
    #endregion

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

    //public void SetPlayerAmount(int amount)
    //{
    //    //Check arguments
    //    if (amount < 0) throw new System.Exception("Amount may not be less than zero.");

    //    if (amount == players.Count) return;

    //    if (amount > players.Count)
    //    {
    //        CreateAIPlayers(amount, 0);
    //    }
    //    else if (amount < players.Count)
    //    {
    //        //Remove existing players
    //        for (int toBeRemoved = players.Count - amount; toBeRemoved > 0; toBeRemoved--)
    //        {
    //            PlayerScript last = players[players.Count - 1];
    //            players.RemoveAt(players.Count - 1);

    //            Destroy(last.gameObject);
    //        }
    //    }
    //}

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


    public void CreateAgents(List<Agent> agents, IEnumerable<Genotype> population)
    {
        //Create new agents from currentPopulation
        agents.Clear();

        foreach (Genotype genotype in population)
        {
            agents.Add(new Agent(genotype, MathHelper.SoftSignFunction, useRNN, FNNTopology));
        }
    }

    public void CreatePlayers(int amount, int group_B_amount)
    {
        players.Clear();

        CreateAIPlayers(amount, CompareBattleManager.GroupName.A);
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
        }        
    }

    private void CreateHumanPlayer()
    {
        CreatePlayer(humenController);
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

    private void AssignAgents(List<Agent> agents, int id_start = 0)
    {
        for (int i = 0; i < agents.Count; i++)
        {
            PlayerScript playerScript = players[i + id_start];
            playerScript.PlayerAgent = agents[i];
            playerScript.id = i + id_start;
            agents[i].AgentDied += OnAgentDied;
        }
    }

    public void RestartTheGame(IEnumerable<Genotype> currentPopulation, IEnumerable<Genotype> compare_battle_group_B_Population = null)
    {
        int group_B_player_count = 0;
        CreateAgents(agents, currentPopulation);
        if (compare_battle_group_B_Population != null)
        {
            CreateAgents(agents_group_B, compare_battle_group_B_Population);
            group_B_player_count = agents_group_B.Count;
        }
        if (players.Count == 0)
            CreatePlayers(agents.Count, group_B_player_count);

        AssignAgents(agents);
        if (compare_battle_group_B_Population != null)
            AssignAgents(agents_group_B, agents.Count);

        RestartPlayers();

        RemoveBullets();

        UpdatePlayerAliveGUI();

        startTime = Time.timeSinceLevelLoad;

        SetCurrentBoardSize(initialBoardSize);

        Observation.Instant.InitMaps();
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
        }
        else
        {
            //todo - add regular game ending.
            messageText.text = "Game Over";
            timeText.text = "0";
            yield return new WaitForSeconds(5);
            RestartTheGame(gamePopulation);
        }
    }


    private void Awake()
    {
        if (Instance != null)
        {
            // TODO check why :( DO NOT remove from scene.
            Debug.LogError("More than one GameManager in the Scene.");
            return;
        }
        Instance = this;
    }

    private void CreatGamePopulation()
    {
        gamePopulation = new List<Genotype>(playerAmount);
        if (GameData.instance.genotypes.Count == 0)
        {
            int weightCount = NeuralNetwork.CalculateOverallWeightCount(useRNN, GameManager.Instance.FNNTopology);
            for (int i = 0; i < playerAmount; i++)
            {
                Genotype genotype = new Genotype(new float[weightCount]);
                genotype.SetRandomParameters(GeneticAlgorithm.DefInitParamMin, GeneticAlgorithm.DefInitParamMax); //todo!! load real genotype and not generate randoms.
                gamePopulation.Add(genotype);
            }
        }
        else
        {
            int i = 0;
            while (gamePopulation.Count < playerAmount)
            {
                gamePopulation.Add(GameData.instance.genotypes[i]);
                i = (i + 1) % GameData.instance.genotypes.Count;
            }
        }
    }

    private void Start()
    {
        if (EvolutionManager.Instance == null && CompareBattleManager.Instance == null)
        {
            CreatGamePopulation();
            RestartTheGame(gamePopulation);
        }
    }

    private void LateUpdate()
    {
        timeRemain = (gameTime - (Time.timeSinceLevelLoad - startTime));
        if (/*timeRemain <= 0 || */PlayersAliveCount <= 1)
        {
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


    private void OnGUI()
    {
        timeText.text = ((int) (timeRemain)).ToString();
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