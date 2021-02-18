using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.InGame.OOP
{
    public abstract class Life : Object
    {
        private DateTime _moveDelta;

        public Text NickName;
        private IEnumerator MoveCoroutine { get; set; }

        private bool _moving;
        public override bool Moving => _moving;

        public Action<Life> OnMove { get; set; }
        public Action<Life> OnStop { get; set; }
        public Action<Life> OnJump { get; set; }

        private IEnumerator CoMove()
        {
            Animator.SetBool("Walking", true);
            while (true)
            {
                var end = DateTime.Now;
                var diff = end - _moveDelta;
                _moveDelta = end;

                var velocityX = this.Speed;
                if (this.Direction == Direction.Left)
                    velocityX *= -1;
                var movedX = (velocityX * diff.Ticks) / 1000000;

                transform.localPosition = new Position(transform.localPosition.x + movedX, transform.localPosition.y).ToVector3();
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void Move(Direction direction)
        {
            if (Moving && this.Direction == direction)
                return;

            this.Direction = direction;
            Stop();

            this._moving = true;
            _moveDelta = DateTime.Now;

            OnMove?.Invoke(this);

            MoveCoroutine = CoMove();
            StartCoroutine(MoveCoroutine);
        }

        public void Jump()
        {
            if (IsGround)
            {
                var rigidBody2D = this.gameObject.GetComponent<Rigidbody2D>();
                rigidBody2D.AddForce(Vector3.up * JUMPING_POWER);

                OnJump?.Invoke(this);
            }
        }

        public void Stop()
        {
            try
            {
                this._moving = false;
                if (MoveCoroutine != null)
                {
                    StopCoroutine(MoveCoroutine);
                    MoveCoroutine = null;
                    Animator.SetBool("Walking", false);

                    OnStop?.Invoke(this);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
