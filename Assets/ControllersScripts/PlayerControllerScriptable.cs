using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerScriptable : ScriptableObject
{
    protected Vector2 move;
    protected Vector2 shot;

    protected Agent PlayerAgent;

    public void init(Agent agent)
    {
        this.PlayerAgent = agent;
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
