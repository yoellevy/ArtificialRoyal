using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Members

    public static GameManager Instance { get; private set; }

    [SerializeField]
    public int playerAmount = 50;
    [SerializeField]
    private int humanPlayerAmount = 0;
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
    private bool useRNN = false;
    // Topology of the agent's FNN, to be set in Unity Editor
    [SerializeField]
    public uint[] FNNTopology;

    // The current population of agents.
    private List<Agent> agents = new List<Agent>();

    /// <summary>
    /// The amount of agents that are currently alive.
    /// </summary>
    public int AgentsAliveCount
    {
        get;
        private set;
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
        return new EvaluationData(AgentsAliveCount, timeRemain, gameTime - timeRemain);
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
        playerAmount = amount;
        //Check arguments
        if (amount < 0) throw new System.Exception("Amount may not be less than zero.");

        if (amount == players.Count) return;

        if (amount > players.Count)
        {
            float hv = verticalSize / 2 - 1;
            float hh = horizontalSize / 2 - 1;
            //Add new players
            for (int toBeAdded = amount - players.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject playerClone = Instantiate(player, transform);
                playerClone.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
                PlayerControllerScriptable controller = Instantiate(aiController);
                PlayerScript playerScript = playerClone.GetComponent<PlayerScript>();
                playerScript.controller = controller;
                playerClone.SetActive(true);
                players.Add(playerScript);
            }
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
        //Observation.Instant.InitMaps();
    }

    public IEnumerator<PlayerScript> GetPlayerEnumerator()
    {
        for (int i = 0; i < players.Count; i++)
            yield return players[i];
    }

    public void Restart()
    {
        float hv = verticalSize / 2 - 1;
        float hh = horizontalSize / 2 - 1;
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        for (int i = bullets.Length - 1; i >= 0; i--)
        {
            Destroy(bullets[i].gameObject);
        }
        foreach (PlayerScript player in players)
        {
            player.Restart();
            player.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        }

        startTime = Time.timeSinceLevelLoad;

        Observation.Instant.InitMaps();
    }


    public void CreateAgents(IEnumerable<Genotype> currentPopulation)
    {
        //Create new agents from currentPopulation
        agents.Clear();
        AgentsAliveCount = 0;

        foreach (Genotype genotype in currentPopulation)
        {
            agents.Add(new Agent(genotype, MathHelper.SoftSignFunction, useRNN, FNNTopology));
        }
    }

    public void RestartTheGame(IEnumerable<Genotype> currentPopulation)
    {
        CreateAgents(currentPopulation);

        //todo - change the implemantation to bettar one :
        SetPlayerAmount(agents.Count);

        IEnumerator<PlayerScript> playersEnum = GameManager.Instance.GetPlayerEnumerator();
        for (int i = 0; i < agents.Count; i++)
        {
            if (!playersEnum.MoveNext())
            {
                Debug.LogError("Players enum ended before agents.");
                break;
            }
            playersEnum.Current.PlayerAgent = agents[i];
            playersEnum.Current.id = i;
            AgentsAliveCount++;
            agents[i].AgentDied += OnAgentDied;
        }
        GameManager.Instance.Restart();
    }

    // Callback for when an agent died.
    private void OnAgentDied(Agent agent)
    {
        AgentsAliveCount--;

        if (AgentsAliveCount == 1 && EvolutionManager.Instance != null)
            EvolutionManager.Instance.EndTheGame();

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

    private void Start()
    {
        if (EvolutionManager.Instance == null) //todo
        {
            //todo!!

            List<Genotype> currentPopulation;
            {
                currentPopulation = new List<Genotype>(playerAmount); //todo
                int weightCount = NeuralNetwork.CalculateOverallWeightCount(GameManager.Instance.FNNTopology);
                for (int i = 0; i < playerAmount; i++)
                {
                    Genotype genotype = new Genotype(new float[weightCount]);
                    genotype.SetRandomParameters(GeneticAlgorithm.DefInitParamMin, GeneticAlgorithm.DefInitParamMax); //todo!! load real genotype and not generate randoms.
                    currentPopulation.Add(genotype);
                }
            }
            RestartTheGame(currentPopulation);
        }

        startTime = Time.timeSinceLevelLoad;
    }

    private void LateUpdate()
    {
        timeRemain = (gameTime - (Time.timeSinceLevelLoad - startTime));
        if (timeRemain <= 0)
        {
            EvolutionManager.Instance.EndTheGame();
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