using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAndDispear : MonoBehaviour
{
    // Start is called before the first frame update
    float RotateSpeed;
    float MoveSpeed;
    Vector3 MoveForward;
    float DisappearTime = 10.0f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(DisappearTime);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.unscaledDeltaTime * RotateSpeed);
        transform.position += Time.unscaledDeltaTime * MoveSpeed * MoveForward;
    }

    public void InitParameters()
    {
        RotateSpeed = Random.Range(20, 100);
        MoveSpeed = Random.Range(1, 5);
        float angle = Random.Range(-Mathf.PI, Mathf.PI);
        MoveForward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
    }
}
