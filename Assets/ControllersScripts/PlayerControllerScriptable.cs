using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerScriptable : ScriptableObject
{
    protected Vector2 Move;
    protected Vector2 Shot;

    protected Agent PlayerAgent;

    protected int Id;

    public void Init(Agent agent, int id)
    {
        this.PlayerAgent = agent;
        this.Id = id;
    }

    public abstract void CalculateNextAction();


    public Vector2 GetMove()
    {
        return Move;
    }
    public Vector2 GetShot()
    {
        return Shot;
    }

}
