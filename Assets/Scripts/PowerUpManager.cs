using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [SerializeField] GameObject powerUpPrefab;
    [SerializeField] float spawnInterval = 8f;
    [SerializeField] Vector2 spawnAreaX = new Vector2(-8f, 8f);
    [SerializeField] Vector2 spawnAreaZ = new Vector2(-8f, 8f);
    [SerializeField] float spawnHeight = 1f;

    // Power-up effects
    [Header("Power-Up Effects")]
    [SerializeField] float speedBoostMultiplier = 1.5f;
    [SerializeField] float paddleStretchMultiplier = 1.8f;
    [SerializeField] float freezeSlowdown = 0.3f;
    [SerializeField] float magnetStrength = 5f;

    // References
    Game gameManager;
    Ball ballScript;
    Paddle playerPaddle, aiPaddle;
    
    // Active effects tracking
    Dictionary<PowerUpType, Coroutine> activePowerUps = new Dictionary<PowerUpType, Coroutine>();
    
    // Multi-ball tracking
    List<GameObject> extraBalls = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find references
        gameManager = FindObjectOfType<Game>();
        ballScript = FindObjectOfType<Ball>();
        
        Paddle[] paddles = FindObjectsOfType<Paddle>();
        foreach (var paddle in paddles)
        {
            if (!paddle.isAI) playerPaddle = paddle;
            else aiPaddle = paddle;
        }

        // Start spawning power-ups
        InvokeRepeating(nameof(SpawnPowerUp), spawnInterval, spawnInterval);
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefab == null) 
        {
            Debug.LogWarning("PowerUp prefab is not assigned!");
            return;
        }

        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaX.x, spawnAreaX.y),
            spawnHeight,
            Random.Range(spawnAreaZ.x, spawnAreaZ.y)
        );

        Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);
    }

    public void CollectPowerUp(PowerUp powerUp)
    {
        Debug.Log($"PowerUpManager: Activating {powerUp.Type} effect for {powerUp.Duration} seconds");
        
        // Stop any existing effect of the same type
        if (activePowerUps.ContainsKey(powerUp.Type))
        {
            Debug.Log($"Stopping existing {powerUp.Type} effect");
            StopCoroutine(activePowerUps[powerUp.Type]);
            activePowerUps.Remove(powerUp.Type);
        }

        // Start new effect
        Coroutine effectCoroutine = StartCoroutine(ApplyPowerUpEffect(powerUp.Type, powerUp.Duration));
        activePowerUps[powerUp.Type] = effectCoroutine;
        
        // Visual feedback - screen shake using your LivelyCamera
        if (FindObjectOfType<LivelyCamera>() != null)
        {
            FindObjectOfType<LivelyCamera>().JostleY();
        }
    }

    IEnumerator ApplyPowerUpEffect(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.SpeedBoost:
                yield return ApplySpeedBoost(duration);
                break;
            case PowerUpType.PaddleStretch:
                yield return ApplyPaddleStretch(duration);
                break;
            case PowerUpType.MultiBall:
                yield return ApplyMultiBall(duration);
                break;
            case PowerUpType.Freeze:
                yield return ApplyFreeze(duration);
                break;
            case PowerUpType.Magnet:
                yield return ApplyMagnet(duration);
                break;
        }

        // Remove from active effects
        if (activePowerUps.ContainsKey(type))
            activePowerUps.Remove(type);
    }

    IEnumerator ApplySpeedBoost(float duration)
    {
        if (playerPaddle == null) yield break;
        
        float originalSpeed = playerPaddle.speed;
        playerPaddle.speed *= speedBoostMultiplier;
        Debug.Log($"Speed Boost activated! Speed: {originalSpeed} -> {playerPaddle.speed}");
        
        yield return new WaitForSeconds(duration);
        
        if (playerPaddle != null)
        {
            playerPaddle.speed = originalSpeed;
            Debug.Log($"Speed Boost ended. Speed restored to: {originalSpeed}");
        }
    }

    IEnumerator ApplyPaddleStretch(float duration)
    {
        if (playerPaddle == null) yield break;
        
        float originalExtents = playerPaddle.maxExtents;
        playerPaddle.maxExtents *= paddleStretchMultiplier;
        // Force paddle to update its visual scale
        playerPaddle.SetExtents(playerPaddle.maxExtents);
        Debug.Log($"Paddle Stretch activated! Size: {originalExtents} -> {playerPaddle.maxExtents}");
        
        yield return new WaitForSeconds(duration);
        
        if (playerPaddle != null)
        {
            playerPaddle.maxExtents = originalExtents;
            // Reset to current score-based extents
            playerPaddle.StartNewGame();
            Debug.Log($"Paddle Stretch ended. Size restored to: {originalExtents}");
        }
    }

    IEnumerator ApplyMultiBall(float duration)
    {
        if (ballScript == null) 
        {
            Debug.LogError("Ball script is null!");
            ballScript = FindObjectOfType<Ball>();
        }

        Debug.Log("Multi-Ball activated! Creating 2 extra balls");
        
        // Get current ball position from transform
        Vector3 currentBallPos = ballScript.transform.position;
        
        // Create 2 extra balls
        for (int i = 0; i < 4; i++)
        {
            GameObject extraBall = Instantiate(ballScript.gameObject);
            Ball extraBallScript = extraBall.GetComponent<Ball>();
            
            if (extraBallScript == null)
            {
                Debug.LogError("Failed to get Ball component from instantiated ball!");
                Destroy(extraBall);
                continue;
            }
            
            // Set random direction and position
            Vector2 randomVel = new Vector2(
                Random.Range(-extraBallScript.maxStartXSpeed, extraBallScript.maxStartXSpeed),
                extraBallScript.constantYSpeed * (Random.value > 0.5f ? 1 : -1)
            );
            
            // Use current ball transform position and add slight offset
            Vector2 startPos = new Vector2(
                currentBallPos.x + Random.Range(-1f, 1f), 
                currentBallPos.z + Random.Range(-0.5f, 0.5f)
            );
            
            // Set up the ball
            extraBallScript.position = startPos;
            extraBallScript.velocity = randomVel;
            
            // Make it autonomous so it moves itself
            if (extraBallScript.GetType().GetMethod("MakeAutonomous") != null)
            {
                extraBallScript.MakeAutonomous();
                Debug.Log($"Made extra ball {i} autonomous");
            }
            
            extraBallScript.UpdateVisualization();
            extraBall.SetActive(true);
                
            extraBalls.Add(extraBall);
        }

        yield return new WaitForSeconds(duration);
        
        Debug.Log("Multi-Ball ended. Removing extra balls");
        // Remove extra balls
        foreach (var ball in extraBalls)
        {
            if (ball != null)
            {
                Destroy(ball);
            }
        }
        extraBalls.Clear();
    }

    IEnumerator ApplyFreeze(float duration)
    {
        if (aiPaddle == null) yield break;
        
        float originalSpeed = aiPaddle.speed;
        aiPaddle.speed *= freezeSlowdown;
        Debug.Log($"Freeze activated! AI Speed: {originalSpeed} -> {aiPaddle.speed}");
        
        yield return new WaitForSeconds(duration);
        
        if (aiPaddle != null)
        {
            aiPaddle.speed = originalSpeed;
            Debug.Log($"Freeze ended. AI Speed restored to: {originalSpeed}");
        }
    }

    IEnumerator ApplyMagnet(float duration)
    {
        Debug.Log($"Magnet activated for {duration} seconds!");
        float timer = 0f;
        
        while (timer < duration && ballScript != null && playerPaddle != null)
        {
            // Get ball position
            Vector3 ballPos = ballScript.transform.position;
            Vector3 paddlePos = playerPaddle.transform.position;
            
            // Only apply magnet force when ball is on player's side
            if (ballPos.z < 0)
            {
                // Calculate attraction force
                Vector3 direction = (paddlePos - ballPos).normalized;
                direction.y = 0; // Only horizontal attraction
                
                // Apply force directly to velocity (now that it's public)
                Vector2 magnetForce = new Vector2(direction.x * magnetStrength * Time.deltaTime, 0);
                ballScript.velocity += magnetForce;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("Magnet effect ended");
    }
}