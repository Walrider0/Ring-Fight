using UnityEngine;

public class BoxingArena : MonoBehaviour
{
    public BoxingAgent agentA;
    public BoxingAgent agentB;
    public float half = 4f;
    public int maxRoundSteps = 3000;

    int steps;


    private void FixedUpdate()
    {
        steps++;
        if (steps >= maxRoundSteps)
        {
            agentA.EpisodeInterrupted();
            agentB.EpisodeInterrupted();
            steps = 0;
        }
    }

    public void KnockOut(BoxingAgent loser)
    {
        BoxingAgent winner = loser == agentA ? agentB : agentA;
        winner.AddReward(1f);
        loser.AddReward(-1f);
        winner.EndEpisode();
        loser.EndEpisode();
        steps = 0;
    }
}
