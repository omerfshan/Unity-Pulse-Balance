using UnityEngine;
using UnityEngine.Events;

public class PulseController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform heartT;  
    [SerializeField] private Transform limitT;   

    [Header("Heart (Player)")]
    [SerializeField] private float growSpeed = 2f;
    [SerializeField] private float shrinkSpeed = 2f;
    [SerializeField] private float heartSmooth = 8f;
    [SerializeField] private float heartMin = 0.5f;
    [SerializeField] private float heartMax = 10f;
    [SerializeField] private float heartStart = 1f;

    [Header("Limit (Auto)")]
    [SerializeField] private float limitMin = 1f;
    [SerializeField] private float limitMax = 10f;
    [SerializeField] private float limitStart = 1.5f;
    [SerializeField] private float limitMoveSpeed = 0.4f;
    [SerializeField] private Vector2 limitWaitRange = new Vector2(0.8f, 1.8f);

    [Header("Events")]
    public UnityEvent onGameOver;

    private float heartTarget, heartCurrent;
    private float limitTarget, limitCurrent;
    private float limitWaitTimer;
    private bool gameOver = false;

    private Vector3 heartBaseScale, limitBaseScale;

    void Start()
    {
        if (!heartT || !limitT)
        {
            Debug.LogError("PulseController: heartT ve limitT atanmamış!");
            enabled = false;
            return;
        }

        heartBaseScale = heartT.localScale / Mathf.Max(heartT.localScale.x, 0.0001f);
        limitBaseScale = limitT.localScale / Mathf.Max(limitT.localScale.x, 0.0001f);

        heartTarget = heartCurrent = heartStart;
        limitTarget = limitCurrent = limitStart;

        ResetLimitTimer();
        ApplyScalesImmediate();
    }

    void Update()
    {
        if (gameOver) return;

        UpdateHeart();
        UpdateLimit();
        CheckGameOver();

        // Uygula
        heartT.localScale = heartBaseScale * heartCurrent;
        limitT.localScale = limitBaseScale * limitCurrent;
    }

    // ---------------- HEART ----------------
    private void UpdateHeart()
    {
        // Basılıysa büyü, değilse küçül
        if (Input.GetMouseButton(0))
            heartTarget += growSpeed * Time.deltaTime;
        else
            heartTarget -= shrinkSpeed * Time.deltaTime;

        // Sınırla
        heartTarget = Mathf.Clamp(heartTarget, heartMin, heartMax);

        // Yumuşat
        heartCurrent = Mathf.Lerp(heartCurrent, heartTarget, Time.deltaTime * heartSmooth);
    }

    // ---------------- LIMIT ----------------
    private void UpdateLimit()
    {
        limitWaitTimer -= Time.deltaTime;

        // Hedefe ulaştıysa veya bekleme süresi bitti ise yeni hedef seç
        if (limitWaitTimer <= 0f || Mathf.Abs(limitCurrent - limitTarget) < 0.01f)
        {
            limitTarget = Random.Range(limitMin, limitMax);
            ResetLimitTimer();
        }

        // Hedefe doğru yavaşça ilerle
        limitCurrent = Mathf.MoveTowards(limitCurrent, limitTarget, limitMoveSpeed * Time.deltaTime);
        limitCurrent = Mathf.Clamp(limitCurrent, limitMin, limitMax);
    }

    // ---------------- CHECK GAME OVER ----------------
    private void CheckGameOver()
    {
        // Kalp minimuma düştü mü?
        if (heartTarget <= heartMin + 0.001f)
        {
            GameOver("Kalp 0.5'e düştü!");
            return;
        }

        // Kalp limiti geçti mi?
        if (heartCurrent > limitCurrent)
        {
            GameOver("Kalp limiti aştı!");
            return;
        }
    }

    // ---------------- HELPERS ----------------
    private void GameOver(string reason)
    {
        gameOver = true;
        Debug.Log($"GAME OVER: {reason}");
        onGameOver?.Invoke();
    }

    private void ResetLimitTimer()
    {
        limitWaitTimer = Random.Range(limitWaitRange.x, limitWaitRange.y);
    }

    private void ApplyScalesImmediate()
    {
        heartT.localScale = heartBaseScale * heartCurrent;
        limitT.localScale = limitBaseScale * limitCurrent;
    }
}
