using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	private float distance;
	private GameObject player;
	private Ray ray;
	private bool opened;
    private Animator animator;

	void Awake () {
		player = GameObject.FindGameObjectWithTag ("Player");
        animator = GetComponent<Animator>();
		opened = false;
	}

	void Update () {

        if (opened == false)
        {
            distance = Vector3.Distance(transform.position, player.transform.position);
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetKeyDown(KeyCode.E) && distance < 2 && Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Door")
            {
                animator.SetBool("IsOpen", true);
                opened = true;
            }
        }
        else
        {
            distance = Vector3.Distance(transform.position, player.transform.position);
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetKeyDown(KeyCode.E) && distance < 2 && Physics.Raycast(ray, out hit) && hit.collider.gameObject.tag == "Door")
            {
                animator.SetBool("IsOpen", false);
                opened = false;
            }
        }
    }
}
