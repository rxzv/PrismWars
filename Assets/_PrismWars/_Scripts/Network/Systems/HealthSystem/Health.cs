using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthSlider;
    
    private NetworkVariable<float> currentHealth = new(100f);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar(currentHealth.Value);
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        UpdateHealthBar(newHealth);
        
        if (newHealth <= 0)
        {
            Debug.Log("Player died!");
        }
    }

    private void UpdateHealthBar(float health)
    {
        if (healthSlider != null)
        {
            healthSlider.value = health / maxHealth;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage = 10f)
    {
        if (currentHealth.Value > 0)
        {
            currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
        }
    }

    private void Update()
    {
        // Тест: нажми H чтобы нанести урон себе
        if (IsOwner && Input.GetKeyDown(KeyCode.H))
        {
            TakeDamageServerRpc(10f);
            Debug.Log("Damage taken!");
        }
    }
}