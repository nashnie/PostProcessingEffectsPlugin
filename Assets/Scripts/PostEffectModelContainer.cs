﻿using UnityEngine;

[CreateAssetMenu(fileName = "PostEffectModelContainer", menuName = "Configs/PostEffectModelContainer")]
public class PostEffectModelContainer : ScriptableObject
{
    public BlurEffectModel blurEffectModel = new BlurEffectModel();
}
