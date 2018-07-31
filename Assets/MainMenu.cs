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



    private void LoadGameData()
    {
        ErrorText.text = "";

        try
        {
            GameData.instance.LoadGenotypes();
        }
        catch (Exception e) 
        {
            if (e is DirectoryNotFoundException || e is ArgumentException || e is FileNotFoundException)
            {
                ErrorText.text = e.ToString();
                throw new FileLoadException("can't load genotypes");
            }

            throw;
        }
    }
}
