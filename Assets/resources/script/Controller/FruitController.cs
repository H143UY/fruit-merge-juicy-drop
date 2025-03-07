using Core.Pool;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FruitController : MonoBehaviour
{
    private Rigidbody2D rg;
    public bool FruitNew;
    private CircleCollider2D circle;
    public int level { get; private set; }
    public bool isIdle;
    public bool isUpgraded;
    private Animator animator;
    private bool CheckGameOver;
    private bool canbounce;
    public bool isMerging;

    private void Awake()
    {
        isUpgraded = false;
        isMerging = false;
    }

    private void Start()
    {
        rg = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        circle.enabled = false;
        isIdle = false;
        FruitNew = true;
        CheckGameOver = false;
        canbounce = !isUpgraded;
    }

    private void Update()
    {
        if (FruitNew && !isUpgraded)
        {
            this.transform.position = FruitManager.instance.PosSpawnFruit.position;
        }
        if (!FruitNew && rg.velocity.magnitude < 0.1f)
        {
            CheckGameOver = true;
        }
        AnimatorFruit();

        if (Input.GetMouseButtonUp(0) && FruitNew)
        {
            Release();
        }

        if (!isIdle)
        {
            float TimeIdle = 0;
            TimeIdle += Time.deltaTime;
            if (TimeIdle >= 10)
            {
                isIdle = true;
            }
        }
        if (isUpgraded)
        {
            circle.enabled = true;
        }
    }

    public void Release()
    {
        FruitNew = false;
        if (circle != null)
        {
            circle.enabled = true;
        }
    }

    public void SetLevel(int newLevel, bool isNew, bool upgraded = false)
    {
        level = newLevel;
        FruitNew = isNew;
        isUpgraded = upgraded;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "san" || collision.gameObject.tag == "Fruit")
        {
            if (canbounce)
            {
                transform.DOMoveY(transform.position.y + 0.5f, 0.2f).SetEase(Ease.OutQuad);
                canbounce = false;
            }
        }

        if (collision.gameObject.tag == "Fruit")
        {
            FruitController otherFruit = collision.gameObject.GetComponent<FruitController>();

            if (otherFruit != null && otherFruit.level == this.level && !isMerging && !otherFruit.isMerging)
            {
                FruitController[] allFruits = FindObjectsOfType<FruitController>();
                FruitController lowestIDFruit = this;

                foreach (var fruit in allFruits)
                {
                    if (fruit.level == this.level && fruit != this && fruit.GetInstanceID() < lowestIDFruit.GetInstanceID())
                    {
                        lowestIDFruit = fruit;
                    }
                }
                if (this == lowestIDFruit)
                {
                    isMerging = true;
                    otherFruit.isMerging = true;
                    int upLevel = level + 1;

                    if (upLevel < FruitManager.instance.fruits.Length)
                    {
                        Vector3 spawnPos = (transform.position + otherFruit.transform.position) / 2;
                        otherFruit.transform.DOMove(spawnPos, 0.5f).OnComplete(() =>
                        {
                            SmartPool.Instance.Despawn(otherFruit.gameObject);
                            SmartPool.Instance.Despawn(this.gameObject);
                            FruitManager.instance.UpLevelFruit(upLevel, spawnPos);
                        });
                    }
                    else
                    {
                        SmartPool.Instance.Despawn(otherFruit.gameObject);
                        SmartPool.Instance.Despawn(this.gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CheckGameOver && collision.gameObject.tag == "Dead")
        {
            this.PostEvent(EventID.GameOver);
        }
    }
    private void AnimatorFruit()
    {
        animator.SetBool("isIdle", isIdle);
    }

    private void SetIdle()
    {
        isIdle = false;
    }
}
