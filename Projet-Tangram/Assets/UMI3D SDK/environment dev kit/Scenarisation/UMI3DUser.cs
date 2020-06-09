﻿/*
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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;


namespace umi3d.edk
{
    public class UMI3DUser : MonoBehaviour
    {

        /// <summary>
        /// The unique user identifier.
        /// </summary>
        private string userId;

        /// <summary>
        /// Readonly getter for user identifier.
        /// </summary>
        public string UserId { get { return userId; } }

        /// <summary>
        /// UMI3D realtime connection.
        /// </summary>
        protected IUMI3DRealtimeConnection connection;

        /// <summary>
        /// The available interactions on last update.
        /// </summary>
        protected AbstractInteraction[] currentInteractions;

        /// <summary>
        /// Indicate if the first GetUpdates has been done.
        /// </summary>
        protected bool visibilitySet = false;

        /// <summary>
        /// List of objects to load on the next frame for the user.
        /// </summary>
        public List<EmptyObject3DDto> ObjectsToLoad = new List<EmptyObject3DDto>();

        /// <summary>
        /// List of objects to remove on the next frame for the user.
        /// </summary>
        public List<RemoveObjectDto> ObjectsToRemove = new List<RemoveObjectDto>();

        /// <summary>
        /// List of toolboxes to load on the next frame for the user.
        /// </summary>
        public List<ToolboxDto> ToolboxesToLoad = new List<ToolboxDto>();

        /// <summary>
        /// List of Tools to load on the next frame for the user.
        /// </summary>
        public List<ToolDto> ToolsToLoad = new List<ToolDto>();

        /// <summary>
        /// List of interactions to load on the next frame for the user.
        /// </summary>
        public List<AbstractInteractionDto> InteractionsToLoad = new List<AbstractInteractionDto>();

        /// <summary>
        /// List of interactions to remove on the next frame for the user.
        /// </summary>
        public List<string> ToolboxesIdsToRemove = new List<string>();

        /// <summary>
        /// List of interactions to remove on the next frame for the user.
        /// </summary>
        public List<string> ToolsIdsToRemove = new List<string>();

        /// <summary>
        /// List of interactions to remove on the next frame for the user.
        /// </summary>
        public List<string> InteractionsIdsToRemove = new List<string>();

        /// <summary>
        /// List of equipable equipped on the user.
        /// </summary>
        public List<CVEEquipable> userEquipment = new List<CVEEquipable>();

        /// <summary>
        /// An event to handle the user disconnection.
        /// </summary>
        public UMI3DUserEvent onUserDisconnection = new UMI3DUserEvent();

        /// <summary>
        /// The user avatar.
        /// </summary>
        public UMI3DAvatar avatar;

        /// <summary>
        /// The user's name.
        /// </summary>
        public string UserName;

        public AppearanceType appearance;

        public bool ImmersiveDeviceUser;

        private float TimeSinceConnectionLost = -1000;

        public QualityType quality;
        public string os;

        #region connection

        /*
        /// <summary>
        /// Link the user to a real-time connection and send an EnterDto. 
        /// </summary>
        /// <param name="connection">The real-time connection.</param>
        public void Create(IUMI3DRealtimeConnection connection)
        {
            this.connection = connection;
            userId = connection.GetId();
            var res = new EnterDto();
            res.UserID = UserId;
            if (avatar != null && avatar.navigation != null)
                res.UserPosition = avatar.navigation.Id;
            Send(res);
        }
        */

        /// <summary>
        /// Link the user to a real-time connection 
        /// </summary>
        /// <param name="connection">The real-time connection.</param>
        public void SetConnection(IUMI3DRealtimeConnection connection)
        {
            this.connection = connection;
            if (connection == null) TimeSinceConnectionLost = 0;
        }

        /// <summary>
        /// Link the user to a real-time connection and send an EnterDto. 
        /// </summary>
        /// <param name="connection">The real-time connection.</param>
        public EnterDto Create(string Id)
        {
            userId = Id;
            var res = new EnterDto();
            res.UserID = UserId;
            return res;
        }

        /// <summary>
        /// called when the connection stops
        /// </summary>
        public void Quit()
        {
            if (avatar != null)
                Destroy(avatar.gameObject);
            foreach (CVEEquipable equipable in userEquipment)
                equipable.onUnequiped.Invoke();
            onUserDisconnection.Invoke(this);
        }

        /// <summary>
        /// Send a Data Transfer Object through the real-time connection.
        /// </summary>
        /// <param name="data">The DTO to be sent.</param>
        public void Send(UMI3DDto data)
        {
            if (connection != null)
                connection.SendData(data);
        }

        /// <summary>
        /// Called when a Data Transfer Object is received by the real-time connection.
        /// </summary>
        /// <param name="e">The received DTO</param>
        public void onMessage(System.Object e)
        {
            if (e is InteractionRequestDto)
                Interact(e as InteractionRequestDto);
            else if (e is NavigationRequestDto && avatar != null)
                avatar.UpdateAvatar(e as NavigationRequestDto);
            else if (e is HoveredDto)
            {
                HoveredDto hover = e as HoveredDto;
                AbstractObject3D obj = UMI3D.Scene.GetObject(hover.abstractObject3DId);

                if ((obj != null) && obj.isInteractable)
                {
                    if (hover.State)
                    {
                        if (!obj.hoveringBones.Contains(this.UserId + hover.boneId))
                        {
                            obj.onHoverEnter.Invoke(this, hover.boneId);
                            obj.hoveringBones.Add(this.UserId + hover.boneId);
                        }

                        if (obj.GetInteractableDto(this).trackHoverPosition)
                        {
                            if ((hover.Position != null) && (hover.Normal != null))
                                obj.onHovered.Invoke(this, hover.boneId, hover.Position, hover.Normal);
                        }
                    }
                    else
                    {
                        if (obj.hoveringBones.Contains(this.UserId + hover.boneId))
                        {
                            obj.onHoverExit.Invoke(this, hover.boneId);
                            obj.hoveringBones.Remove(this.UserId + hover.boneId);
                        }
                    }
                }


            }
            else if (e is AbstractEquipeConfirmationDto)
            {
                AbstractObject3D object3D = UMI3D.Scene.GetObject((e as AbstractEquipeConfirmationDto).objectId);
                if (object3D)
                {
                    CVEEquipable equipable = object3D.gameObject.GetComponent<CVEEquipable>();
                    if (e is EquipeConfirmationDto)
                    {
                        object3D.objectPosition.DeSync(this, false);
                        object3D.objectRotation.DeSync(this, false);
                        object3D.objectScale.DeSync(this, false);
                        userEquipment.Add(equipable);
                        equipable.equipedUser = this;
                        equipable.onEquiped.Invoke();
                    }
                    else if (e is UnequipeConfirmationDto)
                    {
                        object3D.objectPosition.DeSync(this, true);
                        object3D.objectRotation.DeSync(this, true);
                        object3D.objectScale.DeSync(this, true);
                        userEquipment.Remove(equipable);
                        equipable.equipedUser = null;
                        equipable.onUnequiped.Invoke();
                    }
                }
            }
            else if (e is UpdateEquipmentTransformDto)
            {
                UpdateEquipmentTransformDto dto = e as UpdateEquipmentTransformDto;

                AbstractObject3D equipment = UMI3D.Scene.GetObject(dto.objectId);
                if (equipment != null)
                {
                    equipment.objectPosition.SetValue(dto.position);
                    equipment.objectRotation.SetValue(dto.rotation);
                    equipment.objectScale.SetValue(dto.scale);
                }
            }
        }

        #endregion


        #region Scene Loading

        /// <summary>
        /// Return a DTO containing the sub-objects of a UMI3D object.
        /// </summary>
        /// <param name="parentId">The identifier of the UMI3D object. If null, returns the UMI3D scene's roots.</param>
        public LoadDto LoadSubObjects(string parentId = null)
        {
            var list = UMI3D.Scene.GetChildren(parentId, this);
            var res = new LoadDto();
            if (list.Count > 0)
            {
                foreach (AbstractObject3D child in list)
                    if (child.VisibleFor(this))
                    {
                        var dto = child.ConvertToDto(this) as EmptyObject3DDto;
                        dto.pid = parentId;
                        res.Entities.Add(dto);
                    }
            }
            return res;
        }

        #endregion


        #region Interactions

        /// <summary>
        /// Called when an interraction request is received.
        /// </summary>
        /// <param name="request">The InteractionRequestDto DTO that contains the request parameters.</param>
        void Interact(InteractionRequestDto request)
        {
            if (request.id == null)
                return;
            AbstractInteraction interaction = UMI3D.Scene.GetInteraction(request.id);
            if (interaction != null)
                interaction.OnUserInteraction(this, (JSONObject)request.Arguments);
        }

        #endregion


        #region Navigation and Camera Management

        /// <summary>
        /// Send a navigation request through the real-time connection.
        /// </summary>
        /// <param name="position">The position of the requested camera position in the UMI3D Scene frame of reference.</param>
        public void Navigate(Vector3 position)
        {
            Send(new NavigateDto() { Position = position });
        }


        /// <summary>
        /// Send a telepartation request through the real-time connection.
        /// </summary>
        /// <param name="position">The position of the requested camera position in the UMI3D Scene frame of reference.</param>
        public void Teleport(Vector3 position)
        {
            Send(new TeleportDto() { Position = position });
        }

        #endregion


        #region update

        /// <summary>
        /// Get the loaded/removed objects and interactions since last call.
        /// </summary>
        /// <returns></returns>
        public UpdateDto GetUpdates()
        {
            UpdateDto res = new UpdateDto();

            updateInteractionsAvailability(res);
            updateObjectsVisibility(res);

            return res;
        }

        /// <summary>
        /// Get the loaded/removed objects since last call.
        /// </summary>
        /// <param name="res">The UpdateDto to be completed.</param>
        private void updateObjectsVisibility(UpdateDto res)
        {
            foreach (AbstractObject3D obj in UMI3D.Scene.Objects)
                obj.UpdateVisibilityForUser(this);

            res.LoadedObjects.Entities.AddRange(ObjectsToLoad);
            res.RemovedObjects.AddRange(ObjectsToRemove);

            ObjectsToLoad.Clear();
            ObjectsToRemove.Clear();

            foreach (AbstractObject3D obj in UMI3D.Scene.Objects)
                obj.UpdateVisibilityLastFrame(this);
        }

        /// <summary>
        /// Get the loaded/removed interactions since last call.
        /// </summary>
        /// <param name="res">The UpdateDto to be completed.</param>
        private void updateInteractionsAvailability(UpdateDto res)
        {
            foreach (CVEToolbox toolbox in UMI3D.Scene.Toolboxes)
            {
                if (toolbox.UpdateAvailabilityForUser(this))
                {
                    foreach (CVETool tool in toolbox.tools)
                    {
                        if(tool.UpdateAvailabilityForUser(this))
                        {
                            foreach (AbstractInteraction interaction in tool.Interactions)
                            {
                                interaction.UpdateAvailabilityForUser(this);
                            }
                        }
                    }
                }
            }

            res.AddedToolboxes.AddRange(ToolboxesToLoad);
            res.AddedTools.AddRange(ToolsToLoad);
            res.AddedInteractions.AddRange(InteractionsToLoad);
            res.RemovedInteractions.AddRange(ToolboxesIdsToRemove);
            res.RemovedInteractions.AddRange(ToolsIdsToRemove);
            res.RemovedInteractions.AddRange(InteractionsIdsToRemove);

            ToolboxesToLoad.Clear();
            ToolsToLoad.Clear();
            InteractionsToLoad.Clear();
            ToolboxesIdsToRemove.Clear();
            ToolsIdsToRemove.Clear();
            InteractionsIdsToRemove.Clear();

            foreach (CVEToolbox toolbox in UMI3D.Scene.Toolboxes)
            {
                toolbox.UpdateAvailabilityLastFrame(this);
                foreach (CVETool tool in toolbox.tools)
                {
                    tool.UpdateAvailabilityLastFrame(this);
                    foreach (AbstractInteraction interaction in tool.Interactions)
                    {
                        interaction.UpdateAvailabilityLastFrame(this);
                    }
                }
            }
        }

        private void UpdateEquipmentTransform(UpdateEquipmentTransformDto dto)
        {
            throw new System.NotImplementedException();
        }

        #endregion


        private void Update()
        {
            if (connection == null && UMI3D.UserManager.TimeOutTime >= 0)
            {
                TimeSinceConnectionLost += Time.deltaTime;
                if (TimeSinceConnectionLost > UMI3D.UserManager.TimeOutTime)
                {
                    StartCoroutine(UMI3D.UserManager.OnConnectionClose(UserId));
                }
            }
        }


        #region destroy

        /// <summary>
        /// Unity MonoBehaviour method.
        /// </summary>
        void OnDestroy()
        {
            if (avatar != null)
                Destroy(avatar.gameObject);
            foreach (CVEEquipable equipable in userEquipment)
                equipable.onUnequiped.Invoke();
            onUserDisconnection.Invoke(this);
        }
        #endregion
    }
}
