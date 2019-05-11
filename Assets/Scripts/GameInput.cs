using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class InputHandling
{
    static int lastRot_ = 0;
    static int lastMov_ = 0;

    const float repeatDelay_ = .08f;
    static float repeatDelayTimer_ = 0;

    const float defaultFallDelay_ = 1f;
    const float fastFallDelay_ = .065f;

    const float normalFallDelay_ = defaultFallDelay_;
    static float currentFallDelay_ = defaultFallDelay_;

    static float timer_ = defaultFallDelay_;
    static bool isFastFalling_ = false;


    public static int GetRotationInput()
    {
        int rot = (int)Input.GetAxisRaw("Rotate");

        if (rot == lastRot_ && lastRot_ != 0)
            rot = 0;
        else
            lastRot_ = rot;

        return rot;
    }

    public static float GetFallTimer()
    {
        bool fastFall = Input.GetAxisRaw("Vertical") == -1;

        if (fastFall && fastFallDelay_ < normalFallDelay_ && !isFastFalling_)
        {
            timer_ = math.remap(0, normalFallDelay_, 0, fastFallDelay_, timer_);
            currentFallDelay_ = fastFallDelay_;
            isFastFalling_ = true;
        }

        if (!fastFall && isFastFalling_)
        {
            timer_ = math.remap(0, fastFallDelay_, 0, normalFallDelay_, timer_);
            currentFallDelay_ = normalFallDelay_;
            isFastFalling_ = false;
        }

        timer_ -= Time.deltaTime;

        float t = timer_;

        if (timer_ <= 0)
            timer_ = currentFallDelay_;

        return t;
    }

    public static int GetHorizontalInput()
    {
        int mov = (int)Input.GetAxisRaw("Horizontal");

        if (mov == lastMov_ && lastMov_ != 0)
        {
            if (repeatDelayTimer_ > 0f)
                mov = 0;
            else
                repeatDelayTimer_ = repeatDelay_;

            repeatDelayTimer_ -= Time.deltaTime;
        }
        else
        {
            repeatDelayTimer_ = repeatDelay_;
            lastMov_ = mov;
        }

        return mov;
    }
}
