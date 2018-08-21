#region Includes
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

/// <summary>
/// Class representing one member of a population
/// </summary>
[Serializable]
public class Genotype : IComparable<Genotype>, IEnumerable<float>
{
    #region Members
    [NonSerialized]
    private static Random randomizer = new Random();

    /// <summary>
    /// The current evaluation of this genotype.
    /// </summary>
    public float Evaluation
    {
        get;
        set;
    }
    /// <summary>
    /// The current fitness (e.g, the evaluation of this genotype relative 
    /// to the average evaluation of the whole population) of this genotype.
    /// </summary>
    public float Fitness
    {
        get;
        set;
    }

    // The vector of parameters of this genotype.
    private List<float> parameters;

    /// <summary>
    /// The amount of parameters stored in the parameter vector of this genotype.
    /// </summary>
    public int ParameterCount
    {
        get
        {
            if (parameters == null) return 0;
            return parameters.Count;
        }
    }

    // Overridden indexer for convenient parameter access.
    public float this[int index]
    {
        get { return parameters[index]; }
        set { parameters[index] = value; }
    }
    #endregion
    
    #region Constructors
    /// <summary>
    /// Instance of a new genotype with given parameter vector and initial fitness of 0.
    /// </summary>
    /// <param name="parameters">The parameter vector to initialise this genotype with.</param>
    public Genotype(List<float> parameters)
    {
        this.parameters = parameters;
        Evaluation = 0;
        Fitness = 0;
    }

    public Genotype(float[] parameters)
    {
        this.parameters = new List<float>(parameters);
        Evaluation = 0;
        Fitness = 0;
    }

    public Genotype(int parametersCapacity)
    {
        this.parameters = new List<float>(parametersCapacity);
        Evaluation = 0;
        Fitness = 0;
    }

    public Genotype()
    {
        this.parameters = new List<float>();
        Evaluation = 0;
        Fitness = 0;
    }
    #endregion
    

    #region Methods
    #region IComparable
    /// <summary>
    /// Compares this genotype with another genotype depending on their fitness values.
    /// </summary>
    /// <param name="other">The genotype to compare this genotype with.</param>
    /// <returns>The result of comparing the two floating point values representing the genotypes fitness in reverse order.</returns>
    public int CompareTo(Genotype other)
    {
        return other.Fitness.CompareTo(this.Fitness); //in reverse order for larger fitness being first in list
    }
    #endregion

    
    #region IEnumerable
    /// <summary>
    /// Gets an Enumerator to iterate over all parameters of this genotype.
    /// </summary>
    public IEnumerator<float> GetEnumerator()
    {
        for (int i = 0; i < parameters.Count; i++)
            yield return parameters[i];
    }

    /// <summary>
    /// Gets an Enumerator to iterate over all parameters of this genotype.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < parameters.Count; i++)
            yield return parameters[i];
    }

    public void Add(float value)
    {
        parameters.Add(value);
    }
    #endregion
    

    /// <summary>
    /// Sets the parameters of this genotype to random values in given range.
    /// </summary>
    /// <param name="minValue">The minimum inclusive value a parameter may be initialised with.</param>
    /// <param name="maxValue">The maximum exclusive value a parameter may be initialised with.</param>
    public void SetRandomParameters(float minValue, float maxValue)
    {
        //Check arguments
        if (minValue > maxValue) throw new ArgumentException("Minimum value may not exceed maximum value.");

        //Generate random parameter vector
        float range = maxValue - minValue;
        for (int i = 0; i < parameters.Count; i++)
            parameters[i] = (float)((randomizer.NextDouble() * range) + minValue); //Create a random float between minValue and maxValue
    }

    public List<float> GetParameterCopy()
    {
        List<float> copy = new List<float>(ParameterCount);
        for (int i = 0; i < ParameterCount; i++)
            copy[i] = parameters[i];

        return copy;
    }

    /// <summary>
    /// Saves the parameters of this genotype to a file at given file path.
    /// </summary>
    /// <param name="filePath">The path of the file to save this genotype to.</param>
    /// <remarks>This method will override existing files or attempt to create new files, if the file at given file path does not exist.</remarks>
    public void SaveToFile(string filePath)
    {
        StringBuilder builder = new StringBuilder();
        foreach (float param in parameters)
            builder.Append(param.ToString()).Append(";");

        builder.Remove(builder.Length - 1, 1);

        File.WriteAllText(filePath, builder.ToString());
    }
    #endregion
}
