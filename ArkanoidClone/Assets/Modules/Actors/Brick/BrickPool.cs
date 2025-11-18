using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using MiniIT.BRICK;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MiniIT.BRICK
{
    public class BrickPool : MonoBehaviour
    {
        // ========================================================================
        // --- PROPERTIES ---
        // ========================================================================

        public static BrickPool Instance
        {
            get;
            private set;
        }

        public Brick BrickPrefab
        {
            get
            {
                return brickPrefab;
            }
        }

        // ========================================================================
        // --- SERIALIZED FIELDS ---
        // ========================================================================

        [BoxGroup("POOL SETTINGS")]
        [Tooltip("The brick prefab to spawn.")]
        [SerializeField, Required]
        private Brick brickPrefab = null;

        [BoxGroup("STATE")]
        [Tooltip("List of all bricks managed by this pool.")]
        [SerializeField]
        private List<Brick> allManagedBricks = new List<Brick>();

        // ========================================================================
        // --- PUBLIC METHODS ---
        // ========================================================================

        /// <summary>
        /// (RUNTIME) Retrieves a brick from the pool or creates a new one.
        /// </summary>
        public Brick GetBrick()
        {
            foreach (Brick brick in allManagedBricks)
            {
                // If the brick was manually deleted in the editor
                if (brick == null)
                {
                    continue;
                }

                if (!brick.gameObject.activeSelf)
                {
                    brick.gameObject.SetActive(true);
                    return brick;
                }
            }

            // If no "sleeping" bricks found, create a new one
            return CreateNewBrick(true);
        }

        /// <summary>
        /// (RUNTIME) Returns a brick to the pool (deactivates it).
        /// </summary>
        public void ReturnBrick(Brick brick)
        {
            brick.gameObject.SetActive(false);
        }

        /// <summary>
        /// (RUNTIME) Deactivates all currently active bricks.
        /// </summary>
        public void ReturnAllActiveBricks()
        {
            foreach (Brick brick in allManagedBricks)
            {
                if (brick == null)
                {
                    continue;
                }

                if (brick.gameObject.activeSelf)
                {
                    brick.gameObject.SetActive(false);
                }
            }
        }

        // --- EDITOR METHODS ---

        /// <summary>
        /// (EDITOR) Creates a new brick using PrefabUtility and tracks it.
        /// </summary>
        public Brick GetBrickEditor()
        {
            return CreateNewBrick(false); // isRuntime = false
        }

        /// <summary>
        /// (EDITOR) Destroys all bricks tracked by the pool.
        /// </summary>
        [Button("Clear Editor Bricks")]
        public void DestroyAllBricksEditor()
        {
            // 1. Clear the tracking list
            allManagedBricks.Clear();

            // 2. "Nuclear" cleanup: Destroy all children.
            // Using while loop because childCount changes during destruction.
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }

            Debug.Log("BrickPool: Editor cleanup complete.");
        }

        // ========================================================================
        // --- PRIVATE METHODS ---
        // ========================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Initialize all existing bricks on startup
            foreach (Brick brick in allManagedBricks)
            {
                if (brick == null)
                {
                    continue;
                }

                brick.Init(this);
                brick.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Internal method to instantiate a brick.
        /// </summary>
        private Brick CreateNewBrick(bool isRuntime)
        {
            if (brickPrefab == null)
            {
                Debug.LogError("BrickPool: Prefab is not assigned!");
                return null;
            }

            Brick newBrick = null;

            if (isRuntime)
            {
                // RUNTIME: Use standard Instantiate
                newBrick = Instantiate(brickPrefab, transform);
            }
            else
            {
                // EDITOR: Use PrefabUtility
#if UNITY_EDITOR
                newBrick = (Brick)PrefabUtility.InstantiatePrefab(brickPrefab, transform);
#else
                // Fallback
                newBrick = Instantiate(brickPrefab, transform);
#endif
            }

            // General Setup
            newBrick.gameObject.name = "Brick_" + allManagedBricks.Count;
            newBrick.Init(this);
            allManagedBricks.Add(newBrick);

            if (isRuntime)
            {
                newBrick.gameObject.SetActive(true);
            }

            return newBrick;
        }
    }
}