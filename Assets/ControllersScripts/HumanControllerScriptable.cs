using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "HumanController", menuName = "Controller/Human", order = 1)]
public class HumanControllerScriptable : PlayerControllerScriptable
{

    [SerializeField]
    string moveUp, moveDown, moveLeft, moveRight;
    [SerializeField]
    string shotUp, shotDown, shotLeft, shotRight;

    public override Vector2 getMove()
    {
        float y = Input.GetKey(moveUp) ? 1 : 0 + (Input.GetKey(moveDown) ? -1 : 0),
            x = Input.GetKey(moveRight) ? 1 : 0 + (Input.GetKey(moveLeft) ? -1 : 0);

        return new Vector2(x, y);
    }

    public override Vector2 getShot()
    {
        float y = Input.GetKey(shotUp) ? 1 : 0 + (Input.GetKey(shotDown) ? -1 : 0),
            x = Input.GetKey(shotRight) ? 1 : 0 + (Input.GetKey(shotLeft) ? -1 : 0);

        return new Vector2(x, y);
    }
}
