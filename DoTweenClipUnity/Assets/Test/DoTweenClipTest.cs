using System;
using System.Collections;
using System.Collections.Generic;
using Carotaa.Code;
using DG.Tweening;
using UnityEngine;

public class DoTweenClipTest : MonoBehaviour
{
    public DoTweenClip m_Clip;

    private void Start()
    {
        var tweener = transform.DoAnimationClip(m_Clip);
        tweener.SetLoops(-1);
    }
}
