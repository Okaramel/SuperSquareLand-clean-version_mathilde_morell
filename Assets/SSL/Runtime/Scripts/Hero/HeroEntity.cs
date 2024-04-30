using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementsSettings")]
    [SerializeField] private HeroHorizontalMovementSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementSettings _airHorizontalMovementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;
    private float _CountDownDash = -1f;

    [Header("Dash")]
    [SerializeField] private HeroDashSettings _dashSettings;
    private float _moveDash = 0f;


    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Jump")]
    [SerializeField] private HeroJumpSettings _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }

    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;


    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround { get; private set; } = false;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void FixedUpdate()
    {

        _ApplyGroundDetection();

        HeroHorizontalMovementSettings horizontalMovementSettings = _GetCurrentHorizontalMovementSettings();
        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack(horizontalMovementSettings);
        } else
        {
            _UpdateHorizontalSpeed(horizontalMovementSettings);
            _ChangeOrientFromHorizontalMovement();
        }

        if (IsJumping)
        {
            _UpdateJump();
        } else
        {
            if(!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            } else
            {
                _ResetVerticalSpeed();
            }
        }

        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();

       /* if (_CountDownDash > 0)
        {
            _UpdateSpeedDash();
        }*/
    }
    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0f) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }

    private void _Accelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed > settings.speedMax)
        {
            _horizontalSpeed = settings.speedMax;
        }
    }

    private void _Decelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }

    private void _TurnBack(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFrictions * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }
        else
        {
            _Decelerate(settings);
        }
    }

    public void _StartDash()
    {
        _CountDownDash = _dashSettings.duration;
    }

   /* private void _UpdateSpeedDash(HeroHorizontalMovementSettings settings)
    {
        if (_CountDownDash > 0f)
        {
            _CountDownDash -= Time.fixedDeltaTime;
            _horizontalSpeed = _dashSettings.speed;
        } else
        {
            _horizontalSpeed = settings.speedMax;
        }
    } */

    private void _ApplyFallGravity()
    {
        _verticalSpeed -= _fallSettings.fallGrativy * Time.fixedDeltaTime;
        if (_verticalSpeed < -_fallSettings.fallSpeedMax)
        {
            _verticalSpeed = -_fallSettings.fallSpeedMax;
        }
    }

    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }

    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }
    public bool IsJumping => _jumpState != JumpState.NotJumping;    

    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGrativy * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings.jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings.jumpSpeed;
        } else
        {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_jumpFallSettings);
        } else
        {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
        }
    }

    private void _UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;

            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;
    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings.jumpMinDuration;

    private HeroHorizontalMovementSettings _GetCurrentHorizontalMovementSettings()
    {
        if (IsTouchingGround)
        {
            return _groundHorizontalMovementsSettings;
        } else
        {
            return _airHorizontalMovementsSettings;
        }
    }

/*  private HeroHorizontalMovementSettings _GetCurrentHorizontalMovementsSettings() {
 *  return IsTouchingGround ? _groundHorizontalMovementsSettings : _airHorizontalMovementsSettings}*/

    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }
    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        if (IsTouchingGround)
        {
            GUILayout.Label("OnGround");
        } else
        {
            GUILayout.Label("InAir");
        }
        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"MoveDash = {_moveDash}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");
        GUILayout.Label($"istouchingground {IsTouchingGround}");
        GUILayout.EndVertical();
    }
}