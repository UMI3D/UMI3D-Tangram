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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using umi3d.edk;
using umi3d.common;
using umi3d.edk.interaction;

public class TangramModificationListener: MonoBehaviour
{

    UMI3DScene[] scenes;
    List<UMI3DNode> nodes;
    public float time = 0f;
    float timeTmp = 0;
    public int max = 0;
    public int maxPerNode = 0;

    Transaction sceneTransaction = new Transaction();
    Dictionary<UMI3DNode, Transaction> nodeTransactions = new Dictionary<UMI3DNode, Transaction>();

    static TangramModificationListener instance = null;

    public static TangramModificationListener Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(TangramModificationListener)) as TangramModificationListener;
            }

            if (instance == null)
            {
                GameObject obj = new GameObject("DonutSceneListener");
                instance = obj.AddComponent<TangramModificationListener>();
            }
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //TangrameGameManager

        //DonutManager.AddingNode.AddListener(AddNode);
        //DonutManager.RemovingNode.AddListener(RemoveNode);
        //DonutManager.AddingOperation.AddListener(AddInterableOperation);
        nodes = GetComponentsInChildren<UMI3DNode>().ToList();

        foreach (UMI3DNode node in nodes)
        {
            nodeTransactions.Add(node, new Transaction());
        }

        scenes = GetComponentsInChildren<UMI3DScene>();

    }

    // Update is called once per frame
    void Update()
    {
        if (UMI3DServer.Exists)
        {
            foreach (var node in nodes)
                Update(node);

            foreach (var scene in scenes)
                MaterialUpdate(scene);

            Dispatch();
        }
    }

    public void AddNode(UMI3DNode node)
    {
        if (!nodes.Contains(node))
        {
            //transaction.Operations.Add(node.Register());

            //UMI3DServer.Dispatch(new Transaction
            //{
            //    Operations = new List<Operation> { node.Register() }
            //});

            nodeTransactions.Add(node, new Transaction() { Operations = new List<Operation> { node.Register() } });
            nodes.Add(node);

            if (node.TryGetComponent<UMI3DInteractable>(out UMI3DInteractable interactable))
            {
                LoadEntity loadEntity = interactable.Register();
                nodeTransactions[node].Operations.Add(loadEntity);
            }

        }
    }

    void RemoveNode(UMI3DNode node)
    {
        if (nodes.Contains(node))
        {
            nodeTransactions.Remove(node);
            nodes.Remove(node);

            DeleteEntity op = new DeleteEntity
            {
                entityId = node.Id()
            };
            op += UMI3DEnvironment.GetEntities<UMI3DUser>();

            //transaction.Operations.Add(op);

            UMI3DServer.Dispatch(new Transaction
            {
                Operations = new List<Operation> { op }
            });
        }
        else
        {
            throw new System.Exception("Unknown Node");
        }
    }

    public void AddInterableOperation(UMI3DNode node, SetEntityProperty op)
    {
        if (op != null)
            nodeTransactions[node].Operations.Add(op);
    }

    public void AddSceneOperation(SetEntityProperty op)
    {
        if (op != null)
            sceneTransaction.Operations.Add(op);
    }

    void Dispatch()
    {
        if (checkTime() || checkMax())
        {
            foreach (KeyValuePair<UMI3DNode, Transaction> item in nodeTransactions)
            {
                if (item.Value.Operations.Count() >= maxPerNode)
                {
                    item.Value.Simplify();
                    item.Value.reliable = false;
                    UMI3DServer.Dispatch(item.Value);
                    item.Value.Operations.Clear();
                }

                if (sceneTransaction.Operations.Count() >= maxPerNode)
                {
                    sceneTransaction.Simplify();
                    sceneTransaction.reliable = false;
                    UMI3DServer.Dispatch(sceneTransaction);
                    sceneTransaction.Operations.Clear();
                }

            }

            //transaction.Simplify();
            //transaction.reliable = false;
            //UMI3DServer.Dispatch(transaction);
            //transaction.Operations.Clear();
        }
    }

    bool checkTime()
    {
        timeTmp -= Time.deltaTime;
        if (time == 0 || timeTmp <= 0)
        {
            timeTmp = time;
            return true;
        }

        return false;
    }

    bool checkMax()
    {
        return max != 0 && checkCount() > max;
    }

    int checkCount()
    {
        //return transaction.Operations.Count();

        int res = 0;
        foreach (KeyValuePair<UMI3DNode, Transaction> item in nodeTransactions)
        {
            res += item.Value.Operations.Count();
        }
        return res;
    }

    // Clean this
    private void Update(UMI3DNode obj)
    {
        setOperation(obj.objectPosition.SetValue(obj.transform.localPosition));
        setOperation(obj.objectRotation.SetValue(obj.transform.localRotation));
        setOperation(obj.objectScale.SetValue(obj.transform.localScale));
        //setOperation(obj.objectXBillboard.SetValue(obj.xBillboard));
        //setOperation(obj.objectYBillboard.SetValue(obj.yBillboard));
        //if (obj is UIRect)
        //    UIUpdate(obj as UIRect);

        ModelUpdate(obj);
    }

    // Clean this
    private void ModelUpdate(UMI3DNode obj)
    {
        setOperation(obj.objectColliderRadius.SetValue(obj.colliderRadius));
        setOperation(obj.objectColliderCenter.SetValue(obj.colliderCenter));
        setOperation(obj.objectColliderBoxSize.SetValue(obj.colliderBoxSize));
        setOperation(obj.objectColliderDirection.SetValue(obj.colliderDirection));
        setOperation(obj.objectColliderHeight.SetValue(obj.colliderHeight));
        setOperation(obj.objectColliderType.SetValue(obj.colliderType));
        setOperation(obj.objectCustomMeshCollider.SetValue(obj.customMeshCollider));
        setOperation(obj.objectHasCollider.SetValue(obj.hasCollider));
        setOperation(obj.objectIsConvexe.SetValue(obj.convex));
        setOperation(obj.objectIsMeshCustom.SetValue(obj.isMeshCustom));
    }

    // Clean this after rating poll created
    private void UIUpdate(UIRect obj)
    {
        var rectTransform = obj.GetComponent<RectTransform>();
        setOperation(obj.AnchoredPosition.SetValue(rectTransform.anchoredPosition));
        setOperation(obj.AnchoredPosition3D.SetValue(rectTransform.anchoredPosition3D));
        setOperation(obj.AnchorMax.SetValue(rectTransform.anchorMax));
        setOperation(obj.AnchorMin.SetValue(rectTransform.anchorMin));
        setOperation(obj.OffsetMax.SetValue(rectTransform.offsetMax));
        setOperation(obj.OffsetMin.SetValue(rectTransform.offsetMin));
        setOperation(obj.Pivot.SetValue(rectTransform.pivot));
        setOperation(obj.SizeDelta.SetValue(rectTransform.sizeDelta));
        setOperation(obj.RectMask.SetValue(obj.GetComponent<RectMask2D>() != null));
        if (obj is UICanvas)
        {
            var canvas = obj as UICanvas;
            var canvasScaler = obj.GetComponent<CanvasScaler>();
            var Canvas = obj.GetComponent<Canvas>();
            setOperation(canvas.DynamicPixelPerUnit.SetValue(canvasScaler.dynamicPixelsPerUnit));
            setOperation(canvas.ReferencePixelPerUnit.SetValue(canvasScaler.referencePixelsPerUnit));
            setOperation(canvas.OrderInLayer.SetValue(Canvas.sortingOrder));
        }
        if (obj is UIImage)
        {
            var image = obj as UIImage;
            var Image = obj.GetComponent<Image>();
            setOperation(image.Color.SetValue(Image.color));
            setOperation(image.ImageType.SetValue(Image.type));
            //update sprite
        }

        if (obj is UIText)
        {
            var text = obj as UIText;
            var Text = obj.GetComponent<Text>();
            setOperation(text.Alignment.SetValue(Text.alignment));
            setOperation(text.AlignByGeometry.SetValue(Text.alignByGeometry));
            setOperation(text.TextColor.SetValue(Text.color));
            setOperation(text.TextFont.SetValue(Text.font));
            setOperation(text.FontSize.SetValue(Text.fontSize));
            setOperation(text.FontStyle.SetValue(Text.fontStyle));
            setOperation(text.HorizontalOverflow.SetValue(Text.horizontalOverflow));
            setOperation(text.VerticalOverflow.SetValue(Text.verticalOverflow));
            setOperation(text.LineSpacing.SetValue(Text.lineSpacing));
            setOperation(text.ResizeTextForBestFit.SetValue(Text.resizeTextForBestFit));
            setOperation(text.ResizeTextMaxSize.SetValue(Text.resizeTextMaxSize));
            setOperation(text.ResizeTextMinSize.SetValue(Text.resizeTextMinSize));
            setOperation(text.SupportRichText.SetValue(Text.supportRichText));
            setOperation(text.Text.SetValue(Text.text));
        }
    }

    private void MaterialUpdate(UMI3DScene scene)
    {
        //if (sets == null) sets = new Dictionary<string, Dictionary<string, SetEntityProperty>>();

        // Add check material

        foreach (MaterialSO mat in scene.materialSOs)
        {
            //if (!sets.ContainsKey(mat.Id())) sets[mat.Id()] = new Dictionary<string, SetEntityProperty>();
            if (mat as PBRMaterial)
            {
                AddSceneOperation(((PBRMaterial)mat).objectBaseColorFactor.SetValue(((PBRMaterial)mat).baseColorFactor));
                AddSceneOperation(((PBRMaterial)mat).objectEmissiveFactor.SetValue(((PBRMaterial)mat).emissive));
                AddSceneOperation(((PBRMaterial)mat).objectEmissiveTexture.SetValue(((PBRMaterial)mat).textures.emissiveTexture));
                AddSceneOperation(((PBRMaterial)mat).objectHeightTexture.SetValue(((PBRMaterial)mat).textures.heightTexture));
                AddSceneOperation(((PBRMaterial)mat).objectHeightTextureScale.SetValue(((PBRMaterial)mat).textures.heightTexture.scale));
                AddSceneOperation(((PBRMaterial)mat).objectMaintexture.SetValue(((PBRMaterial)mat).textures.baseColorTexture));
                AddSceneOperation(((PBRMaterial)mat).objectMetallicFactor.SetValue(((PBRMaterial)mat).metallicFactor));
                AddSceneOperation(((PBRMaterial)mat).objectMetallicRoughnessTexture.SetValue(((PBRMaterial)mat).textures.metallicRoughnessTexture));
                AddSceneOperation(((PBRMaterial)mat).objectMetallicTexture.SetValue(((PBRMaterial)mat).textures.metallicTexture));
                AddSceneOperation(((PBRMaterial)mat).objectNormalTexture.SetValue(((PBRMaterial)mat).textures.normalTexture));
                AddSceneOperation(((PBRMaterial)mat).objectNormalTextureScale.SetValue(((PBRMaterial)mat).textures.normalTexture.scale));
                AddSceneOperation(((PBRMaterial)mat).objectOcclusionTexture.SetValue(((PBRMaterial)mat).textures.occlusionTexture));
                AddSceneOperation(((PBRMaterial)mat).objectRoughnessFactor.SetValue(((PBRMaterial)mat).roughnessFactor));
                AddSceneOperation(((PBRMaterial)mat).objectRoughnessTexture.SetValue(((PBRMaterial)mat).textures.roughnessTexture));
                AddSceneOperation(((PBRMaterial)mat).objectShaderProperties.SetValue(((PBRMaterial)mat).shaderProperties));
                AddSceneOperation(((PBRMaterial)mat).objectTextureTilingOffset.SetValue(((PBRMaterial)mat).tilingOffset));
                AddSceneOperation(((PBRMaterial)mat).objectTextureTilingScale.SetValue(((PBRMaterial)mat).tilingScale));
            }
            else
            {
                Debug.LogWarning("unsupported material type");
            }
        }
    }
    public void setOperation(SetEntityProperty operation)
    {
        if (operation != null)
        {
            UMI3DNode node = UMI3DEnvironment.GetEntity<UMI3DNode>(operation.entityId);
            if (node != null)
                nodeTransactions[node].Operations.Add(operation);
            //transaction.Operations.Add(operation);
        }
    }
}
