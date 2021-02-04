using UnityEngine;

public class AnimationDelay : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        AudioController.Instance.audioEvent.AddListener(AudioEvent);
        bool isTag = animator.GetCurrentAnimatorStateInfo(0).IsTag("Strum");
        float time = AudioController.Instance.GetMusicSource().time;
        if ((time < 44 || time >= 113) && isTag)
            animator.SetTrigger("Stop");
        else if (time >= 44 && time < 113)
            animator.SetTrigger("Start");
    }

    private void AudioEvent(string arg)
    {
        arg = arg.Trim();
        if (arg == "StartAnimation" || arg == "StopAnimation")
            SendMessage(arg);
    }

    private void StartAnimation()
    {
        animator.SetTrigger("Start");
    }

    private void StopAnimation()
    {
        animator.SetTrigger("Stop");
    }

}