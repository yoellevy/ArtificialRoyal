using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour {

    public static GameData instance = null;

    public const string SAVE_DATA_DIRECTORY = "saves";

    public const string GENOTYPES_FOLDER_NAME = "genotypes";
    public const string GENOTYPE_SUFFIX = "genotype";

    public List<Genotype> genotypes = new List<Genotype>();

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

    public void LoadGenotypes()
    {
        if (!Directory.Exists(GENOTYPES_FOLDER_NAME)) throw new DirectoryNotFoundException(string.Format("Can't find {0} Directory", GENOTYPES_FOLDER_NAME));

        string[] files = Directory.GetFiles(GENOTYPES_FOLDER_NAME, "*." + GENOTYPE_SUFFIX);

        if (files.Length == 0) throw new FileNotFoundException(string.Format("Can't find files with suffix \"{0}\"", GENOTYPE_SUFFIX));

        foreach (string file in files)
        {
            genotypes.Add(Genotype.LoadFromFile(file));
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
