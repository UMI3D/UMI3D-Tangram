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
using System.Collections.Generic;

namespace umi3d.common
{
    [Serializable]
    public class RiggedObjectRotation : UMI3DDto
    {
        public string objectName;
        public SerializableQuaternion objectRotation;
    }

    public class RiggedAvatarDto : AbstractEntityDto
    {
        public List<RiggedObjectRotation> riggedObjects;
        public bool isGLTF;

        public RiggedAvatarDto()
        {
            riggedObjects = new List<RiggedObjectRotation>();
            isGLTF = false;
        }

        public int Count()
        {
            return this.riggedObjects.Count;
        }

        public void Clear()
        {
            this.riggedObjects.Clear();
        }

        public bool TryAdd(RiggedObjectRotation riggedObjectRotation)
        {
            foreach (RiggedObjectRotation obj in riggedObjects)
            {
                if (obj.objectName == riggedObjectRotation.objectName)
                {
                    return false;
                }
            }
            this.riggedObjects.Add(riggedObjectRotation);
            return true;
        }
    }
}
