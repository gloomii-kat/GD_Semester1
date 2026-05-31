using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Attach this to a persistent GameObject (e.g. "GameManager").
/// Each room's exit door needs a DoorBlocker component (see below).
/// </summary>
public class RoomGameManager : MonoBehaviour
{
    [Header("Rooms (in order)")]
    public RoomData[] rooms;
    private int currentRoomIndex = 0;

    [Header("Door Blockers")]
    [Tooltip("One DoorBlocker per room exit, in the same order as rooms[]")]
    public DoorBlocker[] doorBlockers;

    [Header("UI Panel")]
    public GameObject roomInfoPanel;
    public TextMeshProUGUI roomNameText;       // Left side of panel
    public TextMeshProUGUI kidsProgressText;   // Right side of panel
    public TextMeshProUGUI warningText;        // Shown when player tries to leave early

    [Header("Warning Settings")]
    public float warningDuration = 2f;
    public string warningMessage = "Scare all the kids first!";

    private RoomData CurrentRoom => rooms[currentRoomIndex];
    private Coroutine warningCoroutine;

    void Start()
    {
        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError("RoomGameManager: No rooms assigned!");
            return;
        }

        // Lock all doors except maybe none (first room door stays locked too)
        foreach (DoorBlocker blocker in doorBlockers)
            if (blocker != null) blocker.Lock();

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        LoadRoom(0);
    }

    void Update()
    {
        if (rooms == null || currentRoomIndex >= rooms.Length) return;

        UpdateUI();

        // Check if all kids in current room are scared
        RoomData room = CurrentRoom;
        int total = room.GetChildrenInRoom().Count;
        int scared = room.GetScaredChildren().Count;

        if (total > 0 && scared >= total)
        {
            UnlockCurrentDoor();
        }
    }

    void LoadRoom(int index)
    {
        if (index >= rooms.Length) return;

        currentRoomIndex = index;
        Debug.Log($"RoomGameManager: Entered room {rooms[index].roomName}");

        // Lock this room's door when entering
        if (index < doorBlockers.Length && doorBlockers[index] != null)
            doorBlockers[index].Lock();

        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentRoomIndex >= rooms.Length) return;

        RoomData room = CurrentRoom;
        int total = room.GetChildrenInRoom().Count;
        int scared = room.GetScaredChildren().Count;

        if (roomNameText != null)
            roomNameText.text = room.roomName;

        if (kidsProgressText != null)
        {
            kidsProgressText.text = scared >= total && total > 0
                ? "All kids scared! "
                : $"{scared} / {total}\nkids scared";
        }
    }



    void UnlockCurrentDoor()
    {
        if (currentRoomIndex < doorBlockers.Length && doorBlockers[currentRoomIndex] != null)
        {
            if (doorBlockers[currentRoomIndex].IsLocked)
            {
                doorBlockers[currentRoomIndex].Unlock();
                Debug.Log($"Door unlocked for room: {CurrentRoom.roomName}");
            }
        }
    }

    // Called by DoorBlocker when player tries to walk through a locked door
    public void OnPlayerTriedLockedDoor()
    {
        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(ShowWarning());
    }

    // Called by DoorBlocker when player successfully passes through an unlocked door
    public void OnPlayerPassedThroughDoor(int doorIndex)
    {
        int nextRoom = doorIndex + 1;
        if (nextRoom < rooms.Length)
            LoadRoom(nextRoom);
        else
            Debug.Log("All rooms complete!");
    }

    IEnumerator ShowWarning()
    {
        if (warningText == null) yield break;

        warningText.text = warningMessage;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningText.gameObject.SetActive(false);
    }

    public int GetTotalScaredChildren()
    {
        if (rooms == null || currentRoomIndex >= rooms.Length) return 0;
        return rooms[currentRoomIndex].GetScaredChildren().Count;
    }
}
