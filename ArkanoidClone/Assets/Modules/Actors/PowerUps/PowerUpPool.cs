using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace MiniIT.POWERUP
{
    public class PowerUpPool : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static PowerUpPool Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("POOL SETTINGS")]
        [Tooltip("Prefab to instantiate.")]
        [SerializeField, Required]
        private PowerUp powerUpPrefab = null;

        [BoxGroup("POOL SETTINGS")]
        [Tooltip("Number of items to create on Awake.")]
        [SerializeField]
        private int initialPoolSize = 5;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private List<PowerUp> allPowerUps = new List<PowerUp>();

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Retrieves an inactive power-up from the pool or creates a new one.
        /// </summary>
        /// <param name="position">Spawn position.</param>
        public PowerUp GetPowerUp(Vector3 position)
        {
            foreach (PowerUp p in allPowerUps)
            {
                if (!p.gameObject.activeSelf)
                {
                    p.transform.position = position;
                    p.gameObject.SetActive(true);
                    p.ResetState();
                    return p;
                }
            }

            // If no free objects, create a new one
            PowerUp newP = CreateNewPowerUp(true);
            newP.transform.position = position;
            return newP;
        }

        /// <summary>
        /// Returns the power-up to the pool (deactivates it).
        /// </summary>
        public void ReturnPowerUp(PowerUp p)
        {
            p.gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates all power-ups (used when changing levels).
        /// </summary>
        public void ReturnAllActive()
        {
            foreach (PowerUp p in allPowerUps)
            {
                if (p.gameObject.activeSelf)
                {
                    p.gameObject.SetActive(false);
                }
            }
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewPowerUp(false);
            }
        }

        private PowerUp CreateNewPowerUp(bool isActive)
        {
            PowerUp newObj = Instantiate(powerUpPrefab, transform);
            allPowerUps.Add(newObj);
            newObj.gameObject.SetActive(isActive);
            return newObj;
        }
    }
}