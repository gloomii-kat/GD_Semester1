using UnityEngine;

public class ToiletScare : MonoBehaviour
{
    public GameObject toiletText;       // "Press E" prompt
    private AudioManager AudioManager;

    [Header("Child Reference")]
    public GameObject childObject;

    private bool playerInRange = false;
    private bool childInRange = false;
    private bool isScaring = false;

    private void Awake()
    {
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        if (toiletText != null)
            toiletText.SetActive(false);

        if (childObject == null)
        {
            FindChildByTag();
        }
    }

    void Update()
    {
        if (childObject == null)
        {
            FindChildByTag();
            return;
        }

        bool bothInRange = playerInRange && childInRange;

        if (toiletText != null)
        {
            toiletText.SetActive(bothInRange && !isScaring);
        }

        if (bothInRange && Input.GetKeyDown(KeyCode.E) && !isScaring)
        {
            StartScare();
        }
    }

    void FindChildByTag()
    {
        GameObject found = GameObject.FindGameObjectWithTag("LittleGirl");
        if (found != null)
        {
            childObject = found;
            Debug.Log($"ToiletScare found child: {childObject.name}");
        }
    }

    void StartScare()
    {
        if (childObject == null)
        {
            Debug.LogError("ToiletScare: childObject is null!");
            ResetScare();
            return;
        }

        isScaring = true;

        if (toiletText != null)
            toiletText.SetActive(false);

        ChildAI childAI = childObject.GetComponent<ChildAI>();
        if (childAI != null)
        {
            childAI.Scare();
            Debug.Log("Toilet scared the child!");
        }
        else
        {
            Debug.LogError("ToiletScare: ChildAI component not found!");
        }

        if (AudioManager != null)
        {
            AudioManager.PlaySFX(AudioManager.DoorBanging);
        }

        Invoke(nameof(ResetScare), 1f);
    }

   

    void ResetScare()
    {
        isScaring = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered toilet trigger zone");
        }
        if (other.CompareTag("LittleGirl"))
        {
            childInRange = true;
            if (childObject == null || childObject != other.gameObject)
            {
                childObject = other.gameObject;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (toiletText != null)
                toiletText.SetActive(false);
        }
        if (other.CompareTag("LittleGirl"))
        {
            childInRange = false;
        }
    }

    void OnDrawGizmos()
    {
        // Draw a small sphere to show trigger status
        Gizmos.color = (playerInRange && childInRange && !isScaring) ? Color.green :
                       (playerInRange && childInRange && isScaring) ? Color.yellow : Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up, 0.3f);
    }
}
