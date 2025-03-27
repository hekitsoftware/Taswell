using UnityEngine;

public class GhostSpriteManager : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;
    public SpriteRenderer _mySpriteRenderer;
    public PlayerMovement _playerScript;

    public GameObject _echoObj;
    public Rigidbody2D _rb;

    private float timeBtwSpawns;
    public float startTimeBtwSpawns;

    Sprite _currentSprite;

    private bool _isAnyDashing;

    private void FixedUpdate()
    {
        _currentSprite = _spriteRenderer.sprite;
        _mySpriteRenderer.sprite = _currentSprite;

        if (_playerScript._isDashing || _playerScript._isAirDashing) { _isAnyDashing = true; }
        else { _isAnyDashing = false; }

        // Directly set the rotation of echoObj based on the player's direction
        if (_playerScript._isFacingRight)
        {
            _echoObj.transform.localRotation = Quaternion.identity; // No rotation
        }
        else
        {
            _echoObj.transform.localRotation = Quaternion.Euler(0, 180f, 0); // Flip the sprite
        }
    }

    private void Update()
    {
        if (_isAnyDashing)
        {
            if (timeBtwSpawns <= 0)
            {
                // Instantiate the echoObj and set its rotation
                GameObject echoInstance = Instantiate(_echoObj, transform.position, Quaternion.identity);

                // Set the rotation of the newly instantiated echoObj
                if (_playerScript._isFacingRight)
                {
                    echoInstance.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    echoInstance.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                }

                timeBtwSpawns = startTimeBtwSpawns;
            }
            else
            {
                timeBtwSpawns -= Time.fixedDeltaTime;
            }
        }
    }
}
