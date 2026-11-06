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

    void OnTriggerEnter(Collider other)
    {
        if (pickedUp || !other.CompareTag("Player")) return;
        pickedUp = true;   // stops a double pickup if the player has more than one collider

        Debug.Log($"Picked up: {itemname}", this);

        if (ui != null)
            ui.textchange(itemname);   // pickup notification

        if (isKey)
            Player_Man.haskey = true;

        item.SetActive(false);
    }
}