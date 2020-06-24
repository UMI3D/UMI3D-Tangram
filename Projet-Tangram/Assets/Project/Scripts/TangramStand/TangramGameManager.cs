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
using UnityEngine.UI;
using umi3d.common;
using umi3d.edk;

public class TangramGameManager : MonoBehaviour
{
    public List<TangramPiece> pieces = new List<TangramPiece>(7);
    public bool randomisePieces = false;

    public Communication communication;

    public Dropdown dropdown;

    public Teleporter resetSpawnPoint;
    public GameObject prefabBlueCommunicator;
    public GameObject prefabRedCommunicator;

    public GameObject prefabBlueGlyph;
    public GameObject prefabRedGlyph;

    public GameObject blueButtonsAnchor;
    public GameObject redButtonsFeedbackAnchor;
    public GameObject redButtonsDirectionAnchor;
    public GameObject redButtonsRotationAnchor;

    public GameObject buttonPrefab;

    public float glyphDuration = 10f; 
    public float alertDuration = 10f;
    public string defaultAnswer = "";

    public List<string> blueExpressions = new List<string>();
    public List<string> redExpressions = new List<string>();

    public List<Sprite> blueTextures = new List<Sprite>();
    public List<Sprite> redFeedbackTextures = new List<Sprite>();
    public List<Sprite> redDirectionTextures = new List<Sprite>();
    public List<Sprite> redRotationTextures = new List<Sprite>();

    private int visibilityIndex = 0;

    private static TangramGameManager instance;

    private GameObject blueCom;
    private GameObject redCom;

