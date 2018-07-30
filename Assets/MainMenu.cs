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
    private Text Data_File_Succeeded;

    public const string DATA_LOCATION = "save_folder";
    public const string DATA_SUFFIX = "AIData";

    public const string GENOTYPES_FOLDER_NAME = "genotypes";
    public const string GENOTYPE_SUFFIX = "genotype";

    


    List<Genotype> genotypes;


    public void TrainFromScratch()
    {
        ErrorText.text = "";
        SceneManager.LoadScene("Evolution");
    }

    public void LoadGameData()
    {
        ErrorText.text = "";

        try
        {
            GameData.instance.LoadGenotypes();
        } catch (DirectoryNotFoundException e)
        {
            ErrorText.text = string.Format("Can't find {0} Directory", GENOTYPES_FOLDER_NAME);
        }
        catch (ArgumentException e)
        {
            ErrorText.text = e.ToString();
        }
    }
}
