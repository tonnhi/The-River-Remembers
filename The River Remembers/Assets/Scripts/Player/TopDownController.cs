using UnityEngine;

namespace ConNuoc.Player
{
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
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

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
