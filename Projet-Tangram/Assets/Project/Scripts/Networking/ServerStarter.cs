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

using System;
using System.Collections;
using umi3d.common;
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using UnityEngine.UI;

public class ServerStarter : MonoBehaviour
{

    private void Start()
    {
        OnStart();
    }

    [ContextMenu("start")]
    void OnStart()
    {
        UMI3DServer.Instance.Init();
    }

    [ContextMenu("stop")]
    void OnStop()
    {
        UMI3DCollaborationServer.Stop();
    }

    public void UpdateIP(Text text)
    {
        text.text = UMI3DCollaborationServer.Instance.ip;
    }

    public void UpdatePort(Text port)
    {
        port.text = UMI3DCollaborationServer.Instance.httpPort.ToString();
    }

    public void UpdatePin(Text pin)
    {
        pin.text = (UMI3DCollaborationServer.Instance.Identifier as PinIdentifierApi).Pin;
    }

}