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
    
    public void Move(float direction, int currentTick) {
        _currentTick = currentTick;
        
        float velocityX = direction * _moveSpeed;

        _rb.linearVelocityX = velocityX;

        _clientMovementDatas[currentTick % k_buffer_size] = new MovementData {
            tick = currentTick,
            movementDirection = direction,
            positionX = _transform.position.x
        };


        if (currentTick < 2) return;

        MoveServerRPC(_clientMovementDatas[currentTick % k_buffer_size],
                        _clientMovementDatas[(_currentTick - 1) % k_buffer_size]);

    }

    [ServerRpc]
    void MoveServerRPC(MovementData currentMovementData, MovementData lastMovementData) {
        float startPosition = _transform.position.x;

        float moveVector = lastMovementData.movementDirection * _moveSpeed;
        Physics.simulationMode = SimulationMode.Script;
        _transform.position = new Vector2(lastMovementData.positionX, _transform.position.y);
        _rb.linearVelocityX = moveVector;
        Physics.Simulate(Time.fixedDeltaTime);
        Vector2 correctPosition = _transform.position;
        _transform.position = new Vector2(startPosition, _transform.position.y);
        Physics.simulationMode = SimulationMode.FixedUpdate;

        if (Vector2.Distance(correctPosition, new Vector2(currentMovementData.positionX, _transform.position.y)) > _maxPositionError) {
            ReconciliateClientRPC(currentMovementData.tick);
        }
    }

    [ClientRpc]
    void ReconciliateClientRPC(int activationTick) {
        float correctPosition = _clientMovementDatas[(activationTick - 1) % k_buffer_size].positionX;

        Physics.simulationMode = SimulationMode.Script;
        while (activationTick <= _currentTick) {
            float moveVector = _clientMovementDatas[(activationTick - 1) % k_buffer_size].movementDirection * _moveSpeed;
            _transform.position = new Vector2(correctPosition, _transform.position.y);
            _rb.linearVelocityX = moveVector;
            Physics.Simulate(Time.fixedDeltaTime);
            correctPosition = _transform.position.x;
            _clientMovementDatas[activationTick % k_buffer_size].positionX = correctPosition;
            activationTick++;
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

        _transform.position = new Vector2(correctPosition, _transform.position.y);
    }
    public void PlayerIsRespawning(int currentTick, Vector3 position) {
        _currentTick = currentTick;
        _clientMovementDatas[currentTick % k_buffer_size] = new MovementData() {
            tick = currentTick,
            positionX = position.x,
            isRespawning = true,
        };
        _clientMovementDatas[(_currentTick - 1) % k_buffer_size] = new MovementData {
            tick = currentTick,
            positionX = position.x,
            isRespawning = true,
        };
        RespawnServerRPC(_clientMovementDatas[currentTick % k_buffer_size]);
    }

    [ServerRpc]
    void RespawnServerRPC(MovementData currentMovementData) {
        if (currentMovementData.isRespawning) {
            _transform.position = new Vector2(currentMovementData.positionX, _transform.position.y);
        }
    }
}