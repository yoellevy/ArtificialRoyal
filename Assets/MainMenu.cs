using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {


    [SerializeField]
    private Text ErrorText;

    [SerializeField]
    private Dropdown ActivitionFunctionDropDown;
    [SerializeField]
    private Dropdown NNTypeDropDown;
    [SerializeField]
    private Dropdown TopologyDropDown;

    [SerializeField]
    private InputField SelectNBestInputField;
    [SerializeField]
    private InputField SelectMRandomInputField;
    [SerializeField]
    private InputField AmountToSaveFromSelectionInputField;
    [SerializeField]
    private Slider SwapProbSlider;
    [SerializeField]
    private Text SwapProbText;

    [SerializeField]
    private InputField MutationSaveAmountInputField;
    [SerializeField]
    private Slider MutationPercSlider;
    [SerializeField]
    private Text MutationPercText;

    [SerializeField]
    private Slider MutationProbSlider;
    [SerializeField]
    private Text MutationProbText;
    [SerializeField]
    private InputField MutationAmountInputField;

    [SerializeField]
    private InputField EvalRankInputField;
    [SerializeField]
    private InputField EvalKillsInputField;

    [SerializeField]
    private InputField CompareBattleGameToPlayInputField;

    private void Start()
    {
        SetTrainUIData();
        SetCompareBattleUIData();
    }

    public void TrainFromScratch()
    {
        ErrorText.text = "";
        SceneManager.LoadScene("Evolution");
    }

    public void TrainFromData()
    {
        ErrorText.text = "";

        try
        {
            LoadGameData();
            SceneManager.LoadScene("Evolution");
        }
        catch (FileLoadException e)
        {
            ErrorText.text = e.ToString() + "\n" + ErrorText.text;
        }
    }

    public void PlayGameJustAI()
    {
        ErrorText.text = "";

        try
        {
            LoadGameData();
            SceneManager.LoadScene("Game");
        }
        catch (FileLoadException e)
        {
            ErrorText.text = e.ToString() + "\n" + ErrorText.text;
        }
        
    }

    public void PlayGameWithAI()
    {
        ErrorText.text = "";

        try
        {
            LoadGameData();
            GameData.instance.toAddHumanPlayer = true;
            SceneManager.LoadScene("Game");
        }
        catch (FileLoadException e)
        {
            ErrorText.text = e.ToString() + "\n" + ErrorText.text;
        }

    }

    public void PlayCompareBattle()
    {
        ErrorText.text = "";

        try
        {
            string group_A_location = GameData.COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GameData.GROUP_A_FOLDER_NAME;
            string group_B_location = GameData.COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GameData.GROUP_B_FOLDER_NAME;
            GameData.instance.LoadAgents(group_A_location, out GameData.instance.agents);
            GameData.instance.LoadAgents(group_B_location, out GameData.instance.agents_group_B);

            GameData.instance.CompareBattleGamesToPlay = int.Parse(CompareBattleGameToPlayInputField.text);
        }
        catch (Exception e)
        {
            if (e is DirectoryNotFoundException || e is ArgumentException || e is FileNotFoundException)
            {
                ErrorText.text = e.ToString();
            }

            throw e;
        }

        SceneManager.LoadScene("CompareBattle");

    }



    private void LoadGameData()
    {
        ErrorText.text = "";

        try
        {
            GameData.instance.LoadAgents(GameData.AGENTS_FOLDER_NAME, out GameData.instance.agents);
        }
        catch (Exception e) 
        {
            if (e is DirectoryNotFoundException || e is ArgumentException || e is FileNotFoundException)
            {
                ErrorText.text = e.ToString();
                throw new FileLoadException("can't load agents");
            }

            throw e;
        }
    }

    public void SetActivitionFunction()
    {
        GameData.instance.activationFunctionType = (NeuralLayer.ActivationFunctionType)ActivitionFunctionDropDown.value;
    }

    public void SetNNType()
    {
        GameData.instance.useRNN = NNTypeDropDown.value == 0;
    }

    public void SetTopology()
    {
        switch (TopologyDropDown.value)
        {
            case 0:
                GameData.instance.NNTopology = new uint[5] {21, 27, 16, 8, 4};
                break;
            case 1:
                GameData.instance.NNTopology = new uint[5] { 21, 16, 12, 8, 4 };
                break;
        }
    }

    private void SetCompareBattleUIData()
    {
        CompareBattleGameToPlayInputField.text = GameData.instance.CompareBattleGamesToPlay.ToString();
    }

    private void SetTrainUIData()
    {
        SelectNBestInputField.text = GameData.instance.SelectNBest.ToString();
        SelectMRandomInputField.text = GameData.instance.SelectMRandom.ToString();

        AmountToSaveFromSelectionInputField.text = GameData.instance.BreadAmountToSaveFromSelection.ToString();
        float value = GameData.instance.SwapProb;
        SwapProbSlider.value = value;
        SwapProbText.text = value.ToString();

        MutationSaveAmountInputField.text = GameData.instance.MutationSaveAmount.ToString();
        value = GameData.instance.MutationPerc;
        MutationPercSlider.value = value;
        MutationPercText.text = value.ToString();

        value = GameData.instance.MutationProb;
        MutationProbSlider.value = value;
        MutationProbText.text = value.ToString();

        MutationAmountInputField.text = GameData.instance.MutationAmount.ToString();

        EvalRankInputField.text = GameData.instance.EvalRank.ToString();
        EvalKillsInputField.text = GameData.instance.EvalKills.ToString();
    }

    public void SetGeneticToGameData()
    {
        GameData.instance.SelectNBest = int.Parse(SelectNBestInputField.text);
        GameData.instance.SelectMRandom = int.Parse(SelectMRandomInputField.text);
        GameData.instance.BreadAmountToSaveFromSelection = int.Parse(AmountToSaveFromSelectionInputField.text);
        GameData.instance.SwapProb = SwapProbSlider.value;
        GameData.instance.MutationSaveAmount = int.Parse(MutationSaveAmountInputField.text);
        GameData.instance.MutationPerc = MutationPercSlider.value;
        GameData.instance.MutationProb = MutationProbSlider.value;
        GameData.instance.MutationAmount = int.Parse(MutationAmountInputField.text);
    }

    public void IntegerInputFielOnValueChanged(InputField inputField)
    {
        try
        {
            int.Parse(inputField.text);
        }
        catch (FormatException e)
        {
            inputField.text = "";
        }
    }


    public void SwapProbOnValueChanged()
    {
        float value = (float)Math.Round(SwapProbSlider.value, 2);
        SwapProbSlider.value = value;
        SwapProbText.text = value.ToString();
    }

    public void MutationPercOnValueChanged()
    {
        float value = (float)Math.Round(MutationPercSlider.value, 2);
        MutationPercSlider.value = value;
        MutationPercText.text = value.ToString();
    }

    public void MutationProbOnValueChanged()
    {
        float value = (float)Math.Round(MutationProbSlider.value, 2);
        MutationProbSlider.value = value;
        MutationProbText.text = value.ToString();
    }

    public void SetEvaluationToGameData()
    {
        GameData.instance.EvalRank = int.Parse(EvalRankInputField.text);
        GameData.instance.EvalKills = int.Parse(EvalKillsInputField.text);;
    }

    public void StartTrain()
    {
        //set NN:
        SetActivitionFunction();
        SetNNType();
        SetTopology();

        //set Genetics:
        SetGeneticToGameData();

        //set Evaluation:
        SetEvaluationToGameData();

        //load Scene:
        SceneManager.LoadScene("Evolution");
    }

    public void StartCompareBattle()
    {

    }
}
