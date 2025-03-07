using Core.Pool;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class FruitManager : MonoBehaviour
{
    public static FruitManager instance;
    public GameObject[] fruits;
    public Dictionary<GameObject, int> fruitLevels = new Dictionary<GameObject, int>();
    public Transform PosSpawnFruit;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        //for (int i = 0; i < fruits.Length; i++)
        //{
        //    fruitLevels[fruits[i]] = i;
        //}
    }
    private void Start()
    {
        SpawnFruit(0, PosSpawnFruit.position, true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(InstantiateFruit());
        }
    }
    private IEnumerator InstantiateFruit()
    {
        yield return new WaitForSeconds(0.5f);
        int randomIndex = Random.Range(0, 4);
        SpawnFruit(randomIndex, PosSpawnFruit.position, true);
    }

    private void SpawnFruit(int index, Vector3 position, bool isNew)
    {
        var fruit = SmartPool.Instance.Spawn(fruits[index], position, transform.rotation);
        FruitController fruitController = fruit.GetComponent<FruitController>();

        if (fruitController != null)
        {
            fruitController.SetLevel(index, true);
        }
    }
    public void UpLevelFruit(int level, Vector3 upPosition)
    {
        if (level >= fruits.Length) return;

        var newFruit = SmartPool.Instance.Spawn(fruits[level], upPosition, transform.rotation);
        newFruit.transform.localScale = Vector3.zero;
        newFruit.transform.DOScale(Vector3.one, 1f);
        GameManager.instance.addScore(level);
        FruitController fruitController = newFruit.GetComponent<FruitController>();
        fruitController.SetLevel(level, false, true);
        fruitController.isMerging = false;
    }
}
