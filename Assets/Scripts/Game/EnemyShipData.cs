using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShip", menuName = "ShotShotShot/Enemy Ship")]
public class EnemyShipData : ScriptableObject
{
    [SerializeField] private Sprite m_sprite;
    [SerializeField, Min(0f)] private float m_moveSpeed = 1.2f;
    [SerializeField, Min(0.1f)] private float m_attackInterval = 2.3f;

    public Sprite Sprite => m_sprite;
    public float MoveSpeed => m_moveSpeed;
    public float AttackInterval => m_attackInterval;
}
