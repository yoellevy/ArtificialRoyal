using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIController", menuName = "Controller/AI", order = 1)]
public class AIController : PlayerControllerScriptable
{
    public override void CalculateNextAction()
    {

        double[] sensorOutput = Observation.Instant.getObservationOfPlayerID(id);
        double[] controlInputs = PlayerAgent.FNN.ProcessInputs(sensorOutput);
        move = new Vector2((float)controlInputs[0], (float)controlInputs[1]);
        shot = new Vector2((float)controlInputs[2], (float)controlInputs[3]);
    }
}