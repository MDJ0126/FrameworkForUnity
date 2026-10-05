using System;
using UnityEngine;

namespace Game
{
    public abstract class Equipment : MonoBehaviour
    {
        public Pawn Owner { get; private set; }
        public EquipmentInfo equipmentInfo;

        /// <summary>
        /// 소유자 세팅
        /// </summary>
        /// <param name="owner"></param>
        public void SetOwner(Pawn owner)
        {
            Owner = owner;
        }

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }
    }
}