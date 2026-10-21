using UnityEngine;

namespace ConNuoc.Player
{
    /// <summary>
    /// Minimal 4-direction top-down movement for the prototype. Requires a Rigidbody2D
    /// (set to Kinematic or Dynamic with gravity scale 0) and a Collider2D on the same object.
    /// Uses the legacy Input Manager (Input.GetAxisRaw) so it works with no extra package setup.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;

        private Rigidbody2D _rb;
        private Vector2 _input;
        private Vector2 _facing = Vector2.down;

        public Vector2 Facing => _facing;
        public bool IsMoving => _input.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        private void Update()
        {
            // Read input every frame; apply movement in FixedUpdate for consistent physics.
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            // Favour 4-direction movement (no diagonal) to match a simple top-down tileset;
            // delete this block if you want free 8-direction movement instead.
            if (Mathf.Abs(x) > Mathf.Abs(y)) y = 0f;
            else x = 0f;

            _input = new Vector2(x, y).normalized;

            if (IsMoving)
            {
                _facing = _input;
            }
        }

        private void FixedUpdate()
        {
            _rb.MovePosition(_rb.position + _input * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
