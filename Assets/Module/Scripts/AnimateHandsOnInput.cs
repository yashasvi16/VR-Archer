using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandsOnInput : MonoBehaviour
{
    public InputActionProperty pinch;
    public InputActionProperty grab;
    public Animator animator;
    void Update()
    {
        float triggerValue = pinch.action.ReadValue<float>();
        animator.SetFloat("Trigger", triggerValue);
        float gripValue = grab.action.ReadValue<float>();
        animator.SetFloat("Grip", gripValue);
    }
}
