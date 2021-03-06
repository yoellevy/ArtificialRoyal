﻿#region Includes
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    private Text _generationNumber;

    private bool _toSaveGenotypes = false;

    [HideInInspector]
    public int NumberOfGamesInThisGeneration = 0;

    #endregion

    #region Constructors
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EvolutionManager in the Scene.");
            return;
        }
        Instance = this;

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

    /// <summary>
    /// Event for when all agents have died or the game ended.
    /// </summary>
    public event System.Action EndOfGame;
    
   

    /// <summary>
    /// Starts the evolutionary process.
    /// </summary>
    public void StartEvolution()
    {
        NumberOfGamesInThisGeneration = 0;

        int weightCount = NeuralNetwork.CalculateOverallWeightCount(GameData.instance.useRNN, GameData.instance.NNTopology);

        //Setup genetic algorithm
        geneticAlgorithm = new GeneticAlgorithm((uint)weightCount, (uint)(GameManager.Instance.playerAmount - GameManager.Instance.randomPlayerAmount))
        {
            Evaluation = GameManager.Instance.RestartTheGame,
            Selection = GeneticAlgorithm.SelectBestNAndRandomMSelectionOperator,
            Recombination = RandomRecombination,
            Mutation = MutateAllButBestN
        };


        EndOfGame += GameManager.Instance.EvalOfEndGame;
        EndOfGame += geneticAlgorithm.EvaluationFinished;

        //Statistics
        if (SaveStatistics)
        {
            statisticsFileName = "Evaluation - " + DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");
            WriteStatisticsFileStart();
            geneticAlgorithm.FitnessCalculationFinished += WriteStatisticsToFile;
        }

        geneticAlgorithm.Start();
    }

    private void CreateAgentsAndRestart(IEnumerable<Genotype> currentPopulation)
    {
        GameManager.Instance.RestartTheGame();
    }

    // Writes the starting line to the statistics file, stating all genetic algorithm parameters.
    private void WriteStatisticsFileStart()
    {
        string saveFolder = GameData.SAVE_DATA_DIRECTORY + "/";

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        File.WriteAllText(saveFolder + statisticsFileName + ".txt", "Selection:" + Environment.NewLine +
            "Select N Best: " + GameData.instance.SelectNBest.ToString() + Environment.NewLine +
            "Select M Random: " + GameData.instance.SelectMRandom.ToString() + Environment.NewLine +
            Environment.NewLine + "Breed / Recombination:" + Environment.NewLine +
            "Amount To Save From Selection: " + GameData.instance.BreadAmountToSaveFromSelection.ToString() + Environment.NewLine +
            "Swap Probability: " + GameData.instance.SwapProb.ToString() + Environment.NewLine +
            Environment.NewLine + "Mutation:" + Environment.NewLine +
            "Mutation Save Amount: " + GameData.instance.MutationSaveAmount.ToString() + Environment.NewLine +
            "Mutation Percent: " + GameData.instance.MutationPerc.ToString() + Environment.NewLine +
            "Mutation probability: " + GameData.instance.MutationProb.ToString() + Environment.NewLine +
            "Mutation Amount: " + GameData.instance.MutationAmount.ToString() + Environment.NewLine +
            Environment.NewLine + "Evaluation:" + Environment.NewLine +
            "Rank: " + GameData.instance.EvalRank.ToString() + Environment.NewLine +
            "Kills: " + GameData.instance.EvalKills.ToString() + Environment.NewLine +
            Environment.NewLine + (GameData.instance.isNewAgents? "New" : "Loaded") +" Agents:" + Environment.NewLine +
            "Activision Function: " + GameData.instance.activationFunctionType.ToString() + Environment.NewLine +
            "NN Type: " + (GameData.instance.useRNN? "indRNN" : "FNN") + Environment.NewLine +
            "NN Topology: " + TopologyToString(GameData.instance.NNTopology) + Environment.NewLine +
            Environment.NewLine + "Games per Generation: " + GameData.instance.NumberOfGamesPerGeneration.ToString() + Environment.NewLine + Environment.NewLine);
    }

    private static string TopologyToString(uint[] Topology)
    {
        string[] sArr = new string[Topology.Length];

        for (int i = 0; i < Topology.Length; i++)
            sArr[i] = Topology[i].ToString();

        return String.Join(",", sArr);
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
        //Always add atlist best two (unmodified)
        for (int i = 0; i < GameData.instance.BreadAmountToSaveFromSelection; i++)
        {
            newPopulation.Add(intermediatePopulation[i]);
        }

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
                GameData.instance.SwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
                newPopulation.Add(offspring2);
        }

        return newPopulation;
    }

    private void MutateAllButBestN(List<Genotype> newPopulation)
    {
        for (int i = GameData.instance.MutationSaveAmount; i < newPopulation.Count; i++)
        {
            if (randomizer.NextDouble() < GameData.instance.MutationPerc)
                GeneticAlgorithm.MutateGenotype(newPopulation[i], GameData.instance.MutationProb, GameData.instance.MutationAmount);
        }
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

