using System.Collections;
using System.Collections.Generic;
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
    private GameObject player;
    [SerializeField]
    private PlayerControllerScriptable aiController;
    [SerializeField]
    private PlayerControllerScriptable humenController;

    [SerializeField]
    private int gameTime = 2 * 60;
    private float startTime;
    [SerializeField]
    private Text timeText;
    private float timeRemain;
    [SerializeField]
    private Text messageText;

    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private float horizontalSize = 160;
    [SerializeField]
    private float verticalSize = 90;

    public  GameObject _northWall { get; private set; }
    public GameObject _eastWall { get; private set; }
    public GameObject _southWall { get; private set; }
    public GameObject _westWall { get; private set; }

    private GameObject border;
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

    private void CreateBorder()
    {
        border = new GameObject("border");
        float hv = verticalSize / 2;
        float hh = horizontalSize / 2;

        _northWall = GameObject.Instantiate(wall, new Vector3(0, hv, 0), wall.transform.rotation, border.transform);
        _northWall.transform.localScale = new Vector3(_northWall.transform.localScale.x * horizontalSize,
            _northWall.transform.localScale.y, _northWall.transform.localScale.z);

        _eastWall = GameObject.Instantiate(wall, new Vector3(-hh, 0, 0), wall.transform.rotation, border.transform);
        _eastWall.transform.localScale = new Vector3(_eastWall.transform.localScale.x,
            _eastWall.transform.localScale.y * verticalSize, _eastWall.transform.localScale.z);

        _southWall = GameObject.Instantiate(wall, new Vector3(0, -hv, 0), wall.transform.rotation, border.transform);
        _southWall.transform.localScale = new Vector3(_southWall.transform.localScale.x * horizontalSize,
            _southWall.transform.localScale.y, _southWall.transform.localScale.z);

        _westWall = GameObject.Instantiate(wall, new Vector3(hh, 0, 0), wall.transform.rotation, border.transform);
        _westWall.transform.localScale = new Vector3(_westWall.transform.localScale.x,
            _westWall.transform.localScale.y * verticalSize, _westWall.transform.localScale.z);
    }

    public void SetPlayerAmount(int amount)
    {
        //Check arguments
        if (amount < 0) throw new System.Exception("Amount may not be less than zero.");

        if (amount == players.Count) return;

        if (amount > players.Count)
        {
            CreateAIPlayers(amount);
        }
        else if (amount < players.Count)
        {
            //Remove existing players
            for (int toBeRemoved = players.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                PlayerScript last = players[players.Count - 1];
                players.RemoveAt(players.Count - 1);

                Destroy(last.gameObject);
            }
        }
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
        float hv = verticalSize / 2 - 1;
        float hh = horizontalSize / 2 - 1;
        
        foreach (PlayerScript player in players)
        {
            player.Restart();
            player.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        }

        PlayersAliveCount = players.Count;
    }


    public void CreateAgents(IEnumerable<Genotype> currentPopulation)
    {
        //Create new agents from currentPopulation
        agents.Clear();

        foreach (Genotype genotype in currentPopulation)
        {
            agents.Add(new Agent(genotype, MathHelper.SoftSignFunction, useRNN, FNNTopology));
        }
    }

    public void CreatePlayers(IEnumerable<Genotype> currentPopulation)
    {
        CreateAgents(currentPopulation);

        players.Clear();

        CreateAIPlayers(agents.Count);
        if (GameData.instance.toAddHumanPlayer)
        {
            CreateHumanPlayer();
        }
    }

    private void CreateAIPlayers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            PlayerScript playerScript = CreatePlayer(aiController);
            playerScript.PlayerAgent = agents[i];
            playerScript.id = i;
            agents[i].AgentDied += OnAgentDied;
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

    public void RestartTheGame(IEnumerable<Genotype> currentPopulation)
    {
        if (players.Count == 0)
            CreatePlayers(currentPopulation);

        RestartPlayers();

        RemoveBullets();

        startTime = Time.timeSinceLevelLoad;

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

        CreateBorder();
        
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
        if (EvolutionManager.Instance == null)
        {
            CreatGamePopulation();
            RestartTheGame(gamePopulation);
        }
    }

    private void LateUpdate()
    {
        timeRemain = (gameTime - (Time.timeSinceLevelLoad - startTime));
        if (timeRemain <= 0 )//|| PlayersAliveCount == 1)
        {
            StartCoroutine(EndGame());
        }
    }

    public void EvalAlives()
    {
        foreach (PlayerScript player in players)
        {
            if (player.isAlive)
            {
                player.EvalSelf();
            }
        }
    }

    private void OnGUI()
    {
        timeText.text = ((int) (timeRemain)).ToString();
    }
}