using System;

namespace AI
{
    public static class EvaluationFunctionsImplementaion
    {
        public static float EvalPlayer(PlayerScript playerScript)
        {
            var function = playerScript.EvaluationFunction;
            switch (function)
            {
                case EvaluationFunctions.None:
                    return 0;

                case EvaluationFunctions.Survive:
                    return HandleSurvive(playerScript);

                case EvaluationFunctions.Kill:
                    return HandleKill(playerScript);

                case EvaluationFunctions.Rank: // ferst ranked is the last one to servive , and rank last is the first one to die
                    return HandleRank(playerScript);
                    
                case EvaluationFunctions.LinearComposition:
                    return HandleLinearComposition(playerScript);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static float HandleLinearComposition(PlayerScript agent)
        {
            var weights = agent.Weights;
            return weights[0]*HandleRank(agent) + weights[1]*HandleKill(agent) + weights[2]*HandleSurvive(agent);
        }

        private static float HandleRank(PlayerScript agent)
        {
            var rank = agent.Rank;
            return -rank; // the lower the rank, the better.
        }

        private static float HandleKill(PlayerScript agent)
        {
            var killCount = agent.KillCount;
            return killCount;
        }

        private static float HandleSurvive(PlayerScript agent)
        {
            var survivalTime = agent.SurvivelTime;
            return survivalTime;
        }
    }
}