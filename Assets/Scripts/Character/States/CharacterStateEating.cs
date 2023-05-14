using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class CharacterStateEating : CharacterState
{
    private Coroutine eatCoroutine;
    private ItemObjectInteractable foodObject;

    public CharacterStateEating(Customer customer, ItemObjectInteractable foodObject) : base(customer)
    {
        this.foodObject = foodObject;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (foodObject != null)
        {
            eatCoroutine = character.StartCoroutine(eatIEnum(foodObject));
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

        if (eatCoroutine != null)
        {
            character.StopCoroutine(eatCoroutine);
            eatCoroutine = null;
        }

        character.Movement.StopLookingAt();
    }

    private IEnumerator eatIEnum(ItemObjectInteractable foodObject)
    {
        if (foodObject == null)
        {
            yield break;
        }

        Customer customer = character as Customer;

        CharacterInteract.MoveToObject(this.character, foodObject, CharacterInteract.MoveToPointEnum.SpecificSurroundingCell);
        character.Movement.LookAt(foodObject.cell.GetCentrePosition());

        if(CharacterInteract.CloseToObject(character, foodObject, 1f))
        {
            yield return new WaitForSeconds(5.0f);

            customer.PurchaseItemFromObject(foodObject);
        }
        else
        {
            yield break;
        }


        customer.ChangeState(new CharacterStateWander(customer));

        this.foodObject = null;
    }

    public override string GetCurrentStateText()
    {
        return "Getting something to eat";
    }
}

