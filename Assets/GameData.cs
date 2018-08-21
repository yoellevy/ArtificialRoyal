using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour {

    public static GameData instance = null;

    public const string SAVE_DATA_DIRECTORY = "saves";

    public const string AGENTS_FOLDER_NAME = "agents";
    public const string AGENT_SUFFIX = "agent";

    public const string COMPARE_BATTLE_GROUPS_FOLDER_NAME = "CompareBattle";
    public const string GROUP_A_FOLDER_NAME = "Group_A";
    public const string GROUP_B_FOLDER_NAME = "Group_B";


    [HideInInspector]
    public bool toAddHumanPlayer = false;
    [HideInInspector]
    public List<Agent> agents = new List<Agent>();
    [HideInInspector]
    public List<Agent> agents_group_B = new List<Agent>();

    #region EvolutionData
    [SerializeField]
    public bool useRNN = true;
    [SerializeField]
    public uint[] NNTopology = new uint[5] {21, 27, 16, 8, 4};
    [SerializeField]
    public NeuralLayer.ActivationFunctionType activationFunctionType = NeuralLayer.ActivationFunctionType.SoftSign;

    [Range(2, 100)] public int SelectNBest = 5;
    [Range(0, 100)] public int SelectMRandom = 1;
    [Range(2, 100)] public int BreadAmountToSaveFromSelection = 2;
    [Range(0, 1)] public float SwapProb = 0.6f;
    [Range(0, 100)] public int MutationSaveAmount = 2;
    [Range(0, 1)] public float MutationPerc = 1f;
    [Range(0, 1)] public float MutationProb = 0.3f;
    public int MutationAmount = 2;

    public int EvalRank = 1;
    public int EvalKills = 1;

    public int NumberOfGamesPerGeneration = 2;

    [HideInInspector]
    public bool isNewAgents = false;
    #endregion

    public int CompareBattleGamesToPlay = 50;



    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    

    public void LoadAgents(string agents_folder, out List<Agent> agents)
    {
        agents = new List<Agent>();

        if (!Directory.Exists(agents_folder)) throw new DirectoryNotFoundException(string.Format("Can't find {0} Directory", agents_folder));

        string[] agent_files = Directory.GetFiles(agents_folder, "*." + AGENT_SUFFIX);

        if (agent_files.Length == 0) throw new FileNotFoundException(string.Format("Can't find files with suffix \"{0}\"", AGENT_SUFFIX));

        foreach (string file in agent_files)
        {
            agents.Add(Agent.LoadFromFile(file));
        }

        activationFunctionType = NeuralLayer.GetActivationFunctionType(agents[0].FNN.Layers[0].NeuronActivationFunction);
        useRNN = agents[0].FNN.useRNN;
        NNTopology = agents[0].FNN.Topology;
        isNewAgents = false;
    }


    public void BackToMainMenu()
    {
        agents = null;
        agents_group_B = null;
        toAddHumanPlayer = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void CreateAgents(out List<Agent> agents, IEnumerable<Genotype> population)
    {
        //Create new agents from currentPopulation
        agents = new List<Agent>();
        NeuralLayer.ActivationFunction af = NeuralLayer.GetActivitionFunction(activationFunctionType);

        foreach (Genotype genotype in population)
        {
            agents.Add(new Agent(genotype, af, useRNN, NNTopology));
        }
        isNewAgents = true;
    }
}
