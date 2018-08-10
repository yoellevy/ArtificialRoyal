/// Author: Samuel Arzt
/// Date: March 2017


#region Includes
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text;
#endregion

/// <summary>
/// Singleton class for managing the evolutionary processes.
/// </summary>
public class EvolutionManager : MonoBehaviour
{
    #region Members
    private static System.Random randomizer = new System.Random();

    public static EvolutionManager Instance
    {
        get;
        private set;
    }

    // Whether or not the results of each generation shall be written to file, to be set in Unity Editor
    [SerializeField]
    private bool SaveStatistics = false;
    private string statisticsFileName;

    // How many of the first to finish the course should be saved to file, to be set in Unity Editor
    [SerializeField]
    private uint SaveFirstNGenotype = 0;
    private uint genotypesSaved = 0;
    [SerializeField]
    private uint SaveFirstNAgents = 3;


    private GeneticAlgorithm geneticAlgorithm;

    /// <summary>
    /// The age of the current generation.
    /// </summary>
    public uint GenerationCount
    {
        get { return geneticAlgorithm.GenerationCount; }
    }

    [SerializeField]
    float timeScale = 1;

    [SerializeField]
    private Text _generationNumber;

    private bool _toSaveGenotypes = false;

    [SerializeField]
    public bool useRNN = false;
    // Topology of the agent's FNN, to be set in Unity Editor
    [SerializeField]
    public uint[] NNTopology;
    #endregion

    #region Constructors
    void Awake()
    {
        if (Instance != null)
        {
            // TODO check why...
            Debug.LogError("More than one EvolutionManager in the Scene.");
            return;
        }
        Instance = this;

        ////Load gui scene
        //SceneManager.LoadScene("GUI", LoadSceneMode.Additive);

        //Load Game
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);
    }

    private void Start()
    {
        _generationNumber.text = "";
        StartEvolution();
    }
    #endregion

    #region Methods

    private void Update()
    {
        Time.timeScale = timeScale;
    }

    /// <summary>
    /// Event for when all agents have died or the game ended.
    /// </summary>
    public event System.Action EndOfGame;
    
   

    /// <summary>
    /// Starts the evolutionary process.
    /// </summary>
    public void StartEvolution()
    {
        int weightCount = NeuralNetwork.CalculateOverallWeightCount(useRNN, NNTopology);

        //Setup genetic algorithm
        geneticAlgorithm = new GeneticAlgorithm((uint) weightCount, (uint) (GameManager.Instance.playerAmount- GameManager.Instance.randomPlayerAmount));
        genotypesSaved = 0;

        geneticAlgorithm.Evaluation = GameManager.Instance.RestartTheGame;

        geneticAlgorithm.Selection = GeneticAlgorithm.DefaultSelectionOperator;
        geneticAlgorithm.Recombination = RandomRecombination;
        geneticAlgorithm.Mutation = MutateAllButBestTwo;
        

        EndOfGame += GameManager.Instance.EvalOfEndGame;
        EndOfGame += geneticAlgorithm.EvaluationFinished;

        //Statistics
        if (SaveStatistics)
        {
            //TODO: adding log?
            statisticsFileName = "Evaluation - " + DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");
            WriteStatisticsFileStart();
            //WriteNNData();
            geneticAlgorithm.FitnessCalculationFinished += WriteStatisticsToFile;
            //geneticAlgorithm.FitnessCalculationFinished += CheckForTrackFinished;
            //geneticAlgorithm.FitnessCalculationFinished += SaveBestGenotypes;
        }
        



        geneticAlgorithm.Start();
    }

    // Writes the starting line to the statistics file, stating all genetic algorithm parameters.
    private void WriteStatisticsFileStart()
    {
        string saveFolder = GameData.SAVE_DATA_DIRECTORY + "/";

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        File.WriteAllText(saveFolder + statisticsFileName + ".txt", "Evaluation of a Population with size " + GameManager.Instance.playerAmount +
                ", using the following GA operators: " + Environment.NewLine +
                "Selection: " + geneticAlgorithm.Selection.Method.Name + Environment.NewLine +
                "Recombination: " + geneticAlgorithm.Recombination.Method.Name + Environment.NewLine +
                "Mutation: " + geneticAlgorithm.Mutation.Method.Name + Environment.NewLine +
                "FitnessCalculation: " + geneticAlgorithm.FitnessCalculationMethod.Method.Name + Environment.NewLine + Environment.NewLine);
    }

    // Appends the current generation count and the evaluation of the best genotype to the statistics file.
    private void WriteStatisticsToFile(IEnumerable<Genotype> currentPopulation)
    {
        string saveFolder = GameData.SAVE_DATA_DIRECTORY + "/";

        foreach (Genotype genotype in currentPopulation)
        {
            File.AppendAllText(saveFolder + statisticsFileName + ".txt", geneticAlgorithm.GenerationCount + "\t" + genotype.Evaluation + Environment.NewLine);
            break; //Only write first
        }

        SaveBestAgents(SaveFirstNAgents);
    }

    // Checks the current population and saves genotypes to a file if their evaluation is greater than or equal to 1
