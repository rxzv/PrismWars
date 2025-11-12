using _PrismWars._Scripts.Game.Player.Model;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Player.Controllers {
    public class ClientJumpPrediction {
        Animator _animator;
        Rigidbody2D _rb;
        Transform _transform;
        float _jumpForce;
        readonly string _groundLayerName;

        float _maxPositionError;
    
        int _currentTick;
    
        const int k_buffer_size = 1024;
        JumpingData[] _clientJumpingDatas = new JumpingData[k_buffer_size];

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

            _clientJumpingDatas[currentTick % k_buffer_size] = new JumpingData {
                tick = currentTick,
                positionY = _transform.position.y,
                isJumping = isJumping,
                isGrounded = isGrounded,
                isRespawning = false,
            };

            if (currentTick < 2) return;
        
            JumpServerRPC(_clientJumpingDatas[currentTick % k_buffer_size],
                _clientJumpingDatas[(_currentTick - 1) % k_buffer_size]);
        }

        bool GroundCheck() {
            RaycastHit2D hit = Physics2D.Raycast(_rb.transform.position, Vector2.down);
            return hit.collider.IsTouchingLayers(LayerMask.GetMask(_groundLayerName));
        }
    
        [Rpc(SendTo.Server)]
        void JumpServerRPC(JumpingData currentMovementData, JumpingData lastMovementData) {
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

        [Rpc(SendTo.ClientsAndHost)]
        void ReconciliateClientRPC(int activationTick) {
            float correctPosition = _clientJumpingDatas[(activationTick - 1) % k_buffer_size].positionY;

            Physics.simulationMode = SimulationMode.Script;
            while (activationTick <= _currentTick) {
                float jumpVector = _clientJumpingDatas[(activationTick - 1) % k_buffer_size].positionY;
                _transform.position = new Vector2(_transform.position.x, correctPosition);
                _rb.linearVelocityY = jumpVector;
                Physics.Simulate(Time.fixedDeltaTime);
                correctPosition = _transform.position.y;
                _clientJumpingDatas[activationTick % k_buffer_size].positionY = correctPosition;
                activationTick++;
            }
            Physics.simulationMode = SimulationMode.FixedUpdate;

            _transform.position = new Vector2(_transform.position.x, correctPosition);
        }
    
        public void PlayerIsRespawning(int currentTick, Vector3 position) {
            _currentTick = currentTick;
            _clientJumpingDatas[currentTick % k_buffer_size] = new JumpingData {
                tick = currentTick,
                positionY = position.y,
                isJumping = false,
                isGrounded = false,
                isRespawning = true,
            };
            _clientJumpingDatas[(_currentTick - 1) % k_buffer_size] = new JumpingData {
                tick = currentTick,
                positionY = position.y,
                isJumping = false,
                isGrounded = false,
                isRespawning = true,
            };
            RespawnServerRpc(_clientJumpingDatas[currentTick % k_buffer_size]);
        }
        [Rpc(SendTo.Server)]
        void RespawnServerRpc(JumpingData currentMovementData) {
            if (currentMovementData.isRespawning) {
                _transform.position = new Vector2(_transform.position.x, currentMovementData.positionY);
            }
        }
    }
}