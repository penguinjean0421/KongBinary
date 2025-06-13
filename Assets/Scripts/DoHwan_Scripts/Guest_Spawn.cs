using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class GuestRender
{
    [SerializeField] public Mesh guestMesh;
    [SerializeField] public Material guestMaterial;
}

public class Guest_Spawn : MonoBehaviour
{
    [SerializeField] private List<GuestRender> guestRender = new List<GuestRender>();
    [SerializeField] private GameObject targetGuest;
    [SerializeField] private ParticleSystem particleSystem; // 파티클 시스템 컴포넌트
    [SerializeField] private float appearDelayMin = 6f; // 생성 대기 시간 최소 (6초)
    [SerializeField] private float appearDelayMax = 10f; // 생성 대기 시간 최대 (10초)
    [SerializeField] private float disappearDelayMin = 8f; // 사라짐 대기 시간 최소 (8초)
    [SerializeField] private float disappearDelayMax = 12f; // 사라짐 대기 시간 최대 (12초)

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    private void Start()
    {
        // 유효성 검사
        if (targetGuest == null)
        {
            Debug.LogError("Target guest object not assigned!", this);
            return;
        }

        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem component not found!", this);
                return;
            }
        }

        // SkinnedMeshRenderer 가져오기
        //skinnedMeshRenderer = targetGuest.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component not found on target guest!", this);
            return;
        }

        // guestRender 리스트 유효성 검사
        if (guestRender == null || guestRender.Count == 0)
        {
            Debug.LogError("GuestRender list is empty!", this);
            return;
        }

        foreach (var render in guestRender)
        {
            if (render.guestMesh == null || render.guestMaterial == null)
            {
                Debug.LogWarning("Some GuestRender entries have null mesh or material!");
            }
        }

        // 파티클 시스템이 루핑되지 않도록 설정
        var main = particleSystem.main;
        main.loop = false;

        // 초기 상태: 오브젝트 비활성화
        targetGuest.SetActive(false);
        Debug.Log("Initialized Guest_Spawn");

        // 코루틴 시작
        StartCoroutine(ParticleCycle());
    }

    private IEnumerator ParticleCycle()
    {
        while (true)
        {
            // 6~10초 대기
            float appearDelay = Random.Range(appearDelayMin, appearDelayMax);
            yield return new WaitForSeconds(appearDelay);

            // 랜덤 GuestRender 선택
            GuestRender randomRender = guestRender[Random.Range(0, guestRender.Count)];
            if (randomRender.guestMesh != null && randomRender.guestMaterial != null)
            {
                skinnedMeshRenderer.sharedMesh = randomRender.guestMesh;
                skinnedMeshRenderer.material = randomRender.guestMaterial;
                Debug.Log($"Applied mesh {randomRender.guestMesh.name} and material {randomRender.guestMaterial.name} to {targetGuest.name}");
            }
            else
            {
                Debug.LogWarning("Selected GuestRender has null mesh or material!");
            }

            // 오브젝트 활성화 및 파티클 재생
            targetGuest.SetActive(true);
            particleSystem.Play();
            Debug.Log($"Activated {targetGuest.name} and playing particle system");

            // 파티클 재생 시간만큼 대기
            float particleDuration = particleSystem.main.duration;
            yield return new WaitForSeconds(particleDuration);

            // 8~12초 대기
            float disappearDelay = Random.Range(disappearDelayMin, disappearDelayMax);
            yield return new WaitForSeconds(disappearDelay);

            // 파티클 다시 재생 후 비활성화
            particleSystem.Play();
            //Debug.Log($"Playing particle system before deactivating {targetGuest.name}");
            yield return new WaitForSeconds(particleDuration);
            targetGuest.SetActive(false);
            //Debug.Log($"Deactivated {targetGuest.name}");
        }
    }
}