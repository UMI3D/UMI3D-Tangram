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
using umi3d.common;
using System;

namespace umi3d.edk
{
    [Serializable]
    public struct PairBoneObject
    {
        public BoneType bone;
        public GameObject Go;
    }

    public class UMI3DRiggedAvatar : MonoBehaviour
    {
        public UMI3DUser followingUser;
        public bool isGLTF;
        public List<PairBoneObject> boneToMesh = new List<PairBoneObject>();

        private Dictionary<BoneType, Transform> jointToBone = new Dictionary<BoneType, Transform>();

        // Update is called once per frame
        void Update()
        {
            if (followingUser)
                UpdateRig();
        }

        /// <summary>
        /// Updating the rig of an avatar from its user UMI3DAvatarBone data
        /// </summary>
        /// <param name="connection">The real-time connection.</param>
        public void UpdateRig()
        {
            if (UMI3DAvatarBone.instancesByUserId.TryGetValue(followingUser.UserId, out Dictionary<string, UMI3DAvatarBone> userBoneDictionary))
            {
                foreach (var item in userBoneDictionary)
                {
                    BoneType boneType = item.Value.boneType;
                    Transform bone = UMI3D.Scene.GetObject(item.Value.boneAnchorId).transform;

                    if (!jointToBone.ContainsValue(bone))
                        jointToBone.Add(boneType, bone);

                    else
                        throw new Exception("Bone already registered");
                }

                RiggedAvatarDto riggedAvatarDto = new RiggedAvatarDto()
                {
                    id = this.GetComponentInChildren<CVEModel>().Id,
                    isGLTF = this.isGLTF
                };

                foreach (var pair in boneToMesh)
                {
                    BoneType objType = pair.bone;
                    if (jointToBone.ContainsKey(objType))
                    {
                        pair.Go.transform.rotation = jointToBone[objType].transform.rotation;
                    }

                    RiggedObjectRotation riggedObjectRotation = new RiggedObjectRotation
                    {
                        objectName = pair.Go.name,
                        objectRotation = pair.Go.transform.rotation
                    };
                    riggedAvatarDto.TryAdd(riggedObjectRotation);
                }

                if (riggedAvatarDto.Count() != 0)
                {
                    foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                        user.Send(riggedAvatarDto);
                }

                jointToBone.Clear();
            }
        }
    }
}