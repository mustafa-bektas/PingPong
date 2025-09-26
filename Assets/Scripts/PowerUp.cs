using UnityEngine;

public enum PowerUpType
{
    SpeedBoost,
    PaddleStretch,
    MultiBall,
    Freeze,
    Magnet
}

public class PowerUp : MonoBehaviour
{
    [SerializeField] PowerUpType powerUpType;
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float bobHeight = 0.3f;
    [SerializeField] ParticleSystem collectParticles;
    [SerializeField] float duration = 5f;

    Vector3 startPosition;
    
    public PowerUpType Type => powerUpType;
    public float Duration => duration;

    void Start()
    {
        startPosition = transform.position;

        // Set random power-up type if not set in inspector
        if (powerUpType == PowerUpType.SpeedBoost) // Default check
        {
            powerUpType = (PowerUpType)Random.Range(0, System.Enum.GetValues(typeof(PowerUpType)).Length);
        }

        
        // Set visual based on type
        SetVisualStyle();
        
        // Auto-destroy after 10 seconds if not collected
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // Rotate the power-up
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void SetVisualStyle()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        // Set colors based on power-up type
        Color powerUpColor = GetPowerUpColor();
        Material mat = renderer.material;
        
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", powerUpColor);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", powerUpColor * 2f); // Brighter for emission
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", powerUpColor * 1.5f);
    }

    Color GetPowerUpColor()
    {
        switch (powerUpType)
        {
            case PowerUpType.SpeedBoost: return Color.red;
            case PowerUpType.PaddleStretch: return Color.green;
            case PowerUpType.MultiBall: return Color.blue;
            case PowerUpType.Freeze: return Color.cyan;
            case PowerUpType.Magnet: return Color.magenta;
            default: return Color.white;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"PowerUp collected by BALL: {powerUpType} for {duration} seconds!");

        // Check if the ball hit the power-up
        if (other.GetComponent<Ball>() != null)
        {
            Debug.Log($"PowerUp collected by BALL: {powerUpType} for {duration} seconds!");

            // Collect the power-up
            PowerUpManager.Instance?.CollectPowerUp(this);

            // Play collection effect
            if (collectParticles != null)
            {
                collectParticles.transform.SetParent(null);
                collectParticles.Emit(30);
                Destroy(collectParticles.gameObject, 2f);
            }

            Destroy(gameObject);
        }
    }
}