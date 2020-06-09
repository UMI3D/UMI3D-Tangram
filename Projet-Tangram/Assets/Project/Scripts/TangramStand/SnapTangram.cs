/*
Copyright 2019 Gfi Informatique

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnapTangram : MonoBehaviour
{
    public Material detectionMaterial;
    public Material castMaterial;
    public Material cast2Material;

    public Material collideMaterial;
    public Material canBeReleasedMaterial;

    public bool snapped = false;
    public float switchAngle = 90f;

    public bool isColliding = false;

    private Vector3 localHit;
    private Vector3 globalHit;

    private Transform refObject;

    public int collisions = 0;

    //public float snapLimit = 0.01f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            this.transform.Rotate(Vector3.up, switchAngle);
            snapped = false;
            VerifySnap();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            this.transform.Rotate(Vector3.up, switchAngle);
            snapped = false;
            Physics.SyncTransforms();
            VerifySnap();
        }

        if (!(globalHit == transform.TransformPoint(localHit)))
        //if (Input.GetKeyDown(KeyCode.Space))
        {
            Snap();
        }
        // A la fin add tag Tangram + rendre Kinematic

        if (collisions == 0)
        {
            GetComponent<MeshRenderer>().material = canBeReleasedMaterial;
        }
        else
        {
            GetComponent<MeshRenderer>().material = collideMaterial;
        }
    }


    void OnTriggerEnter(Collider collision)
    {
        if (!collision.transform.Equals(refObject) && transform.tag != "Tangram")
        {
            collisions++;
            Debug.Log("Enter");
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (!collision.transform.Equals(refObject) && transform.tag != "Tangram")
        {
            Debug.Log("Exit");
            collisions--;
        }
    }

    GameObject FindClosestTarget(string trgt)
    {
        Vector3 position = transform.position;
        return GameObject.FindGameObjectsWithTag(trgt)
            .OrderBy(o => (o.transform.position - position).sqrMagnitude)
            .FirstOrDefault();
    }

    void Snap()
    {
        GameObject closestObj = FindClosestTarget("Tangram");
        refObject = closestObj.transform;
        closestObj.GetComponent<MeshRenderer>().material = detectionMaterial;

        RaycastHit hit1;
        RaycastHit hit2;

        if (Physics.Linecast(transform.position, closestObj.transform.position, out hit1))
        {
            hit1.transform.GetComponent<MeshRenderer>().material = castMaterial;

            if (Physics.Linecast(closestObj.transform.position, transform.position, out hit2))

                hit2.transform.GetComponent<MeshRenderer>().material = cast2Material;
            else
            {
                transform.Translate(Vector3.Normalize(transform.position - closestObj.transform.position), Space.World);
                Physics.SyncTransforms();

                Physics.Linecast(closestObj.transform.position, transform.position, out hit2);
                hit2.transform.GetComponent<MeshRenderer>().material = cast2Material;
            }



            //float distance = Vector3.Distance(hit1.transform.position, hit2.transform.position);
            Vector3 normal1 = -hit1.normal;
            Vector3 normal2 = hit2.normal;

            Vector3 point1 = hit1.point;
            Vector3 point2 = hit2.point;

            //Debug.Log(point1);
            //Debug.Log(point2);

            float angle = Vector3.SignedAngle(normal2, normal1, Vector3.up);
            //Debug.Log(angle);

            if (angle != 0)
            {
                this.transform.Rotate(Vector3.up, angle);

                Physics.SyncTransforms();

                Physics.Linecast(closestObj.transform.position, transform.position, out RaycastHit hit3);
                point2 = hit3.point;
                //Debug.Log(point2);

            }

            transform.Translate(point1 - point2, Space.World);
            transform.GetComponent<Rigidbody>().isKinematic = true;
            //Debug.Log("__________________");

            // if (Vector3.Distance(point1, point2) < snapLimit)
            if (point1 == point2)
            {
                globalHit = point1;
                localHit = transform.InverseTransformPoint(point1);
                snapped = true;
                VerifySnap();
            }

            transform.GetComponent<MeshRenderer>().material = canBeReleasedMaterial;

        }
    }

    private void VerifySnap()
    {
        GameObject closestObj = FindClosestTarget("Tangram");

        Physics.SyncTransforms();
        Physics.Linecast(transform.position, closestObj.transform.position, out RaycastHit hit1);
        Physics.Linecast(closestObj.transform.position, transform.position, out RaycastHit hit2);

        if (Vector3.Distance(hit2.point, closestObj.transform.position) < Vector3.Distance(hit2.point, transform.position))
        {
            transform.Translate(Vector3.Normalize(transform.position - closestObj.transform.position), Space.World);
            Physics.SyncTransforms();
            snapped = false;
        }
    }

    //IEnumerator SnapCoroutine(GameObject closestObj, Vector3 point1)
    //{
    //    yield return new WaitForEndOfFrame();


    //    Physics.Linecast(transform.position, closestObj.transform.position, out RaycastHit newHit1);
    //    Physics.Linecast(closestObj.transform.position, transform.position, out RaycastHit newHit2);
    //    // Vector3 point2 = newHit.point;
    //  //  Debug.Log(point2);
    //    transform.Translate(newHit1.point - newHit2.point, Space.World);
    //    transform.GetComponent<Rigidbody>().isKinematic = true;
    //}

}
