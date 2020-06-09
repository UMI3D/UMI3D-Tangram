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

public class TangramNotification : MonoBehaviour
{
    public float duration = 10f;
    public string defaultAnswer = "";

    public void SendGlobalNotification(UMI3DUser user, string message)
    {
        if (message != defaultAnswer)
        {
            NotificationDto notif = new NotificationDto()
            {
                title = user.UserName,
                content = message,
                duration = duration,
            };

            foreach (UMI3DUser presentUser in UMI3D.UserManager.GetUsers())
            {
                UMI3DNotifier.Notify(presentUser, notif);
            }
        }
    }

    public void SendGlyphNotification(UMI3DUser user)
    {
        NotificationDto notif = new NotificationDto()
        {
            title = user.UserName,
            content = "has sent a glyph !",
            duration = duration,
        };

        foreach (UMI3DUser presentUser in UMI3D.UserManager.GetUsers())
        {
            UMI3DNotifier.Notify(presentUser, notif);
        }
    }
}
