using UnityEngine;
using System;

namespace Factory.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 10000;
        public int CurrentMoney { get; private set; }

        public Action OnMoneyChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CurrentMoney = startingMoney;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool CanAfford(int amount)
        {
            return CurrentMoney >= amount;
        }

        public void Spend(int amount)
        {
            if (CanAfford(amount))
            {
                CurrentMoney -= amount;
                OnMoneyChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning("Insufficient funds!");
            }
        }

        public void Earn(int amount)
        {
            CurrentMoney += amount;
            OnMoneyChanged?.Invoke();
        }
    }
}
