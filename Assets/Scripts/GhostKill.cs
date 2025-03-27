using UnityEngine;

public class GhostKill : MonoBehaviour
{
    public GameObject _self;
    public SpriteRenderer _spriteRenderer;
    private Sprite _currentSprite;

    public float _GhostTimer = 0.5f;

    private void Awake()
    {
        _self = gameObject;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentSprite = _spriteRenderer.sprite;
    }

    private void FixedUpdate()
    {
        _GhostTimer -= Time.deltaTime;

        if (_GhostTimer <= 0)
        {
            Destroy(_self);
        }
    }
}