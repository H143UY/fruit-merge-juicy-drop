using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Pool
{
    //--------------------------------------------------------------------------------------
//----Smart pool script use singleton pattern to cache prefabs with pooling mechanism.
//--------------------------------------------------------------------------------------


    /// <summary>
    /// -------------------Hose use:
    /// SmartPool.Instance.Spawn()      =     Instantite()
    /// SmartPool.Instance.Despawn()    =     Destroy()
    /// SmartPool.Instance.Preload()    =     Preload some object in game
    /// </summary>


    public class Pool
    {
        private int _nextId;

        private Stack<GameObject> _inactive;         // Stack hold gameobject belong this pool in state inactive
        private GameObject _prefabContrainer;                // Gameobject contain pools gameobject
        public GameObject Prefab;                          // Prefabs belong pool

        /// <summary>
        /// Inital pool
        /// </summary>
        /// <param name="prefabs">Prefab belong to pool</param>
        /// <param name="initQuantify">Number gameobject initial</param>
        public Pool(GameObject prefabs, int initQuantify)
        {
            this.Prefab = prefabs;
            this._prefabContrainer = new GameObject(prefabs.name + "_pool");

            _inactive = new Stack<GameObject>(initQuantify);
        }

        /// <summary>
        /// Instantiate gameobject to scene
        /// If stack don't have any gameobject in state deactive,
        /// we will instantiate new gameobject
        /// Otherwise, we remove one elemnet in stack and active it in game
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="localScale"></param>
        /// <returns></returns>
        public GameObject Spawn(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            GameObject obj;

            if (_inactive.Count == 0)
            {
                // Instatite if stack empty
                obj = Object.Instantiate(Prefab, position, rotation);

                if (_nextId >= 10)
                    obj.name = Prefab.name + "_" + (_nextId++);
                else
                    obj.name = Prefab.name + "_0" + (_nextId++);
                var poolIdentify = obj.GetComponent<PoolIdentify>();
                if (poolIdentify)
                {
                    poolIdentify.pool = this;
                }
                else
                {
                    obj.AddComponent<PoolIdentify>().pool = this;
                }
          
                // Set to contrainer
                obj.transform.SetParent(_prefabContrainer.transform);
            }
            else
            {
                obj = _inactive.Pop();

                if (obj == null)
                    return Spawn(position, rotation, localScale);
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = localScale;
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Method return gameobject belong to pool
        /// </summary>
        /// <param name="obj">Gameobject will return pool</param>
        public void Despawn(GameObject obj)
        {
            obj.SetActive(false);
            _inactive.Push(obj);
        }

        /// <summary>
        /// Method to destroy pool
        /// </summary>
        public void DestroyAll()
        {
            // Return stack
            Prefab = null;

            // Clear stack
            _inactive.Clear();

            // Destroy child
            for (int i = 0; i < _prefabContrainer.transform.childCount; i++)
                Object.Destroy(_prefabContrainer.transform.GetChild(i).gameObject);

            // Destroy parent
            Object.Destroy(_prefabContrainer);

            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        ///  Chekc pool exist or not when load new level
        /// </summary>
        /// <returns></returns>
        public bool CheckPoolExist()
        {
            return (_prefabContrainer);
        }

        /// <summary>
        /// Method return all gameobject to pool
        /// </summary>
        public void ReturnPool()
        {
            Transform containerTrans = _prefabContrainer.transform;
            for (int i = 0; i < containerTrans.childCount; i++)
            {
                if (containerTrans.GetChild(i).gameObject.activeSelf)
                    Despawn(containerTrans.GetChild(i).gameObject);
            }
        }
    }


    /// <summary>
    /// Main class hold pool data
    /// </summary>
    public class SmartPool : SingletonMono<SmartPool>
    {

        const int DefaultPoolSize = 3;

        private Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

        void OnEnable()
        {
            //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        /// <summary>
        /// Initial dictionary for pool system
        /// </summary>
        /// <param name="prefabs"></param>
        /// <param name="quantify"></param>
        void Init(GameObject prefabs = null, int quantify = DefaultPoolSize)
        {
            if (Instance._pools == null)
                instance._pools = new Dictionary<GameObject, Pool>();

            if (prefabs != null && instance._pools.ContainsKey(prefabs) == false)
                instance._pools[prefabs] = new Pool(prefabs, quantify);
        }

        /// <summary>
        /// Method to preload some gameobject in to scene
        /// </summary>
        /// <param name="prefab">Prefab will instantiate</param>
        /// <param name="quantify">Number instantiate</param>
        public void Preload(GameObject prefab, int quantify)
        {
            Init(prefab, quantify);

            GameObject[] obs = new GameObject[quantify];
            for (int i = 0; i < quantify; i++)
                obs[i] = Spawn(prefab, Vector3.zero, Quaternion.identity);

            for (int i = 0; i < quantify; i++)
                Despawn(obs[i]);
        }


        /// <summary>
        ///  Method to instantiate prefab to scene
        /// </summary>
        /// <param name="prefabs">Objects will spawn</param>
        /// <param name="position">Position for gameoject</param>
        /// <param name="rotation">Rotation for gameobject</param>
        /// <param name="localScale">LocalScale for gameobject</param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefabs, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            if (!prefabs)
            {
                return null;
            }
            Init(prefabs);
      
            return instance._pools[prefabs].Spawn(position, rotation, localScale);
        }

        /// <summary>
        ///  Method to instantiate prefab to scene
        /// </summary>
        /// <param name="prefabs">Objects will spawn</param>
        /// <param name="position">Position for gameoject</param>
        /// <param name="rotation">Rotation for gameobject</param>
        /// <returns></returns>
        public GameObject Spawn(GameObject prefabs, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefabs, position, rotation, Vector3.one);
        }

        /// <summary>
        /// Method to deactive gameobject
        /// </summary>
        /// <param name="prefabs">Gameobject will deactive</param>
        public void Despawn(GameObject prefabs)
        {
            PoolIdentify poolIndent = prefabs.GetComponent<PoolIdentify>();
        
            if (poolIndent == null)
            {
          

                prefabs.SetActive(false);
            }
            else
            {
           

                poolIndent.pool.Despawn(prefabs);
            }
           
        }

        /// <summary>
        /// Method will remove prefab in system pool
        /// </summary>
        /// <param name="prefabs"></param>
        public void DestroyPool(GameObject prefabs)
        {
            if (instance._pools.ContainsKey(prefabs))
            {
                instance._pools[prefabs].DestroyAll();
                instance._pools.Remove(prefabs);
            }
        }

        /// <summary>
        /// Method will make all gameoject belong prefab will deactive
        /// </summary>
        /// <param name="prefab"></param>
        public void ReturnPool(GameObject prefab)
        {
            if (instance._pools == null)
                return;

            if (instance._pools.ContainsKey(prefab))
                instance._pools[prefab].ReturnPool();
        }

        /// <summary>
        /// Method make all gameobject will deactive in pool system
        /// </summary>
        public void ReturnPoolAll()
        {
            var pools = FindObjectsOfType<PoolIdentify>();
            for (int i = 0; i < pools.Length; i++)
                Despawn(pools[i].gameObject);
        }

        /// <summary>
        /// When load new scene, we need clear garbage 
        /// </summary>
        private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            var itemsRemove = _pools.Where(f => !f.Value.CheckPoolExist()).ToArray();
            foreach (KeyValuePair<GameObject, Pool> element in itemsRemove)
            {
                if (!element.Value.CheckPoolExist())
                    _pools.Remove(element.Key);
            }

            // Clear resource and GC in memory
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}