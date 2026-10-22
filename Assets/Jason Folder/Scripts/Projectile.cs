using UnityEngine;

public class Projectile : MonoBehaviour
{
    
    int speed;
    float lifeSpan = 2;
    float currentLifeTime = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = 5;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime > lifeSpan)
        {
            Destroy(gameObject);
        }
    }

}
