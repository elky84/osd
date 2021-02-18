using NetworkShared;
using System;
using UnityEngine;


namespace Assets.Scripts.InGame.OOP
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Object : SpriteObject
    {
        public static float JUMPING_POWER = 30.0f;
        public static float SPEED = 1.0f;

        public float Speed => 1;

        public Direction Direction;
        public abstract ObjectType Type { get; }

        public int Sequence { get; set; }
        public string Name { get; set; }
        public bool IsGround { get; private set; }
        public virtual bool Moving { get; } = false;

        public Action<Object, Collision2D> OnCollisionEnter { get; set; }
        public Action<Object, Collision2D> OnCollisionExit { get; set; }

        public void Awake()
        {
            OnAwake();
        }

        public void RemoveRigidBody()
        {
            var collider2D = this.gameObject.GetComponent<BoxCollider2D>();
            if (collider2D != null)
            {
                collider2D.enabled = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            this.OnCollisionEnter?.Invoke(this, collision);
        }


        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.transform.tag == "Tile")
            {
                this.IsGround = true;
            }
        }

        //땅에서 탈출한 시점에 실행됨
        private void OnCollisionExit2D(Collision2D collision)
        {
            var before = this.IsGround;
            if (collision.transform.tag == "Tile")
            {
                this.IsGround = false;
            }

            if (before && !this.IsGround)
                this.OnCollisionExit?.Invoke(this, collision);
        }
    }
}
