using MiniIT.BALL;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.BALL
{
    public class BallPool : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static BallPool Instance
        {
            get;
            private set;
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("POOL CONFIG")]
        [SerializeField, Required]
        private BallController ballPrefab = null;

        [BoxGroup("POOL CONFIG")]
        [SerializeField]
        private int initialPoolSize = 5;

        // ========================================================================
        // --- PRIVATE FIELDS ---
        // ========================================================================

        private List<BallController> allBalls = new List<BallController>();

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// Retrieves an inactive ball from the pool or creates a new one if needed.
        /// </summary>
        public BallController GetBall()
        {
            foreach (BallController ball in allBalls)
            {
                if (!ball.gameObject.activeSelf)
                {
                    ball.gameObject.SetActive(true);
                    return ball;
                }
            }

            return CreateNewBall(true);
        }

        /// <summary>
        /// Returns a specific ball to the pool (deactivates it).
        /// </summary>
        public void ReturnBall(BallController ball)
        {
            ball.gameObject.SetActive(false);
        }

        /// <summary>
        /// Deactivates all balls currently in the pool.
        /// </summary>
        public void ReturnAllBalls()
        {
            foreach (BallController ball in allBalls)
            {
                ball.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Returns a list of all currently active balls (useful for cloning logic).
        /// </summary>
        public List<BallController> GetActiveBalls()
        {
            List<BallController> active = new List<BallController>();

            foreach (BallController ball in allBalls)
            {
                if (ball.gameObject.activeSelf)
                {
                    active.Add(ball);
                }
            }

            return active;
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            Instance = this;

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewBall(false);
            }
        }

        private BallController CreateNewBall(bool isActive)
        {
            BallController newBall = Instantiate(ballPrefab, transform);
            allBalls.Add(newBall);
            newBall.gameObject.SetActive(isActive);
            return newBall;
        }
    }
}