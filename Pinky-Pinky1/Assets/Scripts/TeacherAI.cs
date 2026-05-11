using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class TeacherAI : MonoBehaviour
{
    [Header("Chase Target")]
    public Transform playerTarget;

    [Header("Movement")]
    public float aggressiveSpeed = 220f;
    public float searchingSpeed = 140f;
    public float tiredSpeed = 80f;

    public float nextWaypointDistance = 3f;

    [Header("Search Timing")]
    public float aggressiveTime = 5f;
    public float searchingTime = 5f;
    public float tiredTime = 5f;

    

    [Header("References")]
    public NightManager nightManager;

    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool facingRight = true;

    private Seeker seeker;
    private Rigidbody2D rb;

    private float currentSpeed;

    private enum SearchState
    {
        Aggressive,
        Searching,
        Tired,
        Finished
    }

    private SearchState currentState;

    private float stateTimer;

    void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
      
    }

    void OnEnable()
    {
        currentState = SearchState.Aggressive;
        stateTimer = aggressiveTime;
        currentSpeed = aggressiveSpeed;

        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);

        Debug.Log("Teacher activated - AGGRESSIVE chase started");
    }

    void OnDisable()
    {
        CancelInvoke(nameof(UpdatePath));

        path = null;
        currentWaypoint = 0;

        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        HandleSearchStates();
    }

    void HandleSearchStates()
    {
        if (currentState == SearchState.Finished)
            return;

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            switch (currentState)
            {
                case SearchState.Aggressive:

                    currentState = SearchState.Searching;
                    stateTimer = searchingTime;
                    currentSpeed = searchingSpeed;

                    Debug.Log("Teacher is now SEARCHING");
                    break;

                case SearchState.Searching:

                    currentState = SearchState.Tired;
                    stateTimer = tiredTime;
                    currentSpeed = tiredSpeed;

                    Debug.Log("Teacher is getting TIRED");
                    break;

                case SearchState.Tired:

                    currentState = SearchState.Finished;

                    Debug.Log("Player survived the chase!");

                    if (nightManager != null)
                    {
                        nightManager.OnNightComplete();
                    }

                    gameObject.SetActive(false);
                    break;
            }
        }
    }

    void UpdatePath()
    {
        if (playerTarget == null)
            return;

        if (seeker.IsDone())
        {
            seeker.StartPath(
                rb.position,
                playerTarget.position,
                OnPathComplete
            );
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction =
     ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;


        Vector2 force = direction * currentSpeed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(
            rb.position,
            path.vectorPath[currentWaypoint]
        );
    }

    }