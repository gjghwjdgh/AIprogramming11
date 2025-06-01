using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource sfxAudioSource; // 효과음을 재생할 AudioSource 컴포넌트
    public AudioSource walkingAudioSource; // 걷는 소리를 재생할 AudioSource 컴포넌트
    void Awake()
    {
        // sfxAudioSource가 인스펙터에서 할당되지 않았다면, 현재 게임오브젝트에서 찾아봅니다.
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            // 그래도 없다면, 새로 하나 추가합니다.
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (walkingAudioSource == null)
        {
            // 만약 인스펙터에서 할당 안되어있고, AudioSource가 여러개라면 특정하기 어려움
            // 여기서는 인스펙터에서 walkingAudioSource를 직접 할당하는 것을 권장합니다.
            // 임시로 sfxAudioSource와 같은 것을 쓰지 않도록 주의 메시지 추가
            Debug.LogWarning("SoundManager: walkingAudioSource가 할당되지 않았습니다. 별도의 AudioSource를 할당해주세요.");
        }
        else
        {
            walkingAudioSource.playOnAwake = false; // 자동으로 재생되지 않도록 설정
            walkingAudioSource.loop = true;         // 걷는 소리는 반복 재생
        }
    }



    // 범용 효과음 재생 함수
    // 이 함수를 다른 스크립트에서 호출하여 사운드를 재생합니다.
    public void PlaySoundEffect(AudioClip clipToPlay, float volume = 1.0f)
    {
        if (sfxAudioSource != null && clipToPlay != null)
        {
            sfxAudioSource.PlayOneShot(clipToPlay, volume);
        }
        else
        {
            if (sfxAudioSource == null)
                Debug.LogWarning("SoundManager: AudioSource가 할당되지 않았습니다.");
            if (clipToPlay == null)
                Debug.LogWarning("SoundManager: 재생할 AudioClip이 null입니다.");
        }
    }
    
    public void PlayWalkingSound(AudioClip walkingClip)
    {
        if (walkingAudioSource != null && walkingClip != null)
        {
            // 현재 재생 중인 클립과 다르면 교체하고 재생
            if (walkingAudioSource.clip != walkingClip)
            {
                walkingAudioSource.clip = walkingClip;
            }
            // 아직 재생 중이 아니라면 재생 시작
            if (!walkingAudioSource.isPlaying)
            {
                walkingAudioSource.Play();
            }
        }
        else
        {
             if(walkingAudioSource == null) Debug.LogWarning("SoundManager: walkingAudioSource가 설정되지 않았습니다.");
             if(walkingClip == null) Debug.LogWarning("SoundManager: walkingClip이 null입니다.");
        }
    }

    // 걷는 소리 정지 함수 (새로 추가)
    public void StopWalkingSound()
    {
        if (walkingAudioSource != null && walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Stop();
        }
    }

}