using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLocomotionManager : CharacterLocomotionManager
{
    private float _verticalMovement;
    private float _horizontalMovement;
    private float _moveAmount;

    private Vector3 _moveDirection;
    private Vector3 _targetRotationDireciton;
    [SerializeField] private float _runningSpeed = 5;
    [SerializeField] private float _walkingSpeed = 3;
    [SerializeField] private float _rotationSpeed = 15;
    [SerializeField] private PlayerManager _playerManager;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        EventSystem.LocomotionAction += HandleGroundedMovement;
        EventSystem.LocomotionAction += HandleRotation;
    }
    private void OnDisable()
    {
        EventSystem.LocomotionAction -= HandleGroundedMovement;
        EventSystem.LocomotionAction -= HandleRotation;
    }

    protected override void Update()
    {
        base.Update();

        if (_playerManager.IsOwner)
        {
            _playerManager.characterNetworkManager.animatorVerticalValue.Value = _verticalMovement;
            _playerManager.characterNetworkManager.animatorHorizontalValue.Value = _horizontalMovement;
            _playerManager.characterNetworkManager.networkMoveAmount.Value = _moveAmount;
        }
        else
        {
            _verticalMovement = _playerManager.characterNetworkManager.animatorVerticalValue.Value;
            _horizontalMovement = _playerManager.characterNetworkManager.animatorHorizontalValue.Value;
            _moveAmount = _playerManager.characterNetworkManager.networkMoveAmount.Value;

            //_playerManager.playerAnimatorManager.UpdateAnimatorMovementParameters(0, _moveAmount);

            EventSystem.UpdateFloatAnimatorParameterAction?.Invoke(_playerManager.networkID, "Horizontal", 0);
            EventSystem.UpdateFloatAnimatorParameterAction?.Invoke(_playerManager.networkID, "Vertical", _moveAmount);

            //HORIZONTAL WILL USE WHEN LOCKED ON
        }
    }
    public void HandleAllMovement()
    {
        //HandleGroundedMovement();
        //HandleRotation();
    }

    private void GetMovementValues()
    {
        _verticalMovement = PlayerInputManager.Instance.VerticalInput;
        _horizontalMovement = PlayerInputManager.Instance.HorizontalInput;
        _moveAmount = PlayerInputManager.Instance.MoveAmount;
    }

    private void HandleGroundedMovement(ulong id, float verticalMovement, float horizontalMovement, float moveAmount)
    {
        //GetMovementValues();
        if (id == _playerManager.networkID)
        {
            _verticalMovement = verticalMovement;
            _horizontalMovement = horizontalMovement;
            _moveAmount = moveAmount;

            _moveDirection = PlayerCamera.Instance.transform.forward * _verticalMovement;
            _moveDirection += PlayerCamera.Instance.transform.right * _horizontalMovement;
            _moveDirection.Normalize();
            _moveDirection.y = 0;

            if (PlayerInputManager.Instance.MoveAmount <= 0.5)
            {
                _playerManager.characterController.Move(_moveDirection * _walkingSpeed * Time.deltaTime);
            }
            else if (PlayerInputManager.Instance.MoveAmount > 0.5)
            {
                _playerManager.characterController.Move(_moveDirection * _runningSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleRotation(ulong id, float verticalMovement, float horizontalMovement, float moveAmount)
    {
        if (id == _playerManager.networkID)
        {
            _targetRotationDireciton = Vector3.zero;
            _targetRotationDireciton = PlayerCamera.Instance.CameraObject.transform.forward * _verticalMovement;
            _targetRotationDireciton += PlayerCamera.Instance.CameraObject.transform.right * _horizontalMovement;
            _targetRotationDireciton.Normalize();
            _targetRotationDireciton.y = 0;

            if (_targetRotationDireciton == Vector3.zero)
            {
                _targetRotationDireciton = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(_targetRotationDireciton);
            Quaternion rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            transform.rotation = rotation;
        }
    }
}
