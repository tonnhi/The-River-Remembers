using UnityEngine;

namespace ConNuoc.Player
{
    /// <summary>
    /// Simple smoothed top-down camera follow. Put on the Main Camera, assign the player
    /// transform, leave Z alone (camera stays at its own -10 or whatever your 2D setup uses).
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
