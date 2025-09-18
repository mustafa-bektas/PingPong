using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField, Min(0f)] float maxXSpeed = 20f, maxStartXSpeed = 2f, constantYSpeed = 10f, extents = 0.5f;

    Vector2 position, velocity;
    
    public float Extents => extents;
	public Vector2 Position => position;
    public Vector2 Velocity => velocity;

	void Awake () => gameObject.SetActive(false);

    public void UpdateVisualization() =>
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
	}

	public void EndGame ()
	{
		position.x = 0f;
		gameObject.SetActive(false);
	}
    
    public void SetXPositionAndSpeed (float start, float speedFactor, float deltaTime)
    {
        velocity.x = maxXSpeed * speedFactor;
        position.x = start + velocity.x * deltaTime;
    }

    public void BounceX(float boundary)
    {
        position.x = 2f * boundary - position.x;
        velocity.x = -velocity.x;
    }

	public void BounceY (float boundary)
	{
		position.y = 2f * boundary - position.y;
		velocity.y = -velocity.y;
	}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
