using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guest_Spawn : MonoBehaviour
{
    [SerializeField] private GameObject guest;
    [SerializeField] private ParticleSystem particleSystem; // 파티클 시스템 컴포넌트
    [SerializeField] private float appearDelayMin = 6f; // 생성 대기 시간 최소 (6초)
    [SerializeField] private float appearDelayMax = 10f; // 생성 대기 시간 최대 (10초)
    [SerializeField] private float disappearDelayMin = 8f; // 사라짐 대기 시간 최소 (8초)
    [SerializeField] private float disappearDelayMax = 12f; // 사라짐 대기 시간 최대 (12초)

    private void Start()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem component not found!", this);
                return;
            }
        }

        // 파티클 시스템이 루핑되지 않도록 설정
        var main = particleSystem.main;
        main.loop = false;

        // 초기 상태: 오브젝트 비활성화
        guest.gameObject.SetActive(false);

        Debug.Log("test0");
        // 코루틴 시작
        StartCoroutine(ParticleCycle());
    }

    private IEnumerator ParticleCycle()
    {
        Debug.Log("test1");
        while (true)
        {
            Debug.Log("test2");
            // 6~10초 대기
            float appearDelay = Random.Range(appearDelayMin, appearDelayMax);
            yield return new WaitForSeconds(appearDelay);

            // 오브젝트 활성화 및 파티클 재생
            guest.gameObject.SetActive(true);
            particleSystem.Play();

            // 파티클 재생 시간만큼 대기
            float particleDuration = particleSystem.main.duration;
            yield return new WaitForSeconds(particleDuration);

            // 8~12초 대기
            float disappearDelay = Random.Range(disappearDelayMin, disappearDelayMax);
            yield return new WaitForSeconds(disappearDelay);

            // 파티클 다시 재생 후 비활성화
            particleSystem.Play();
            yield return new WaitForSeconds(particleDuration);
            guest.gameObject.SetActive(false);
        }
    }
}
