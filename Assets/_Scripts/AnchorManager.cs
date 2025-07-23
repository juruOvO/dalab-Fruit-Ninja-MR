using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class AnchorManager : MonoBehaviour
{
    public GameObject LogTextMesh;
    public GameObject anchorPrefab;
    private float currDriftDelay = 0;
    private float maxDriftDelay = 0.5f;

    public float FloorOffset = 0.5f;

    public GameObject OnFloorObject;

    public GameObject floorPrefab;
    public GameObject ceilingPrefab;

    private List<Transform> wallAnchors = new List<Transform>();
    private Transform ceilingTransform;
    private Transform floorTransform;

    private Dictionary<ulong, Transform> anchorList = new Dictionary<ulong, Transform>();
    private void FixedUpdate()
    {
        HandleSpacialDrift();
    }

    void Start()
    {
        LoadSpaceData();
        ObjectPlaceOnFloor();
    }

    private void Onnable()
    {
        PXR_Manager.AnchorEntityLoaded += AnchorEntityLoaded;
    }

    private void ObjectPlaceOnFloor()
    {
        if (OnFloorObject == null) return;
        var ObjectHandle = Instantiate(OnFloorObject);
        ObjectHandle.transform.position.Set(0, floorTransform.position.y + FloorOffset, 0);
    }

    private void HandleSpacialDrift()
    {
        if (anchorList.Count == 0) return;
        currDriftDelay += Time.deltaTime;
        if (currDriftDelay > maxDriftDelay)
        {
            currDriftDelay = 0;
            foreach (var handlePair in anchorList)
            {
                var handle = handlePair.Key;
                var handleObj = handlePair.Value;
                if (handle == UInt64.MinValue)
                {
                    Debug.LogError("Handle is null");
                    continue;
                }
                PXR_MixedReality.GetAnchorPose(handle, out var rotation, out var position);
                handleObj.transform.rotation = rotation;
                handleObj.transform.position = position;
            }
        }
    }

    private void LoadSpaceData()
    {
        PxrSpatialSceneDataTypeFlags[] flags = {
            PxrSpatialSceneDataTypeFlags.Ceiling,
            PxrSpatialSceneDataTypeFlags.Door,
            PxrSpatialSceneDataTypeFlags.Floor,
            PxrSpatialSceneDataTypeFlags.Object,
            PxrSpatialSceneDataTypeFlags.Opening,
            PxrSpatialSceneDataTypeFlags.Unknown,
            PxrSpatialSceneDataTypeFlags.Wall,
            PxrSpatialSceneDataTypeFlags.Window
        };

        PXR_MixedReality.LoadAnchorEntityBySceneFilter(flags, out var taskId);

    }

    private void AnchorEntityLoaded(PxrEventAnchorEntityLoaded result)
    {
        if (result.result == PxrResult.SUCCESS && result.count != 0)
        {
            PXR_MixedReality.GetAnchorEntityLoadResults(result.taskId, result.count, out var loadedAnchors);
            foreach (var key in loadedAnchors.Keys)
            {
                GameObject anchorObject = Instantiate(anchorPrefab);
                PXR_MixedReality.GetAnchorPose(key, out var orientation, out var position);
                anchorObject.transform.position = position;
                anchorObject.transform.rotation = orientation;
                Anchor anchor = anchorObject.GetComponent<Anchor>();
                if (anchor == null)
                {
                    anchorObject.AddComponent<Anchor>();
                }
                anchorList.Add(key, anchorObject.transform);
                PxrResult labelResult = PXR_MixedReality.GetAnchorSceneLabel(key, out var label);
                if (labelResult == PxrResult.SUCCESS)
                {
                    anchor.UpdateLabel(label.ToString());

                    switch (label)
                    {
                        case PxrSceneLabel.Wall:
                            {
                                PXR_MixedReality.GetAnchorPlaneBoundaryInfo(key, out var center, out var extent);
                                Transform wallTransform = anchorObject.transform;
                                wallTransform.SetParent(anchorObject.transform);
                                wallTransform.localPosition = Vector3.zero;
                                wallTransform.localRotation = Quaternion.identity;
                                wallTransform.localScale = new Vector3(extent.x, extent.y, 0.001f);
                                wallAnchors.Add(wallTransform);
                            }
                            break;
                        case PxrSceneLabel.Floor:
                            {
                                // if (floorPrefab == null)
                                // {
                                //     floorTransform = anchorObject.transform;
                                // }
                                // else
                                // {
                                //     var floor = Instantiate(floorPrefab);
                                //     floor.transform.SetParent(anchorObject.transform);
                                //     floor.transform.localPosition = Vector3.zero;
                                //     floor.transform.localRotation = Quaternion.identity;
                                //     floorTransform = floor.transform;
                                // }
                                floorTransform = anchorObject.transform;
                            }
                            break;
                        case PxrSceneLabel.Ceiling:
                            {
                                // if (ceilingTransform == null)
                                // {
                                //     ceilingTransform = anchorObject.transform;
                                // }
                                // else
                                // {
                                //     var ceiling = Instantiate(ceilingPrefab);
                                //     ceiling.transform.SetParent(anchorObject.transform);
                                //     ceiling.transform.localPosition = Vector3.zero;
                                //     ceiling.transform.localRotation = Quaternion.identity;
                                //     ceilingTransform = ceiling.transform;
                                // }
                                ceilingTransform = anchorObject.transform;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            //ScaleFloorCeiling();
            // if (floorPrefab != null)
            // {
            //     var floor = Instantiate(floorPrefab);
            //     floor.transform.SetParent(floorTransform);
            // }
            // if (ceilingPrefab != null)
            // {
            //     var ceiling = Instantiate(ceilingPrefab);
            //     ceiling.transform.SetParent(ceilingTransform);
            // }
        }
    }

    private void ScaleFloorCeiling()
    {
        if (wallAnchors.Count == 0) return;
        Vector3 extent = CalcScaleSides_Angle();
        ceilingTransform.localScale = new Vector3(extent.x * 1.1f, extent.y * 1.1f, 1);
        floorTransform.localScale = new Vector3(extent.x * 1.1f, extent.y * 1.1f, 1);

        Transform longwall = wallAnchors[(int)extent.z];
        float angle = Vector3.Angle(floorTransform.transform.up, longwall.transform.right);
        int direction = -1;
        float value = Vector3.Dot(floorTransform.up, longwall.transform.right);
        if (value < 0) direction = 1;
        if (angle == 90)
        {
            direction = 1;
        }
        floorTransform.Rotate(0, 0, direction * angle);
        ceilingTransform.rotation = floorTransform.rotation;
    }

    private Vector3 CalcScaleSides_Angle()
    {
        List<(float distance, int index1, int index2)> distances = new List<(float, int, int)>();
        for (int i = 0; i < wallAnchors.Count; ++i)
        {
            for (int j = i + 1; j < wallAnchors.Count; ++j)
            {
                float distance = Vector3.Distance(wallAnchors[i].position, wallAnchors[j].position);
                distances.Add((distance, i, j));
            }
        }

        distances.Sort((x, y) => y.distance.CompareTo(x.distance));
        var longestPair = distances[0];
        var remainingIndices = new List<int> { 1, 2, 3, 4 };
        remainingIndices.Remove(longestPair.index1);
        remainingIndices.Remove(longestPair.index2);
        float width = Vector3.Distance(wallAnchors[remainingIndices[0]].position, wallAnchors[remainingIndices[1]].position);
        float depth = Vector3.Distance(wallAnchors[longestPair.index1].position, wallAnchors[longestPair.index2].position);

        return new Vector3(width, depth, remainingIndices[0]);
    }
}
