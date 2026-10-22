using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RubbishTarget : MonoBehaviour
{
    private bool _collected;

    public bool Collect()
    {
        if (_collected) return false;

        _collected = true;
        Destroy(gameObject);
        return true;
    }
}