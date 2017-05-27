// Copyright © Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;

public class LoadingIconAnimator : MonoBehaviour
{
    private class Constants
    {
        public const float RotateTime = 0.05f;
        public const float RotateAngle = -18f;
    }

    private float time = 0;

    private void Update()
    {
        if (gameObject.activeSelf && time > Constants.RotateTime)
        {
            Rotate();
            time = 0;
        }
        time += Time.deltaTime;
    }

    private void Rotate()
    {
        gameObject.transform.Rotate(0f, 0f, Constants.RotateAngle);
    }
}
