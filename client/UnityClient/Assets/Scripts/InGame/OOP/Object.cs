using NetworkShared;
using System;
using UnityEngine;

namespace Assets.Scripts.InGame.OOP
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Object : SpriteObject
    {
        public static float JUMPING_POWER = 2.0f;
        public static float SPEED = 1.0f;

        public float Speed => 1;

        private Direction _direction;
        public Direction Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                SpriteRenderer.flipX = _direction == Direction.Left;
            }
        }
        public abstract ObjectType Type { get; }
        public BoxCollider2D BoxCollider2D { get; private set; }

        public bool IsGround
        {
            get
            {
                var y = BoxCollider2D.bounds.min.y - 0.1f;
                return Map.Blocked(new Vector2(BoxCollider2D.bounds.min.x, y)) ||
                    Map.Blocked(new Vector2(BoxCollider2D.bounds.max.x, y));
            }
        }

        public int Sequence { get; set; }
        public string Name { get; set; }

        public virtual bool Moving { get; } = false;

        public Action<Object> OnJumpEnd { get; set; }
        public Action<Object> OnJumpStart { get; set; }

        public LayerMask GroundLayer;

        public TileMap Map { get; set; }

        protected float JumpPower = 0.0f;

        public void Awake()
        {
            OnAwake();

            BoxCollider2D = GetComponent<BoxCollider2D>();
        }

        protected bool Raycast(Vector2 position, Vector2 direction, float distance = 0.5f)
        {
            Debug.DrawRay(position, direction * distance, Color.green);

            var raycast = Physics2D.Raycast(position, direction, distance, GroundLayer);
            return raycast.collider != null;
        }

        public void Update()
        {
            if (JumpPower > 0.0f)
            {
                var next = new Vector3(transform.position.x, transform.position.y);
                var min = Math.Min(0.05f, JumpPower);

                next.y += min;
                JumpPower -= min;
                transform.position = next;
            }
            else if(IsGround == false)
            {
                var next = new Vector3(transform.position.x, transform.position.y);
                next.y -= 0.05f;
                transform.position = Map.FixHeight(this, next);

                if (IsGround)
                {
                    this.OnJumpEnd?.Invoke(this);
                }
            }
        }
    }
}
