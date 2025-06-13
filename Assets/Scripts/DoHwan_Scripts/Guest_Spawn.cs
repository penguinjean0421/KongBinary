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
    [SerializeField] private ParticleSystem particleSystem; // ��ƼŬ �ý��� ������Ʈ
    [SerializeField] private float appearDelayMin = 6f; // ���� ��� �ð� �ּ� (6��)
    [SerializeField] private float appearDelayMax = 10f; // ���� ��� �ð� �ִ� (10��)
    [SerializeField] private float disappearDelayMin = 8f; // ����� ��� �ð� �ּ� (8��)
    [SerializeField] private float disappearDelayMax = 12f; // ����� ��� �ð� �ִ� (12��)

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    private void Start()
    {
        // ��ȿ�� �˻�
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

        // SkinnedMeshRenderer ��������
        //skinnedMeshRenderer = targetGuest.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component not found on target guest!", this);
            return;
        }

        // guestRender ����Ʈ ��ȿ�� �˻�
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

        // ��ƼŬ �ý����� ���ε��� �ʵ��� ����
        var main = particleSystem.main;
        main.loop = false;

        // �ʱ� ����: ������Ʈ ��Ȱ��ȭ
        targetGuest.SetActive(false);
        Debug.Log("Initialized Guest_Spawn");

        // �ڷ�ƾ ����
        StartCoroutine(ParticleCycle());
    }

    private IEnumerator ParticleCycle()
    {
        while (true)
        {
            // 6~10�� ���
            float appearDelay = Random.Range(appearDelayMin, appearDelayMax);
            yield return new WaitForSeconds(appearDelay);

            // ���� GuestRender ����
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

            // ������Ʈ Ȱ��ȭ �� ��ƼŬ ���
            targetGuest.SetActive(true);
            particleSystem.Play();
            Debug.Log($"Activated {targetGuest.name} and playing particle system");

            // ��ƼŬ ��� �ð���ŭ ���
            float particleDuration = particleSystem.main.duration;
            yield return new WaitForSeconds(particleDuration);

            // 8~12�� ���
            float disappearDelay = Random.Range(disappearDelayMin, disappearDelayMax);
            yield return new WaitForSeconds(disappearDelay);

            // ��ƼŬ �ٽ� ��� �� ��Ȱ��ȭ
            particleSystem.Play();
            //Debug.Log($"Playing particle system before deactivating {targetGuest.name}");
            yield return new WaitForSeconds(particleDuration);
            targetGuest.SetActive(false);
            //Debug.Log($"Deactivated {targetGuest.name}");
        }
    }
}