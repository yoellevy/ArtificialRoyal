using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIController", menuName = "Controller/AI", order = 1)]
public class AIController : PlayerControllerScriptable
{
    public override void CalculateNextAction()
    {
        double[] sensorOutput = Observation.Instant.GetObservationOfPlayerId(Id);
        double[] controlInputs = PlayerAgent.FNN.ProcessInputs(sensorOutput);
        Move = new Vector2((float)controlInputs[0], (float)controlInputs[1]);
        Shot = new Vector2((float)controlInputs[2], (float)controlInputs[3]);
    }
}