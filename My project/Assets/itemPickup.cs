using UnityEngine;

public class itemPickup : MonoBehaviour
{
    [Tooltip("Name shown in the pickup notification")]
    public string itemname = "";

    [Tooltip("Tick this only on the key. Other items won't give the key.")]
    public bool isKey = false;

    [Tooltip("Object to hide when picked up. Leave empty to hide this object.")]
    public GameObject item;

    // Handles item pickups and notifications (besides weapons)
    Player_Man ui;
    bool pickedUp = false;

    void Start()
    {
        ui = FindFirstObjectByType<Player_Man>();
        if (ui == null)
            Debug.LogWarning("itemPickup: no Player_Man found in the scene.", this);

        if (item == null)
            item = gameObject;
    }
    public void Update()
    {
        if (Player_Man.haskey == true)
            item.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // TEMP DEBUG: remove once pickups work
        Debug.Log($"[itemPickup] '{name}' touched by '{other.name}' (tag: {other.tag}, pickedUp: {pickedUp})", this);

        // Ignore anything that isn't the player, and ignore repeat hits after pickup
        if (pickedUp || !other.CompareTag("Player"))
            return;

        pickedUp = true;   // stops a double pickup if the player has more than one collider

        Debug.Log($"Picked up: {itemname}", this);

        if (ui != null)
            ui.textchange(itemname);   // pickup notification

        if (isKey)
            Player_Man.haskey = true;

        item.SetActive(false);
    }
}