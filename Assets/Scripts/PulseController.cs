using Unity.VisualScripting;
using UnityEngine;

public class PulseController : MonoBehaviour
{
    [Header("Mouse Kontrollü (Hearth)")]
    [SerializeField] private float growSpeed = 2f;
    [SerializeField] private float shrinkSpeed = 2f;
    [SerializeField] private float smooth = 5f;
    [SerializeField] private GameObject Hearth;          // Mouse ile kontrol edilen

    [Header("Rastgele (limit)")]
    [SerializeField] private GameObject limit;           // Kendi kendine nefes alan
    [SerializeField] private Vector2 globalMinMax = new Vector2(0.3f, 10f); // her döngü için genel ölçek çarpanı aralığı
    [SerializeField] private float speed = 1f;           // random hedefe yaklaşma hızı (küçük = yavaş)
    [SerializeField] private Vector2 waitRange = new Vector2(0.5f, 2f); // hedefe varınca bekleme aralığı (sn)

    // --- Hearth (mouse) state ---
    private Transform heartT;
    private Vector3 heartBaseScale;
    private float heartCurrent = 0.1f;   // baseScale çarpanı
    private float heartTarget = 1f;

    // --- limit (random) state ---
    private Transform limitT;
    private Vector3 limitBaseScale;
    private float randCurrent = 1f;    // baseScale çarpanı
    private float randTarget = 1f;
    private float waitTimer = 0f;
    private float localMin, localMax;  // her döngüde değişen min/max

    void Awake()
    {
        heartT = Hearth != null ? Hearth.transform : transform;
        limitT =  limit  != null ? limit.transform  : transform;

        heartBaseScale = heartT.localScale;
        limitBaseScale = limitT.localScale;

        // İlk random aralığı ve hedefi kur
        SetNewRandomRange();
        PickNewTarget();

        // Başlangıçları mevcut boyuta göre ayarla (çarpan)
        heartCurrent = 1f;
        randCurrent  = 1f;
    }

    void Update()
    {
        UpdateHeart();   // Mouse kontrollü
        UpdateRandom();  // Kendi kendine nefes
    }

    // ---------------- HEART (Mouse kontrollü) ----------------
    private void UpdateHeart()
    {
        if (Input.GetMouseButton(0))
            heartTarget += growSpeed * Time.deltaTime;   // büyü
        else
            heartTarget -= shrinkSpeed * Time.deltaTime; // küçül

        // Negatif/çok küçük değeri engelle (çarpan mantığı)
        heartTarget = Mathf.Max(heartTarget, 0.01f);

        // Smooth yaklaşım
        heartCurrent = Mathf.Lerp(heartCurrent, heartTarget, Time.deltaTime * smooth);

        // Uygula
        heartT.localScale = heartBaseScale * heartCurrent;
    }

    // ---------------- LIMIT (Rastgele nefes) ----------------
    private void UpdateRandom()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        randCurrent = Mathf.Lerp(randCurrent, randTarget, Time.deltaTime * speed);
        limitT.localScale = limitBaseScale * randCurrent;

        if (Mathf.Abs(randCurrent - randTarget) < 0.02f)
        {
            SetNewRandomRange();              // her döngüde yeni min/max
            PickNewTarget();                  // o aralıkta yeni hedef
            waitTimer = Random.Range(waitRange.x, waitRange.y);
        }
    }

    private void SetNewRandomRange()
    {
        // global aralıktan iki rastgele değer seç → küçük olan min, büyük olan max
        float a = Random.Range(globalMinMax.x, globalMinMax.y);
        float b = Random.Range(globalMinMax.x, globalMinMax.y);
        localMin = Mathf.Min(a, b);
        localMax = Mathf.Max(a, b);

        // Güvenlik: çok küçük olmasın
        localMin = Mathf.Max(localMin, 0.01f);
    }

    private void PickNewTarget()
    {
        randTarget = Random.Range(localMin, localMax);
    }
}
