using System;

namespace AI
{
    public static class EvaluationFunctionsImplementaion
    {
        public static float evalAgent(Agent agent)
        {
            var function = agent.EvaluationFunctions;
            switch (function)
            {
                case EvaluationFunctions.None:
                    return 0;

                case EvaluationFunctions.Survive:
                    return HandleSurvive(agent);

                case EvaluationFunctions.Kill:
                    return HandleKill(agent);

                case EvaluationFunctions.Rank:
                    return HandleRank(agent);
                    
                case EvaluationFunctions.LinearComposition:
                    return HandleLinearComposition(agent);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static float HandleLinearComposition(Agent agent)
        {
            var weights = agent.Weights;
            return weights[0]*HandleRank(agent) + weights[1]*HandleKill(agent) + weights[2]*HandleSurvive(agent);
        }

        private static float HandleRank(Agent agent)
        {
            var rank = agent.Rank;
            return -rank; // the lower the rank, the better.
        }

        private static float HandleKill(Agent agent)
        {
            var killCount = agent.KillCount;
            return killCount;
        }

        private static float HandleSurvive(Agent agent)
        {
            var survivalTime = agent.SurvivelTime;
            return survivalTime;
        }
    }
}