using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIElement
{
    public abstract IEnumerator EnterAnimation();
    public abstract IEnumerator IdleAnimation();
    public abstract IEnumerator CloseAnimation();
}

public interface IInteractableUI : IUIElement
{
    public abstract IEnumerator HighlightedAnimation();
    public abstract IEnumerator UnhighlightedAnimation();
    public abstract IEnumerator SelectAnimation();
}
