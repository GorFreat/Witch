using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Witch.Core.Stats
{
    [Serializable]
    public class CharacterStat
    {
        public float BaseValue;

        protected bool _isDirty = true;
        protected float _value;
        protected float _lastBaseValue;

        protected readonly List<StatModifier> _statModifiers;
        public readonly ReadOnlyCollection<StatModifier> StatModifiers;

        public event Action OnStatChange;
        public CharacterStat()
        {
            _statModifiers = new List<StatModifier>();
            StatModifiers = _statModifiers.AsReadOnly();
        }
        public CharacterStat(float baseValue) : this()
        {
            BaseValue = baseValue;
        }

        public virtual float Value
        {
            get
            {
                //If base get modified in the Inspector or by a new modifier
                if(_isDirty || Mathf.Abs(BaseValue - _lastBaseValue) > 0.0001f)
                {
                    _lastBaseValue = BaseValue;
                    _value = CalculateFinalValue();
                    _isDirty = false;
                }
                return _value;
            }
        }

        public virtual void AddModifier(StatModifier modifier)
        {
            _isDirty = true;
            _statModifiers.Add(modifier);
            _statModifiers.Sort(CompareModifierOrder);
            OnStatChange?.Invoke();
        }

        public virtual bool RemoveModifierFromSource(object source)
        {
            bool didRemove = false;
            for (int i = _statModifiers.Count - 1; i >= 0; i--)
            {
                if (_statModifiers[i].Source == source)
                {
                    _statModifiers.RemoveAt(i);
                    didRemove = true;
                }
            }

            if (didRemove)
            {
                _isDirty = true;
                OnStatChange?.Invoke();
            }
            return didRemove;
        }

        public virtual bool RemoveModifier(StatModifier modifier)
        {
            if (_statModifiers.Remove(modifier))
            {
                _isDirty = true;
                OnStatChange?.Invoke();
                return true;
            }
            return false;
        }

        protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order) return -1;
            if (a.Order > b.Order) return 1;
            return 0;
        }

        protected virtual float CalculateFinalValue()
        {
            float finalValue = BaseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < _statModifiers.Count; i++)
            {
                StatModifier mod = _statModifiers[i];
                
                if(mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;
                    //If the last Modifier is Percent Add or the next Modifier isn't Percent Add
                    if(i >= _statModifiers.Count - 1 || _statModifiers[i + 1].Type != StatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                }
                else if(mod.Type == StatModType.PercentMult)
                {
                    finalValue *= 1 + mod.Value;
                }
            }

            return (float)Math.Round(finalValue, 4);
        }
    }
}
