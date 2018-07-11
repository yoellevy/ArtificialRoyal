using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : PlayerControllerScriptable
{
    public override void CalculateNextAction()
    {
        base.move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        base.shot = new Vector2(Input.GetAxis("HorizontalShot"), Input.GetAxis("VerticalShot"));
    }
}