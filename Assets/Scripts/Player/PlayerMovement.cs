using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float MaxSpeed;
    public float Acceleration;
    public float JumpForce;
    public AudioClip JumpSFX;
    public float JumpSFXVolume;
    public AudioClip LandSFX;
    public float LandSFXVolume;
    public bool Grounded;
    public bool Stunned;
    public float JumpTime;

    private bool _isJumping;
    private float _jumpTimeCounter;
    private float _moveSpeed;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private AudioSource _audio_source;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _audio_source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Stunned)
        {
            return;
        }

        _animator.SetFloat("VerticalSpeed", _rigidbody.velocity.y);

        #region Movement
        if (Input.GetKey("a"))
        {
            _moveSpeed = _moveSpeed > 0 ? 0 : _moveSpeed - Acceleration;
            _moveSpeed = _moveSpeed < -MaxSpeed ? -MaxSpeed : _moveSpeed;

            _rigidbody.velocity = new Vector2(_moveSpeed * Time.fixedDeltaTime, _rigidbody.velocity.y);
            transform.eulerAngles = new Vector3(0, 180, 0);
            _animator.SetBool("Running", true);
        }
        else if (Input.GetKey("d"))
        {
            _moveSpeed = _moveSpeed < 0 ? 0 : _moveSpeed + Acceleration;
            _moveSpeed = _moveSpeed > MaxSpeed ? MaxSpeed : _moveSpeed;

            _rigidbody.velocity = new Vector2(_moveSpeed * Time.fixedDeltaTime, _rigidbody.velocity.y);
            transform.eulerAngles = new Vector3(0, 0, 0);
            _animator.SetBool("Running", true);
        }
        else
        {
            _moveSpeed = 0;
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            _animator.SetBool("Running", false);
        }
        #endregion

        #region Jumping
        if (Input.GetKeyDown(";"))
        {
            if (Grounded == true)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, JumpForce);
                Grounded = false;
                _jumpTimeCounter = JumpTime;
                _isJumping = true;
                _animator.SetBool("Running", false);
                _animator.SetBool("Grounded", false);

                _audio_source.volume = JumpSFXVolume;
                _audio_source.PlayOneShot(JumpSFX);
            }
        }
        if (Input.GetKey(";") && !Grounded)
        {
            if (_jumpTimeCounter > 0 && _isJumping)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, JumpForce);
                _jumpTimeCounter -= Time.deltaTime;
            } else
            {
                _isJumping = false;
            }
        }

        if (Input.GetKeyUp(";"))
        {
            _isJumping = false;
        }
        #endregion
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Room" || collision.tag == "Platform")
        {
            if (Grounded) //avoid case of sound playing when moving between rooms
                return;
            Grounded = true;
            _animator.SetBool("Grounded", true);
            _audio_source.volume = LandSFXVolume;
            _audio_source.PlayOneShot(LandSFX);
        }
    }

    public void Stop() //Stops player horizontal movement
    {
        _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
    }
}
