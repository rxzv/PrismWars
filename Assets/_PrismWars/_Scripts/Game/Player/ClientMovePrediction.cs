using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using Unity.Netcode;
public class ClientMovementPrediction {
    float _moveSpeed;
    Animator _animator;
    Rigidbody2D _rb;
    Transform _transform;
    
    float _maxPositionError;
    
    int _currentTick;
    
    const int k_buffer_size = 1024;
    MovementData[] _clientMovementDatas = new MovementData[k_buffer_size];

    public ClientMovementPrediction(Transform transform, float moveSpeed, Animator animator, Rigidbody2D rb, float maxPositionError) {
        _moveSpeed = moveSpeed;
        _animator = animator;
        _rb = rb;
        _maxPositionError = maxPositionError;
        _transform = transform;
    }

    public void Move(Vector2 direction, int currentTick) {
        _currentTick = currentTick;

        direction.y = 0;
        
        Vector2 moveVector = direction.normalized * _moveSpeed;

        // _animator.SetFloat("Speed", moveVector.magnitude);
        _rb.linearVelocity = moveVector;

        // if (moveVector != Vector2.zero) {
        //     _animator.SetFloat("Horizontal", moveVector.normalized.x);
        //     _animator.SetFloat("Vertical", moveVector.normalized.y);
        // }

        _clientMovementDatas[currentTick % k_buffer_size] = new MovementData {
            tick = currentTick,
            movementDirection = direction,
            position = _transform.position
        };


        if (currentTick < 2) return;

        MoveServerRPC(_clientMovementDatas[currentTick % k_buffer_size],
                        _clientMovementDatas[(_currentTick - 1) % k_buffer_size]);

    }

    [ServerRpc]
    private void MoveServerRPC(MovementData currentMovementData, MovementData lastMovementData)
    {
        Vector2 startPosition = _transform.position;

        Vector2 moveVector = lastMovementData.movementDirection.normalized * _moveSpeed;
        Physics.simulationMode = SimulationMode.Script;
        _transform.position = lastMovementData.position;
        _rb.linearVelocity = moveVector;
        Physics.Simulate(Time.fixedDeltaTime);
        Vector2 correctPosition = _transform.position;
        _transform.position = startPosition;
        Physics.simulationMode = SimulationMode.FixedUpdate;

        if (Vector2.Distance(correctPosition, currentMovementData.position) > _maxPositionError)
        {
            Debug.Log("Position is off");

            ReconciliateClientRPC(currentMovementData.tick);

        }
    }

    [ClientRpc]
    private void ReconciliateClientRPC(int activationTick)
    {
        Vector2 correctPosition = _clientMovementDatas[(activationTick - 1) % k_buffer_size].position;

        Physics.simulationMode = SimulationMode.Script;
        while (activationTick <= _currentTick)
        {
            Vector2 moveVector = _clientMovementDatas[(activationTick - 1) % k_buffer_size].movementDirection.normalized * _moveSpeed;
            _transform.position = correctPosition;
            _rb.linearVelocity = moveVector;
            Physics.Simulate(Time.fixedDeltaTime);
            correctPosition = _transform.position;
            _clientMovementDatas[activationTick % k_buffer_size].position = correctPosition;
            activationTick++;
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

        _transform.position = correctPosition;
    }
}