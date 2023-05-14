using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterStateDrinking : CharacterState
{
    private Coroutine drinkCoroutine;
    private ItemObjectInteractable drinkObject;

    public CharacterStateDrinking(Customer customer, ItemObjectInteractable drinkObject) : base(customer)
    {
        this.drinkObject = drinkObject;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (drinkObject != null)
        {
            drinkCoroutine = character.StartCoroutine(drinkIEnum(drinkObject));
        }
        // Leave if could not find food
        else
        {
            character.ChangeState(new CharacterStateLeaving(character));
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        Customer customer = character as Customer;

        if (drinkCoroutine != null)
        {
            character.StopCoroutine(drinkCoroutine);
            drinkCoroutine = null;
        }

        character.Movement.StopLookingAt();
    }

    private IEnumerator drinkIEnum(ItemObjectInteractable drinkObject)
    {
        if (drinkObject == null)
        {
            yield break;
        }

        Customer customer = character as Customer;

        CharacterInteract.MoveToObject(this.character, drinkObject, CharacterInteract.MoveToPointEnum.SpecificSurroundingCell);
        character.Movement.LookAt(drinkObject.cell.GetCentrePosition());

        if (CharacterInteract.CloseToObject(character, drinkObject, 1f))
        {
            yield return new WaitForSeconds(5.0f);

            customer.PurchaseItemFromObject(drinkObject);
        }
        else
        {
            yield break;
        }

        customer.ChangeState(new CharacterStateWander(customer));

        this.drinkObject = null;
    }

    public override string GetCurrentStateText()
    {
        return "Getting a drink";
    }
}
