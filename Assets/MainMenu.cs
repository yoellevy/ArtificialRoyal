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
}
