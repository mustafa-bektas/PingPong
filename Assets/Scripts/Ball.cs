using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField, Min(0f)] public float maxXSpeed = 20f, maxStartXSpeed = 2f, constantYSpeed = 10f, extents = 0.5f;

    public Vector2 position, velocity;

    [SerializeField]
    public ParticleSystem startParticleSystem, bounceParticleSystem, trailParticleSystem;

    [SerializeField]
    public int bounceParticleEmission = 20;

    [SerializeField]
    public int startParticleEmission = 100;

    public float Extents => extents;
    public Vector2 Position => position;
    public Vector2 Velocity => velocity;

    void Awake() => gameObject.SetActive(false);

    public void Initialize(Vector2 startPos, Vector2 startVel)
    {
        position = startPos;
        velocity = startVel;
        UpdateVisualization();
        gameObject.SetActive(true);
        startParticleSystem.Emit(startParticleEmission);
        SetTrailEmission(true);
        trailParticleSystem.Play();
    }

    public void UpdateVisualization() => trailParticleSystem.transform.localPosition =
        transform.localPosition = new Vector3(position.x, 0f, position.y);

    public void Move()
    {
        position += velocity * Time.deltaTime;
    }

    public void StartNewGame()
    {
        position = Vector2.zero;
        UpdateVisualization();
        velocity.x = Random.Range(-maxStartXSpeed, maxStartXSpeed);
        velocity.y = -constantYSpeed;
        gameObject.SetActive(true);
        startParticleSystem.Emit(startParticleEmission);
        SetTrailEmission(true);
        trailParticleSystem.Play();
    }

    public void EndGame()
    {
        position.x = 0f;
        gameObject.SetActive(false);
        SetTrailEmission(false);
    }

    void SetTrailEmission(bool enabled)
    {
        ParticleSystem.EmissionModule emission = trailParticleSystem.emission;
        emission.enabled = enabled;
    }

    public void SetXPositionAndSpeed(float start, float speedFactor, float deltaTime)
    {
        velocity.x = maxXSpeed * speedFactor;
        position.x = start + velocity.x * deltaTime;
    }

    public void BounceX(float boundary)
    {
        float durationAfterBounce = (position.x - boundary) / velocity.x;
        position.x = 2f * boundary - position.x;
        velocity.x = -velocity.x;
        EmitBounceParticles(
            boundary,
            position.y - velocity.y * durationAfterBounce,
            boundary < 0f ? 90f : 270f
        );
    }

    public void BounceY(float boundary)
    {
        float durationAfterBounce = (position.y - boundary) / velocity.y;
        position.y = 2f * boundary - position.y;
        velocity.y = -velocity.y;
        EmitBounceParticles(
            position.x - velocity.x * durationAfterBounce,
            boundary,
            boundary < 0f ? 0f : 180f
        );
    }

    void EmitBounceParticles(float x, float z, float rotation)
    {
        ParticleSystem.ShapeModule shape = bounceParticleSystem.shape;
        shape.position = new Vector3(x, 0f, z);
        shape.rotation = new Vector3(0f, rotation, 0f);
        bounceParticleSystem.Emit(bounceParticleEmission);
    }
    
    // Add this to your Ball.cs script:

    [SerializeField] bool isMainBall = true; // Check this for your main ball in inspector
    bool isAutonomous = false; // Set to true for power-up balls

    void Update()
    {
        // Only move autonomously if this is NOT the main ball
        if (isAutonomous && !isMainBall)
        {
            Move();
            UpdateVisualization();
            
            // Handle bouncing for autonomous balls
            HandleAutonomousBouncing();
        }
    }

    void HandleAutonomousBouncing()
    {
        // Get arena extents from Game manager
        Game gameManager = FindObjectOfType<Game>();
        if (gameManager == null) return;
        
        Vector2 arenaExtents = gameManager.arenaExtents;
        
        // Check X bounds
        float xExtents = arenaExtents.x - extents;
        if (position.x < -xExtents)
        {
            BounceX(-xExtents);
        }
        else if (position.x > xExtents)
        {
            BounceX(xExtents);
        }
        
        // Check Y bounds (goals) - destroy extra ball if it hits
        float yExtents = arenaExtents.y - extents;
        if (position.y < -yExtents || position.y > yExtents)
        {
            // Extra ball hit a goal - just destroy it
            Destroy(gameObject);
        }
    }

    // Add this method for power-up balls
    public void MakeAutonomous()
    {
        isAutonomous = true;
        isMainBall = false;
    }
}
