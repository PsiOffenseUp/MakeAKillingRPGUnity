using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Animation
{
    static readonly string spritesRoot = "Sprites/";
	static readonly string normalPost = "_norm";
    [SerializeField] public int frameCount { get; private set; }
    public bool looping { get; private set; }
    public int currentFrame = 0; //Current frame in the given animation
    public Sprite[] frames;
    public Sprite[] normalFrames;
    //public Texture2D[] framesTex;
    public float speed { get; private set; } //Number of frames in game to pass before incrementing a frame of the animation
    public float invSpeed { get; private set; } //Number of frames in game to pass before incrementing a frame of the animation
	public bool hasNormal { get; private set; }

    public Animation(string resourcePath, bool loops = true, float speed = 1.0f, bool has_normal = true)
    {
        looping = loops;
        this.speed = speed;
        this.invSpeed = 1.0f / speed;
		this.hasNormal = has_normal;

        //Load the animation resource
        frames = Resources.LoadAll<Sprite>(spritesRoot + resourcePath);
        if(frames == null) //If the loading failed
        {
            Debug.Log("Failed to load animation from: " + resourcePath);
            frameCount = 0;
        }
        else //Load normal frames and convert to textures
		{
            frameCount = frames.Length;

			if(this.hasNormal)
			{
				normalFrames = Resources.LoadAll<Sprite>(spritesRoot + resourcePath + normalPost);
				if(normalFrames == null)
				{
					Debug.Log("Could not load normal frames for: " + resourcePath);
					this.hasNormal = false; //Turn off normal if it could not be loaded
				}
			}
		}
    }
}

[System.Serializable] class AnimationMetadata
{
    [SerializeField] public string name;
    [SerializeField] public string resourcePath;
    [SerializeField] public bool loops = true;
    [SerializeField] public float speed = 1.0f;
    [SerializeField] public bool hasNormal = false;
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

    //Programatic animation variables
    Vector3 originalScale;
    [SerializeField] float bounciness = 1;
    [SerializeField] Transform scaleAnchor = null;
    const float bouncinessMultiplier = 0.0062f;
    const float horizontalStretchEffect = 0.0575f;
    const float stretchAccelThreshold = 0.00000125f;
    const float bounceDecay = 0.97f; //Used to decay velocity each frame
    const float accelerationMultiplier = 0.0125f;
    float stretchVelocity = 0.0f, stretchAcceleration = 0.0f;
    Vector3 tempScale; //Scale based off of unit value
    float percentScale;
    bool animateStretch = false;

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

        originalScale = spriteRenderer.transform.localScale;
        percentScale = 1.0f;
        tempScale = Vector3.one;
        if (scaleAnchor == null) scaleAnchor = spriteRenderer.transform;
    }
    public void OnUpdate()
    {
        if (!playAnimation) //If we should not play the animation, do nothing
            return;

        timePassed += Time.deltaTime;

        if (currentAnimation.invSpeed == 0)
            Debug.Log("invSpeed is 0 for animation: " + n);

        //Advance the current frame of animation depending on the speed

        if (currentAnimation.looping) //If the current animation loops, loop it
            currentAnimation.currentFrame = (int)(timePassed / currentAnimation.invSpeed) % currentAnimation.frameCount;
        else if ((int)(timePassed / currentAnimation.invSpeed) >= currentAnimation.frameCount) //If it does not loop, go to last frame and stop animating
        {
            playAnimation = false;
            currentAnimation.currentFrame = currentAnimation.frameCount - 1;
        }

        spriteRenderer.sprite = currentAnimation.frames[currentAnimation.currentFrame];
        SetMesh();

        HandleSquashAndStretch(); //Handle animating squash and stretch
    }

    public Animation GetAnimation(string key) { return animationDictionary[key]; }

    public Sprite GetCurrentSprite() { return currentAnimation.frames[currentAnimation.currentFrame]; }

    ///Changes the animation to the animation with the given key. If the animation is not found,
    ///does not transition
    string n;
    public void ChangeAnimation(string key) 
    {
        n = key;
        try  { 
            currentAnimation = animationDictionary[key];
            currentAnimation.currentFrame = 0;
            timePassed = 0.0;
            playAnimation = true;
            spriteRenderer.sprite = currentAnimation.frames[0];
        }
        catch { Debug.Log("Could not find animation: " + key);  }
    }

    ///Changes the animation to the animation with the given key without starting the animation over. 
    ///If the animation is not found,
    ///does not transition
    public void ChangeAnimationNoReset(string key)
    {
        try
        {
            int previousFrame = currentAnimation.currentFrame; //Preserve the previous frame of animation
            currentAnimation = animationDictionary[key];
            currentAnimation.currentFrame = previousFrame % currentAnimation.frameCount;
            timePassed = 0.0;
            playAnimation = true;
            spriteRenderer.sprite = currentAnimation.frames[0];
        }
        catch { Debug.Log("Could not find animation: " + key); }
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
    public void SetMesh()
    {

        //meshRenderer.material.SetTexture("_MainTex", currentAnimation.framesTex[currentAnimation.currentFrame]);
    }

    #region Squash and Stretch

    //Squashes the sprite and makes it somewhat bouncy
    public void Squash(float amount = 1.0f)
    {
        stretchVelocity = -bouncinessMultiplier * bounciness * amount;
        animateStretch = true;
    }

    //Stretches the sprite and makes it somehwat bouncy
    public void Stretch(float amount = 1.0f)
    {
        stretchVelocity = bouncinessMultiplier * bounciness * amount;
        animateStretch = true;
    }

    void HandleSquashAndStretch()
    {
        //Check whether or not to animate stretch at all this frame
        if (!animateStretch)
            return;

        percentScale += stretchVelocity;
        tempScale.y = percentScale * originalScale.x;
        tempScale.x = originalScale.y * (1.0f + horizontalStretchEffect*(1.0f - percentScale));
        //tempScale.z = 1;
        scaleAnchor.localScale = tempScale;

        //Next, update the stretch velocity and decay
        stretchAcceleration = accelerationMultiplier * (1.0f - tempScale.y);

        //Find the scale for this frame
        //If stretch amount is too low, keep scale like original
        if (stretchAcceleration <= stretchAccelThreshold && stretchAcceleration >= -stretchAccelThreshold)
        {
            scaleAnchor.localScale = originalScale;
            stretchAcceleration = stretchVelocity = 0.0f;
            percentScale = 1.0f;
            animateStretch = false;
            return;
        }

        stretchVelocity += stretchAcceleration;
        stretchVelocity *= bounceDecay;
    }

    #endregion

    #endregion
}
