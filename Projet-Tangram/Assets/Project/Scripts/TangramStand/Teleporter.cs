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
using umi3d.edk;
using umi3d.common;

public class Teleporter : MonoBehaviour
{
    public Transform spawnPosition;
    public bool Directed = false;

    public void TeleportUser(UMI3DUser user, Transform pos)
    {
        Vector3 spawnPos = new Vector3(
            pos.position.x,
            pos.position.y,
            pos.position.z
            );
        
        if (Directed)
            user.Send(new TeleportDto() { Position = spawnPos, Rotation = transform.localRotation });
        else
            user.Send(new TeleportDto() { Position = spawnPos });
    }

    protected IEnumerator TeleportWithDelay(UMI3DUser user, float delay)
    {
        yield return new WaitForSeconds(delay);
        TeleportUser(user, spawnPosition);
    }

    //private void Start()
    //{
    //    GetComponent<UMI3DEvent>().onTrigger.AddListener((user, boneDto) =>
    //    {
    //        TeleportUser(user, spawnPosition);
    //    });
    //}

    public void TeleportSpawnPosition(UMI3DUser user, string boneId)
    {
        TeleportUser(user, spawnPosition);
    }
}
