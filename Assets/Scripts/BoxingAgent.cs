using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BoxingAgent : Agent
{
    public BoxingArena arena;
    public BoxingAgent opponent;

    private float moveSpeed = 3f;
    private float turnSpeed = 200f;

    private float punchRange = 2f;
    private float punchDamage = 10f;
    public float punchCoolDown = 0.5f;

    public float health;
    public bool block;
    public float cooldown;

    private Vector3 startPosition;
    private Quaternion startRotation;

    public override void Initialize()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        health = 100f;
        block = false;
        cooldown = 0f;
        transform.position = startPosition + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        transform.rotation = startRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 toOpponent = transform.InverseTransformDirection(opponent.transform.position - transform.position);
        Vector3 toCenter = transform.InverseTransformDirection(arena.transform.position - transform.position);
        Vector3 opponentForward = transform.InverseTransformDirection(opponent.transform.forward);

        sensor.AddObservation(toOpponent.x / 8f);
        sensor.AddObservation(toOpponent.z / 8f);
        sensor.AddObservation(toCenter.x / 4f);
        sensor.AddObservation(toCenter.z / 4f);
        sensor.AddObservation(opponentForward.x);
        sensor.AddObservation(opponentForward.z);

        sensor.AddObservation(health / 100f);
        sensor.AddObservation(opponent.health / 100f);
        sensor.AddObservation(cooldown / punchCoolDown);
        sensor.AddObservation(opponent.cooldown / punchCoolDown);
        sensor.AddObservation(block ? 1f : 0f);
        sensor.AddObservation(opponent.block ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forward = actions.ContinuousActions[0];
        float side = actions.ContinuousActions[1];
        float turn = actions.ContinuousActions[2];
        int action = actions.DiscreteActions[0];

        block = action == 2;
        float speed = block ? moveSpeed * 0.5f : moveSpeed;

        transform.Rotate(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);
        Vector3 move = transform.forward * forward + transform.right * side;
        transform.position += Vector3.ClampMagnitude(move, 1f * speed * Time.fixedDeltaTime);

        //Vector3 posi = transform.position - arena.transform.position;
        //float limit = 

        cooldown -= Time.fixedDeltaTime;
        if (action == 1 && cooldown <= 0)
        {
            cooldown = punchCoolDown;
            Hit();
        }
    }

    private void Hit()
    {
        Vector3 toOpponent = opponent.transform.position = transform.position;
        bool inRange = toOpponent.magnitude < punchRange;
        bool isFacing = Vector3.Angle(transform.forward, toOpponent) < 30f;
        if (!inRange || !isFacing) return;
        float damage = opponent.block ? punchDamage * 0.2f : punchDamage;
        AddReward(0.05f);
        opponent.AddReward(-0.05f);
        opponent.health -= damage;
        if (opponent.health <= 0) 
        {
            arena.KnockOut(opponent);
        }
    }


}
