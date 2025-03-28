using UnityEngine;

public class GhostSpriteManager : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;
    public SpriteRenderer _mySpriteRenderer;
    public PlayerMovement _playerScript;

    public GameObject _echoObj;
    public Rigidbody2D _rb;

    public bool _isHazy = false;

    private float timeBtwSpawns;
    public float startTimeBtwSpawns;

    Sprite _currentSprite;

    private bool _isAnyDashing;

    private void FixedUpdate()
    {
        _currentSprite = _spriteRenderer.sprite;
        _mySpriteRenderer.sprite = _currentSprite;

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
        if (_isHazy) { Haze(); }
        else { _mySpriteRenderer.color = new Color(1f, 0.643f, 0f); }
        //#FFA400

        DebugInputs();

        if (_rb.linearVelocityX != 0 || _rb.linearVelocityY > 0)
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

    private void Haze()
    {
        float hue = Mathf.PingPong(Time.time * 0.1f, 1f); // Change the speed of color transition with the multiplier
        _mySpriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f); // Saturation and Value set to 1 for vibrant colors
    }

    private void DebugInputs()
    {
        if (Input.GetKey(KeyCode.F4)) { _isHazy = true; }
        if (Input.GetKey(KeyCode.F5)) { _isHazy = false; }
    }
}
