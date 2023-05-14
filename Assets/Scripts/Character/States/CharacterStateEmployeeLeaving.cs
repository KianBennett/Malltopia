using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateEmployeeLeaving : CharacterStateLeaving
{
    public CharacterStateEmployeeLeaving(Character character) : base(character)
    {
    }

    public override void OnUpdateState()
    {
        base.OnUpdateState();

        // Employees leave when they run out of energy and no staff room is available
        // If they are quitting and a staff room is built, they will return to their break

        Room nearestStaffRoom = Mall.CurrentFloor.FindNearestRoomOfType((character as Employee).StaffRoomBuilding, character.transform.position);

        if (nearestStaffRoom != null)
        {
            character.ChangeState(new CharacterStateBreak(character as Employee));
        }
    }

    public override string GetCurrentStateText()
    {
        return "Quitting";
    }
}
