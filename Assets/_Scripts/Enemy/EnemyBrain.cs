using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    public enum State
    {
        Move,
        Wait
    }

    [SerializeField] private float waitTime = 1.5f;

    private State state;
    private float timer;

    public bool CanMove => state == State.Move;
    public bool CanShoot => state == State.Wait;

    private void Start()
    {
        SetMove();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (state == State.Move)
                SetWait();
            else
                SetMove();
        }
    }

    private void SetMove()
    {
        state = State.Move;
        timer = Random.Range(1.5f, 3f);
    }

    private void SetWait()
    {
        state = State.Wait;
        timer = waitTime;
    }
}