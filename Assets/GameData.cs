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
    public const string COMPARE_BATTLE_GROUPS_FOLDER_NAME = "CompareBattle";
    public const string GROUP_A_GENOTYPES_FOLDER_NAME = "Group_A";
    public const string GROUP_B_GENOTYPES_FOLDER_NAME = "Group_B";

    public List<Genotype> genotypes = new List<Genotype>();
    public List<Genotype> group_A_genotypes = new List<Genotype>();
    public List<Genotype> group_B_genotypes = new List<Genotype>();

    public bool toAddHumanPlayer = false;

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

    public void LoadCompareBattleGenotypes()
    {
        string group_A_location = COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GROUP_A_GENOTYPES_FOLDER_NAME;
        string group_B_location = COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GROUP_B_GENOTYPES_FOLDER_NAME;
        if (!Directory.Exists(group_A_location)) throw new DirectoryNotFoundException(string.Format("Can't find {0} Directory", group_A_location));
        if (!Directory.Exists(group_B_location)) throw new DirectoryNotFoundException(string.Format("Can't find {0} Directory", group_B_location));

        string[] group_A_files = Directory.GetFiles(group_A_location, "*." + GENOTYPE_SUFFIX);
        string[] group_B_files = Directory.GetFiles(group_B_location, "*." + GENOTYPE_SUFFIX);

        if (group_A_files.Length == 0 || group_B_files.Length == 0) throw new FileNotFoundException(string.Format("Can't find files with suffix \"{0}\"", GENOTYPE_SUFFIX));

        foreach (string file in group_A_files)
        {
            group_A_genotypes.Add(Genotype.LoadFromFile(file));
        }
        foreach (string file in group_B_files)
        {
            group_B_genotypes.Add(Genotype.LoadFromFile(file));
        }
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        toAddHumanPlayer = false;
        SceneManager.LoadScene("MainMenu");
    }
}
