using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillManager : MonoBehaviour
{
    [SerializeField] SkillStrategy[] _skills;
    [SerializeField] CharacterManager _characterManager;

    private void OnEnable()
    {
        EventSystem.OnSkillButtonPressed += CastSkill;
    } 

    private void OnDisable()
    {
        EventSystem.OnSkillButtonPressed -= CastSkill;
    }

    private void CastSkill(ulong id, int skillIndex)
    {
        if( id == _characterManager.networkID)
        {
            _skills[skillIndex].CastSkill(transform);
        }
    }
}