//    private void CheckForTrackFinished(IEnumerable<Genotype> currentPopulation)
//    {
//        if (genotypesSaved >= SaveFirstNGenotype) return;
//
//        string saveFolder = statisticsFileName + "/";
//
//        foreach (Genotype genotype in currentPopulation)
//        {
//            if (genotype.Evaluation >= 1)
//            {
//                if (!Directory.Exists(saveFolder))
//                    Directory.CreateDirectory(saveFolder);
//
//                genotype.SaveToFile(saveFolder + "Genotype - Finished as " + (++genotypesSaved) + ".txt");
//
//                if (genotypesSaved >= SaveFirstNGenotype) return;
//            }
//            else
//                return; //List should be sorted, so we can exit here
//        }
//    }



    public void EndTheGame()
    {
        if (EndOfGame != null)
            EndOfGame();
    }

    private void OnGUI()
    {
        _generationNumber.text = (GenerationCount).ToString(); //todo : move it to different area, we don't need to update this every frame.
    }


    public void SaveBestAgents(uint amountToSave)
    {
        string saveFolder = GameData.SAVE_DATA_DIRECTORY + "/" + statisticsFileName + "/"; //todo - change it to work in linux and windows.

        uint agentSaved = 0;

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        GameData.instance.agents.Sort();

        foreach (Agent agent in GameData.instance.agents)
        {
            string filePath = saveFolder + String.Format("Agnet - Generation #{0} Finished as {1}.{2}", GenerationCount, ++agentSaved, GameData.AGENT_SUFFIX);
            agent.SaveToFile(filePath);

            if (agentSaved >= amountToSave) break;
        }
    }



    #region GA Operators
    // Selection operator for the genetic algorithm, using a method called remainder stochastic sampling.
    private List<Genotype> RemainderStochasticSampling(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();
        //Put integer portion of genotypes into intermediatePopulation
        //Assumes that currentPopulation is already sorted
        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Fitness < 1)
                break;
            else
            {
                for (int i = 0; i < (int) genotype.Fitness; i++)
                    intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
            }
        }

        //Put remainder portion of genotypes into intermediatePopulation
        foreach (Genotype genotype in currentPopulation)
        {
            float remainder = genotype.Fitness - (int)genotype.Fitness;
            if (randomizer.NextDouble() < remainder)
                intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
        }

        return intermediatePopulation;
    }

    // Recombination operator for the genetic algorithm, recombining random genotypes of the intermediate population
    private List<Genotype> RandomRecombination(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        //Check arguments
        if (intermediatePopulation.Count < 2)
            throw new System.ArgumentException("The intermediate population has to be at least of size 2 for this operator.");

        List<Genotype> newPopulation = new List<Genotype>();
        //Always add best two (unmodified)
        newPopulation.Add(intermediatePopulation[0]);
        newPopulation.Add(intermediatePopulation[1]);


        while (newPopulation.Count < newPopulationSize)
        {
            //Get two random indices that are not the same
            int randomIndex1 = randomizer.Next(0, intermediatePopulation.Count), randomIndex2;
            do
            {
                randomIndex2 = randomizer.Next(0, intermediatePopulation.Count);
            } while (randomIndex2 == randomIndex1);

            Genotype offspring1, offspring2;
            GeneticAlgorithm.CompleteCrossover(intermediatePopulation[randomIndex1], intermediatePopulation[randomIndex2], 
                GeneticAlgorithm.DefCrossSwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
                newPopulation.Add(offspring2);
        }

        return newPopulation;
    }

    // Mutates all members of the new population with the default probability, while leaving the first 2 genotypes in the list untouched.
    private void MutateAllButBestTwo(List<Genotype> newPopulation)
    {
        for (int i = 2; i < newPopulation.Count; i++)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(newPopulation[i], GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }

    // Mutates all members of the new population with the default parameters
    private void MutateAll(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(genotype, GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }
    #endregion
    #endregion

    }

