using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BowStringController : MonoBehaviour
{
    [SerializeField]
    private BowString bowStringRenderer;

    private XRGrabInteractable interactable;

    [SerializeField]
    private Transform midPointGrabObject, midPointVisualObject, midPointParent;

    [SerializeField]
    private float bowStringStretchLimit = 0.3f;

    private Transform interactor;

    private float strength, previousStength;

    [SerializeField]
    private float stringSoundThreshold = 0.001f;

    [SerializeField]
    private AudioSource bowStringAudio;

    public UnityEvent OnBowPulled;
    public UnityEvent<float> OnBowReleased;


    private void Awake()
    {
        interactable = midPointGrabObject.GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        interactable.selectEntered.AddListener(PrepareBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void ResetBowString(SelectExitEventArgs arg0)
    {
        OnBowReleased?.Invoke(strength);
        strength = 0.0f;

        previousStength = 0.0f;
        bowStringAudio.pitch = 1f;
        bowStringAudio.Stop();

        interactor = null;
        midPointGrabObject.localPosition = Vector3.zero;
        midPointVisualObject.localPosition = Vector3.zero;
        bowStringRenderer.CreateString(null);

    }

    private void PrepareBowString(SelectEnterEventArgs arg0)
    {
        interactor = arg0.interactorObject.transform;
        OnBowPulled?.Invoke();
    }

    private void Update()
    {
        if (interactor != null)
        {
            //convert bow string mid point position to the local space of the MidPoint
            Vector3 midPointLocalSpace =
                midPointParent.InverseTransformPoint(midPointGrabObject.position); // localPosition

            //get the offset
            float midPointLocalZAbs = Mathf.Abs(midPointLocalSpace.x);

            previousStength = strength;

            HandleStringPushedBackToStart(midPointLocalSpace);

            HandleStringPulledBackTolimit(midPointLocalZAbs, midPointLocalSpace);

            HandlePullingString(midPointLocalZAbs, midPointLocalSpace);

            bowStringRenderer.CreateString(midPointVisualObject.position);
        }
    }

    private void HandlePullingString(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        //what happens when we are between point 0 and the string pull limit
        if (midPointLocalSpace.x < 0 && midPointLocalZAbs < bowStringStretchLimit)
        {
            if(bowStringAudio.isPlaying == false && strength <= 0.01f)
            {
                Debug.Log("playing string audio!");
                bowStringAudio.Play();
            }

            strength = Remap(midPointLocalZAbs, 0, bowStringStretchLimit, 0, 1);
            midPointVisualObject.localPosition = new Vector3(midPointLocalSpace.x, 0, 0);

            PlayStringPullingSound();
        }
    }

    private float Remap(float value, int fromMin, float fromMax, int toMin, int toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    private void HandleStringPulledBackTolimit(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        //We specify max pulling limit for the string. We don't allow the string to go any farther than "bowStringStretchLimit"
        if (midPointLocalSpace.x < 0 && midPointLocalZAbs >= bowStringStretchLimit)
        {
            Debug.Log("pausing string audio!");
            bowStringAudio.Pause();
            strength = 1f;
            //Vector3 direction = midPointParent.TransformDirection(new Vector3(0, 0, midPointLocalSpace.z));
            midPointVisualObject.localPosition = new Vector3(-bowStringStretchLimit, 0, 0);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.x >= 0)
        {
            Debug.Log("stopping string audio!");
            bowStringAudio.pitch = 1f;
            bowStringAudio.Stop();
            strength = 0f;
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }

    private void PlayStringPullingSound()
    {
        if(Mathf.Abs(strength - previousStength) < stringSoundThreshold)
        {
            if(strength < previousStength)
            {
                bowStringAudio.pitch = -1f;
            }
            else
            {
                bowStringAudio.pitch = 1f;
            }
            Debug.Log("unpausing string audio!");
            bowStringAudio.UnPause();
        }
        else
        {
            Debug.Log("pausing string audiox2!");
            bowStringAudio.Pause();
        }
    }
}