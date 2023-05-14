using UnityEngine;

[CreateAssetMenu(fileName = "CustomerParams", menuName = "Data Assets/CustomerParams", order = 0)]
public class CustomerParams : ScriptableObject
{
    public float MaxHappiness = 100.0f;
    public float MaxEnergy = 100.0f;
    public float MaxInterest = 100.0f;

    [Tooltip("Amount energy decreases each second while standing")]
    public float EnergyDecreaseRate = 2.0f;

    [Tooltip("Amount happiness decreases each second while in a queue")]
    public float QueueHappinessDecreaseRate = 0.5f;

    [Tooltip("Amount energy increases each second while sitting")]
    public float EnergyRecoveryRateSitting = 10.0f;

    [Tooltip("How often customers check for nearby trash piles")]
    public float SeeTrashInterval = 10.0f;
    [Tooltip("Distance to check for trash piles every SeeTrashInterval seconds")]
    public float SeeTrashRange = 8.0f;
    [Tooltip("Amount happiness decreases per trash pile in range")]
    public float SeeTrashHappinessDecrease = 4.0f;

    [Tooltip("How often customers check for nearby decorations")]
    public float SeeDecorationInterval = 10.0f;
    [Tooltip("Range to check for decorations every SeeDecorationRange seconds")]
    public float SeeDecorationRange = 8.0f;
    [Tooltip("Amount happiness increases per decoration in range")]
    public float SeeDecorationHappinessIncrease = 4.0f;

    [Tooltip("The maximum amount of energy that a customer can restore by sitting")]
    public float MaxEnergyRestoredBySitting = 150.0f;

    [Tooltip("Distance to check for windows/doors every frame to increase interest")]
    public float RangeToIncreaseShopInterest = 5.0f;
    [Tooltip("The amount interest increases each second per window/door in range for each shop")]
    public float ShopInterestIncreaseRatePerObject = 8.0f;
}
