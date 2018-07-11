using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerScriptable : ScriptableObject
{
    protected Vector2 move;
    protected Vector2 shot;

    protected Agent PlayerAgent;

    protected int id;

    public void init(Agent agent, int id)
    {
        this.PlayerAgent = agent;
        this.id = id;
    }

    public abstract void CalculateNextAction();


    public Vector2 getMove()
    {
        return move;
    }
    public Vector2 getShot()
    {
        return shot;
    }

}
