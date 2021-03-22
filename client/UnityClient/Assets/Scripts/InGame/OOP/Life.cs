using NetworkShared;
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

        public Func<Life, Vector3, Vector3> OnPositionChanging;
        public Action<Life> OnMove { get; set; }
        public Action<Life> OnStop { get; set; }
        public Action<Life> OnJump { get; set; }

        private IEnumerator CoMove()
        {
            Animator.SetBool("Walking", true);
            while (true)
            {
                var originPosition = new Vector3(transform.localPosition.x, transform.localPosition.y);

                var end = DateTime.Now;
                var diff = end - _moveDelta;
                _moveDelta = end;

                var velocityX = this.Speed;
                if (this.Direction == Direction.Left)
                    velocityX *= -1;

                var movedX = (velocityX * diff.Ticks) / 1000000;
                var newPosition = new Position(Mathf.Clamp(transform.localPosition.x + movedX, 0, 32), Mathf.Max(0, transform.localPosition.y)).ToVector3();

                if (OnPositionChanging != null)
                    newPosition = OnPositionChanging.Invoke(this, newPosition);

                transform.localPosition = Map.FixHeight(this, newPosition);
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
                JumpPower = JUMPING_POWER;
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


        public void Attacking()
        {
        }

        public void DamageEnd()
        {
        }

        public void DeadEnd()
        {
            if (Type == ObjectType.Mob)
            {
                EventAggregator.Instance.Publish(new GameEvent.DeadEnd { Sequence = Sequence });
            }
        }

        public override void OnDie()
        {
            Animator.Play("Dying");
        }

        public override void OnDamaged()
        {
            Animator.Play("Hurt");
        }
        public override void OnAttack()
        {
            //TODO 이거 엑셀에서 불러와야함. 임시 하드 코딩.
            Animator.Play(Type == NetworkShared.ObjectType.Character ? "Slashing" : "Throwing");
        }

        public override void OnHealed()
        {

        }
    }
}
