using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Animation
{
    static readonly string spritesRoot = "Sprites/";
    [SerializeField] public int frameCount { get; private set; }
    public bool looping { get; private set; }
    public int currentFrame = 0; //Current frame in the given animation
    public Sprite[] frames;
    public float speed { get; private set; } //Number of frames in game to pass before incrementing a frame of the animation
    public float invSpeed { get; private set; } //Number of frames in game to pass before incrementing a frame of the animation

    public Animation(string resourcePath, bool loops = true, float speed = 1.0f)
    {
        looping = loops;
        this.speed = speed;
        this.invSpeed = 1.0f / speed;

        //Load the animation resource
        frames = Resources.LoadAll<Sprite>(spritesRoot + resourcePath);
        if(frames == null) //If the loading failed
        {
            Debug.Log("Failed to load animation from: " + resourcePath);
            frameCount = 0;
        }
        else
            frameCount = frames.Length;
    }
}

[System.Serializable] class AnimationMetadata
{
    [SerializeField] public string name;
    [SerializeField] public string resourcePath;
    [SerializeField] public bool loops = true;
    [SerializeField] public float speed = 1.0f;
}

[System.Serializable]
public class AnimationHandler
{
    Dictionary<string, Animation> animationDictionary;
    Animation currentAnimation;
    [SerializeField] string defaultAnimationName;
    [SerializeField] AnimationMetadata[] animations;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] bool playAnimation = true;
    double timePassed;

    public void Initialize()
    {
        timePassed = 0.0;

        //Add all of the animations to the animation dictionary from their resources
        animationDictionary = new Dictionary<string, Animation>();
        for (int i = 0; i < animations.Length; i++)
            animationDictionary[animations[i].name] = new Animation(animations[i].resourcePath, 
                animations[i].loops, animations[i].speed);

        //Set the starting animation
        ChangeAnimation(defaultAnimationName);
    }
    public void OnUpdate()
    {
        if (!playAnimation) //If we should not play the animation, do nothing
            return;

        timePassed += Time.deltaTime;

        //Debug.Log("Time Passed " + timePassed);

        //Advance the current frame of animation depending on the speed

        if (currentAnimation.looping) //If the current animation loops, loop it
            currentAnimation.currentFrame = (int)(timePassed / currentAnimation.invSpeed) % currentAnimation.frameCount;
        else if ((int)(timePassed / currentAnimation.invSpeed) >= currentAnimation.frameCount) //If it does not loop, go to last frame and stop animating
        {
            playAnimation = false;
            currentAnimation.currentFrame = currentAnimation.frameCount - 1;
        }

        spriteRenderer.sprite = currentAnimation.frames[currentAnimation.currentFrame];
    }

    public Animation GetAnimation(string key) { return animationDictionary[key]; }

    ///Changes the animation to the animation with the given key. If the animation is not found,
    ///does not transition
    public void ChangeAnimation(string key) 
    {
        try  { 
            currentAnimation = animationDictionary[key];
            currentAnimation.currentFrame = 0;
            timePassed = 0.0;
            playAnimation = true;
            spriteRenderer.sprite = currentAnimation.frames[0];
        }
        catch { Debug.Log("Could not find animation: " + key);  }
    }

    #region Playing/pausing animation
    public void PauseAnimation() { playAnimation = false; }
    public void PlayAnimation() { playAnimation = true; }
    public void ToggleAnimationPlay() { playAnimation = !playAnimation; }
    #endregion

    #region Sprite Manipulation
    public void SetHorizontalFlip(bool facingRight) { spriteRenderer.flipX = facingRight; }
    public void SetVerticalFlip(bool rightsideUp) { spriteRenderer.flipX = !rightsideUp; }
    public void FlipHorizontal() { spriteRenderer.flipX = !spriteRenderer.flipX; }
    #endregion
}
