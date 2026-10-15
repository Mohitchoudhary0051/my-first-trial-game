using UnityEngine;

/// <summary>
/// 3D Coin Collectible Script
/// 
/// SETUP INSTRUCTIONS FOR STUDENTS:
/// 1. Create a 3D Object (e.g. GameObject > 3D Object > Cylinder or Sphere) — name it "Coin".
/// 2. Scale it to look like a coin (e.g., Scale X:0.6, Y:0.1, Z:0.6).
/// 3. In the Inspector, check "Is Trigger" on the Collider component.
/// 4. Attach this script to the Coin GameObject.
/// 5. Make sure your Player GameObject is tagged as "Player" (or has PlayerController attached).
/// 6. (Optional) Drag into Project window to save as a Prefab and duplicate around your map!
/// </summary>
using UnityEngine;
public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            Destroy(gameObject);
        }
    }
}