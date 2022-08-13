using UnityEngine;
using UnityEngine.Events;

public class DrumsProxyCollider : MonoBehaviour
{
    [SerializeField]
    private AudioClip _clip;
    [SerializeField]
    private AudioSource _source;
    [SerializeField]
    private float _soundDelay;
    [SerializeField]
    private UnityEvent OnClick;

    private void OnMouseUpAsButton()
    {
        if (_source && _clip)
        {
            Invoke(nameof(PlaySound), _soundDelay);
        }
        
        OnClick?.Invoke();
    }

    private void PlaySound()
    {
        _source.PlayOneShot(_clip);
    }
}
