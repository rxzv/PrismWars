using _PrismWars._Scripts.UI.Model;
using UnityEngine;
using Unity.Netcode;
public class ClientJumpPrediction {
    Animator _animator;
    Rigidbody2D _rb;
    Transform _transform;
    float _jumpForce;
    readonly string _groundLayerName;

    float _maxPositionError;
    
    int _currentTick;
    
    const int k_buffer_size = 1024;
    JumpingData[] _clientMovementDatas = new JumpingData[k_buffer_size];

    public ClientJumpPrediction(Transform transform, float jumpForce, Animator animator, Rigidbody2D rb, float maxPositionError, string groundLayerName = "Ground") {
        _animator = animator;
        _rb = rb;
        _maxPositionError = maxPositionError;
        _transform = transform;
        _groundLayerName = groundLayerName;
        _jumpForce = jumpForce;
    }

    public void Jump(int currentTick, bool isJumping) {
        _currentTick = currentTick;
        bool isGrounded = GroundCheck();
        
        if (isGrounded && isJumping)
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

        _clientMovementDatas[currentTick % k_buffer_size] = new JumpingData {
            tick = currentTick,
            positionY = _transform.position.y,
            isJumping = isJumping,
            isGrounded = isGrounded,
        };

        if (currentTick < 2) return;

        MoveServerRPC(_clientMovementDatas[currentTick % k_buffer_size],
                        _clientMovementDatas[(_currentTick - 1) % k_buffer_size]);

    }
    bool GroundCheck() {
        RaycastHit2D hit = Physics2D.Raycast(_rb.transform.position, Vector2.down);
        return hit.collider.IsTouchingLayers(LayerMask.GetMask(_groundLayerName));
    }
    
    [ServerRpc]
    void MoveServerRPC(JumpingData currentMovementData, JumpingData lastMovementData) {
        float startPosition = _transform.position.y;

        Physics.simulationMode = SimulationMode.Script;
        _transform.position = new Vector2(_transform.position.x, lastMovementData.positionY);
        
        if (lastMovementData.isJumping && lastMovementData.isGrounded)
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        
        Physics.Simulate(Time.fixedDeltaTime);
        Vector2 correctPosition = _transform.position;
        _transform.position = new Vector2(_transform.position.x, startPosition);
        Physics.simulationMode = SimulationMode.FixedUpdate;

        if (Vector2.Distance(correctPosition, new Vector2(_transform.position.x, currentMovementData.positionY)) > _maxPositionError) {
            ReconciliateClientRPC(currentMovementData.tick);
            Debug.Log("Jump cheater");
        }
    }

    [ClientRpc]
    void ReconciliateClientRPC(int activationTick) {
        float correctPosition = _clientMovementDatas[(activationTick - 1) % k_buffer_size].positionY;

        Physics.simulationMode = SimulationMode.Script;
        while (activationTick <= _currentTick) {
            float jumpVector = _clientMovementDatas[(activationTick - 1) % k_buffer_size].positionY;
            _transform.position = new Vector2(_transform.position.x, correctPosition);
            _rb.linearVelocityY = jumpVector;
            Physics.Simulate(Time.fixedDeltaTime);
            correctPosition = _transform.position.y;
            _clientMovementDatas[activationTick % k_buffer_size].positionY = correctPosition;
            activationTick++;
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

        _transform.position = new Vector2(_transform.position.x, correctPosition);
    }
}