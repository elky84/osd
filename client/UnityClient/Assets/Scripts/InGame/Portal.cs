using UnityEngine;

namespace Assets.Scripts.InGame
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Portal : MonoBehaviour
    {
        public static Portal Create(Vector2 position, Transform parent)
        {
            var gameObj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Portal") as GameObject, position, Quaternion.identity, parent);
            return gameObj.GetComponent<Portal>();
        }
    }
}