    private Image blueGlyph;
    private Image redGlyph;

public static TangramGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TangramGameManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<TangramGameManager>();
                    Debug.Log("New Tangram Game Manager");
                }
            }
            return instance;
        }
    }

    void Start()
    {
        if (randomisePieces)
            pieces.Shuffle();
    }

    private Dictionary<string, UserRole> roleDictionary = new Dictionary<string, UserRole>();

    public UserRole? getRole(string userId)
    {
        if (roleDictionary.ContainsKey(userId))
            return roleDictionary[userId];
        else
            return null;
    }

    public void RemoveUser(string userId)
    {
        if (roleDictionary.ContainsKey(userId))
            roleDictionary.Remove(userId);
    }

    public void AddBluePill(UMI3DUser user, string bone)
    {
        if (!roleDictionary.ContainsValue(UserRole.BluePill) && !roleDictionary.ContainsKey(user.UserId))
        {
            roleDictionary.Add(user.UserId, UserRole.BluePill);

            switch (communication)
            {
                case Communication.None:
                    break;
                case Communication.Voice:
                    break;
                case Communication.Glyphs:
                    blueGlyph = (Instantiate(prefabBlueGlyph, user.avatar.anchor.transform)).GetComponentInChildren<Image>();
                    ArrangeChildren(blueButtonsAnchor.transform, blueTextures, UserRole.BluePill);
                    break;
                case Communication.Expressions:
                    blueCom = Instantiate(prefabBlueCommunicator, user.avatar.anchor.transform);
                    blueCom.GetComponent<StringEnumParameter>().options = redExpressions;
                    blueCom.GetComponent<TangramNotification>().duration = alertDuration;
                    blueCom.GetComponent<TangramNotification>().defaultAnswer = defaultAnswer;
                    break;
                default:
                    break;
            }
        }
    }

    public void AddRedPill(UMI3DUser user, string bone)
    {
        if (!roleDictionary.ContainsValue(UserRole.RedPill) && !roleDictionary.ContainsKey(user.UserId))
        {
            roleDictionary.Add(user.UserId, UserRole.RedPill);

            switch (communication)
            {
                case Communication.None:
                    break;
                case Communication.Voice:
                    break;
                case Communication.Glyphs:
                    redGlyph = (Instantiate(prefabRedGlyph, user.avatar.anchor.transform)).GetComponentInChildren<Image>();
                    ArrangeChildren(redButtonsFeedbackAnchor.transform, redFeedbackTextures, UserRole.RedPill);
                    ArrangeChildren(redButtonsDirectionAnchor.transform, redDirectionTextures, UserRole.RedPill);
                    ArrangeChildren(redButtonsRotationAnchor.transform, redRotationTextures, UserRole.RedPill);
                    break;
                case Communication.Expressions:
                    redCom = Instantiate(prefabRedCommunicator, user.avatar.anchor.transform);
                    redCom.GetComponent<StringEnumParameter>().options = blueExpressions;
                    redCom.GetComponent<TangramNotification>().duration = alertDuration;
                    redCom.GetComponent<TangramNotification>().defaultAnswer = defaultAnswer;
                    break;
                default:
                    break;
            }
        }
    }

    public void RemoveRole(UMI3DUser user, string bone)
    {
        UserRole role = roleDictionary[user.UserId];

        if (role.Equals(UserRole.BluePill))
        {
            switch (communication)
            {
                case Communication.None:
                    break;
                case Communication.Voice:
                    break;
                case Communication.Glyphs:
                    Destroy(blueGlyph.transform.parent.gameObject);

                    foreach (Transform child in blueButtonsAnchor.transform)
                        Destroy(child.gameObject);

                    break;
                case Communication.Expressions:
                    Destroy(blueCom);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (communication)
            {
                case Communication.None:
                    break;
                case Communication.Voice:
                    break;
                case Communication.Glyphs:
                    Destroy(redGlyph.transform.parent.gameObject);

                    foreach (Transform child in redButtonsFeedbackAnchor.transform)
                        Destroy(child.gameObject);

                    foreach (Transform child in redButtonsDirectionAnchor.transform)
                        Destroy(child.gameObject);

                    foreach (Transform child in redButtonsRotationAnchor.transform)
                        Destroy(child.gameObject);

                    break;
                case Communication.Expressions:
                    Destroy(redCom);
                    break;
                default:
                    break;
            }
        }

        RemoveUser(user.UserId);
    }

    public void NextPiece(UMI3DUser user, string bone)
    {
        if (visibilityIndex == 0 || (visibilityIndex < pieces.Count && pieces[visibilityIndex - 1].hasBeenPlaced))
        {
            pieces[visibilityIndex].isVisible = true;
            visibilityIndex++;
        }
    }

    private void Reset()
    {
        foreach (TangramPiece piece in pieces)
        {
            StopAllCoroutines(); 
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localRotation = Quaternion.identity;
            piece.isVisible = false;
            piece.hasBeenPlaced = false;
            roleDictionary.Clear();

            switch (communication)
            {
                case Communication.None:
                    break;
                case Communication.Voice:
                    break;
                case Communication.Glyphs:

                    if (blueGlyph != null)
                        Destroy(blueGlyph.transform.parent.gameObject);

                    if (redGlyph != null)
                        Destroy(redGlyph.transform.parent.gameObject);

                    foreach (Transform child in blueButtonsAnchor.transform)   
                        Destroy(child.gameObject); 
                    
                    foreach (Transform child in redButtonsFeedbackAnchor.transform)
                        Destroy(child.gameObject);

                    foreach (Transform child in redButtonsDirectionAnchor.transform)
                        Destroy(child.gameObject);

                    foreach (Transform child in redButtonsRotationAnchor.transform)
                        Destroy(child.gameObject);

                    break;
                case Communication.Expressions:
                    Destroy(blueCom);
                    Destroy(redCom);
                    break;
                default:
                    break;
            }
            communication = Communication.None;
            visibilityIndex = 0;
        }

        foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
        {
            resetSpawnPoint.TeleportUser(user, resetSpawnPoint.transform);
        }
    }

    public void ResetTangram()
    {
        Reset();
        Start();
        dropdown.value = 0;    
    }

    public void SetCommunication(Text mode)
    {
        Reset();
        communication = (Communication)System.Enum.Parse(typeof(Communication), mode.text);
    }

    public void TransmitGlyph(UMI3DUser user, Image img)
    {
        Image glyph;

        UserRole role = roleDictionary[user.UserId];

        if (role.Equals(UserRole.BluePill))
            glyph = blueGlyph;

        else
            glyph = redGlyph;

        GlyphDisplay(glyph, img);
    }

    public void GlyphDisplay(Image glyph, Image img)
    {
        glyph.GetComponent<UIImage>().sprite.Path = img.GetComponent<UIImage>().sprite.Path;
        Debug.Log(glyph.GetComponent<UIImage>().sprite.Path);
        glyph.color = new Color(1, 1, 1, 1);
        glyph.sprite = img.sprite;
        StartCoroutine("GlyphDisappearing", glyph);
    }

    IEnumerator GlyphDisappearing(Image glyph)
    {
        float duration = glyphDuration;
        while (duration >= 0.2f)
        {
            duration -= Time.deltaTime; 
            yield return null;
        }

        glyph.color = new Color(1, 1, 1, 0);
//        glyph.GetComponent<UIImage>().sprite.Path = "";
//        glyph.sprite = null;
    }

    private const int Columns = 3;
    private const float Space = 0.12f;

    private void ArrangeChildren(Transform buttonAnchor, List<Sprite> sprites, UserRole role)
    {
        Transform[] children;

        children = new Transform[sprites.Count];

        for (int i = 0; i < children.Length; i++)
        {
            children[i] = Instantiate(buttonPrefab).transform;
            children[i].localScale = 0.39f * children[i].localScale;
            children[i].parent = buttonAnchor;
            children[i].localRotation = Quaternion.identity;
            int row = i / Columns;
            int column = i % Columns;
            children[i].localPosition = new Vector3(-column * 0.55f * Space, -row * 0.7f * Space, 0);

            PillSeeFilter filter = children[i].gameObject.AddComponent<PillSeeFilter>();
            filter.userRole = role;
            filter.canSee = true;

            children[i].GetComponentInChildren<Image>().sprite = sprites[i];
            children[i].GetComponentInChildren<UIImage>().sprite.IsLocalFile = true;
            children[i].GetComponentInChildren<UIImage>().sprite.Path = "/Textures/Tangram/" + sprites[i].name + ".png";
            children[i].GetComponentInChildren<TangramNotification>().duration = alertDuration;
        }
    }

    private void Awake()
    {
        UMI3D.OnUserQuit.AddListener((UMI3DUser user) =>
        {
            roleDictionary.Remove(user.UserId);
        });
    }

}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random randomInt = new System.Random();
        for (var index = 0; index < list.Count; index++)
            list.Swap(index, randomInt.Next(index, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var tmp = list[i];
        list[i] = list[j];
        list[j] = tmp;
    }
}

public enum UserRole
{
    RedPill,
    BluePill
}

public enum Communication
{
    None,
    Voice, 
    Glyphs,
    Expressions
}