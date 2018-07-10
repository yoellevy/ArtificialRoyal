using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerScriptable : ScriptableObject
{
    public abstract Vector2 getMove();
    public abstract Vector2 getShot();
}
