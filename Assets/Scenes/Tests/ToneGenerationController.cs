using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToneGenerationController : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mousePickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mousePickRay, out RaycastHit hit))
            {
                BellSynthesizer bell = hit.collider.gameObject.GetComponent<BellSynthesizer>();
                if (bell != null)
                {
                    bell.Play();
                }
            }
        }
    }
}
