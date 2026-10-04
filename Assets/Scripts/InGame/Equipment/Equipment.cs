using System;
using UnityEngine;

namespace Game
{
    public abstract class Equipment : MonoBehaviour
    {
        public EquipmentInfo equipmentInfo;

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }
    }
}