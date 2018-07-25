using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerScriptable : ScriptableObject
{
    protected Vector2 Move;
    protected Vector2 Shot;

    protected PlayerScript playerScript;

    //protected int Id;

    private float numThreshold = 1/3f;

    public void Init(PlayerScript playerScript)
    {
        this.playerScript = playerScript;
        //this.Id = id;
    }

    public abstract void CalculateNextAction();

    int FloatToInt(float num)
    {
        if (num > numThreshold)
            return 1;
        if (num < -numThreshold)
            return -1;
        return 0;
    }

    public Vector2 GetMove()
    {
        return new Vector2(FloatToInt(Move.x), FloatToInt(Move.y));
    }
    public Vector2 GetShot()
    {
        return new Vector2(FloatToInt(Shot.x), FloatToInt(Shot.y));
    }

}
