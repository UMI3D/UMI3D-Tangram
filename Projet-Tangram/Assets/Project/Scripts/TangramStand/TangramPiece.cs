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
using umi3d.edk;
using System.Linq;

public class TangramPiece : MonoBehaviour
{
    public string tagGround;
    public string tagTangram;

    public GameObject ghostPrefab;
    public Transform defaultParent;
    public CVEEquipable equipable;

    public bool isVisible = false;
    public bool isEquiped = false;
    public bool hasBeenPlaced = false;

    private float verticalOffset = 0.05f;

    private float switchAngle = 90f;

    private bool hadGhost = false;
    private UMI3DAvatarBone equipedBone;

    private GameObject ghost;
    private GameObject holder;
    private Vector3 hitPoint;
    private float offset = 0f;

    private bool flipped = false;

    Vector3 lastGhostPos;
    Quaternion lastGhostRot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isEquiped)
        {
            Transform holder = GetPointedHolder();
            if (holder == null)
            {
                if (ghost != null)
                {
                    Destroy(ghost);
                    ghost = null;
                }
                hadGhost = false;
            }
            else
            {
                if (ghost == null)
                    ghost = Instantiate(ghostPrefab);

                 
                SimpleTransform simpleTransform = GetProjection(holder, hitPoint);
                ghost.transform.position = simpleTransform.position;
                ghost.transform.Translate(0, verticalOffset, 0, Space.World);
                ghost.transform.rotation = simpleTransform.rotation;

                if (flipped)
                {
                    ghost.transform.Rotate(Vector3.right, 180);
                    ghost.transform.LookAt(GetProjection(holder, this.transform.position).position + new Vector3(0, verticalOffset, 0), Vector3.down);
                }
                else
                    ghost.transform.LookAt(GetProjection(holder, this.transform.position).position + new Vector3(0, verticalOffset, 0));

                ghost.transform.Rotate(Vector3.up, offset, Space.World);

                Physics.SyncTransforms();

                Snap();

                lastGhostPos = ghost.transform.position;
                lastGhostRot = ghost.transform.rotation;
                hadGhost = true; 
            }
        }
    }

    public struct SimpleTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public SimpleTransform GetProjection(Transform holder, Vector3 piecePosition)
    {
        return new SimpleTransform()
        {
            position = holder.position + Vector3.ProjectOnPlane(piecePosition - holder.position, holder.up),
            rotation = holder.rotation,
            scale = Vector3.one
        };
    }

    protected virtual void Awake()
    {
        equipable.onUnequiped.AddListener(() =>
        {
            isEquiped = false;
            offset = 0f;
            if (hadGhost)
            {
                Destroy(ghost);
                this.transform.position = lastGhostPos;
                this.transform.rotation = lastGhostRot;
                hadGhost = false;
                this.transform.tag = tagTangram;
                hasBeenPlaced = true;
            }
            else
            {
                this.transform.parent = defaultParent;
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.Euler(Vector3.zero);
                this.transform.tag = "Untagged";
            }
        });

        equipable.onEquiped.AddListener(() =>
        {
            isEquiped = true;
            hasBeenPlaced = false;
            this.transform.tag = "Untagged";
        });
    }

    public void Equipe(UMI3DUser user, string bone)
    {
        if (TangramGameManager.Instance.getRole(user.UserId) == UserRole.BluePill)
        {
            BoneType boneType = UMI3DAvatarBone.instancesByUserId[user.UserId][bone].boneType;

            if ((boneType == BoneType.LeftHand) || (boneType == BoneType.RightHand))
            {
                equipable.RequestEquip(user, bone);
            }
            else
            {
                UMI3DAvatarBone userBone = UMI3DAvatarBone.GetUserBoneByType(user.UserId, BoneType.RightHand);
                equipable.RequestEquip(user, (userBone != null) ? userBone.boneId : bone);
            }

            //equipedUser = user;
            equipedBone = UMI3DAvatarBone.GetUserBoneByType(user.UserId, umi3d.common.BoneType.RightHand);
        }
    }

    public Transform GetPointedHolder()
    {
        Transform boneAnchor = UMI3D.Scene.GetObject(equipedBone.boneAnchorId).transform;

        RaycastHit[] hits;
        hits = umi3d.Physics.RaycastAll(boneAnchor.position, boneAnchor.up, 10.0F);

        foreach (RaycastHit hit in hits)
        {
            hitPoint = hit.point;
            Transform holder = hit.transform;
            if (holder.tag == tagGround)
                return holder;
        }
            return null;
    }

    GameObject FindClosestPiece()
    {
        Vector3 position = ghost.transform.position;
        return GameObject.FindGameObjectsWithTag(tagTangram)
            .OrderBy(o => (o.transform.position - position).sqrMagnitude)
            .FirstOrDefault();
    }

    void Snap()
    {
        GameObject closestObj = FindClosestPiece();

        if (closestObj != null)
        {
            if (!Physics.Linecast(ghost.transform.position, closestObj.transform.position, out RaycastHit hit1))
            {
                transform.Translate(0.25f * Vector3.Normalize(ghost.transform.position - closestObj.transform.position), Space.World);
                Physics.SyncTransforms();

                Physics.Linecast(ghost.transform.position, closestObj.transform.position, out hit1);
            }

            if (!Physics.Linecast(closestObj.transform.position, ghost.transform.position, out RaycastHit hit2))
            {
                transform.Translate(0.25f * Vector3.Normalize(ghost.transform.position - closestObj.transform.position), Space.World);
                Physics.SyncTransforms();

                Physics.Linecast(closestObj.transform.position, ghost.transform.position, out hit2);
            }

            Vector3 normal1 = -hit1.normal;
            Vector3 normal2 = hit2.normal;

            Vector3 point1 = hit1.point;
            Vector3 point2 = hit2.point;

            float angle = Vector3.SignedAngle(normal2, normal1, Vector3.up);

            if (angle != 0)
            {
                ghost.transform.Rotate(Vector3.up, angle, Space.World);

                Physics.SyncTransforms();

                Physics.Linecast(closestObj.transform.position, ghost.transform.position, out RaycastHit hit3);
                point2 = hit3.point;
            }

            ghost.transform.Translate(point1 - point2, Space.World);
        }
    }

    public void Pose(UMI3DUser user, string bone)
    {
        equipable.RequestUnequip();
    }

    public void RotateRight(UMI3DUser user, string bone)
    {
        offset += switchAngle;
    }

    public void RotateLeft(UMI3DUser user, string bone)
    {
        offset -= switchAngle;
    }

    public void Flip(UMI3DUser user, string bone)
    {
        flipped = !flipped;
    }
}
