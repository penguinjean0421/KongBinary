using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guest_Spawn : MonoBehaviour
{
    [SerializeField] private GameObject guest;
    [SerializeField] private ParticleSystem particleSystem; // ��ƼŬ �ý��� ������Ʈ
    [SerializeField] private float appearDelayMin = 6f; // ���� ��� �ð� �ּ� (6��)
    [SerializeField] private float appearDelayMax = 10f; // ���� ��� �ð� �ִ� (10��)
    [SerializeField] private float disappearDelayMin = 8f; // ����� ��� �ð� �ּ� (8��)
    [SerializeField] private float disappearDelayMax = 12f; // ����� ��� �ð� �ִ� (12��)

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

        // ��ƼŬ �ý����� ���ε��� �ʵ��� ����
        var main = particleSystem.main;
        main.loop = false;

        // �ʱ� ����: ������Ʈ ��Ȱ��ȭ
        guest.gameObject.SetActive(false);

        Debug.Log("test0");
        // �ڷ�ƾ ����
        StartCoroutine(ParticleCycle());
    }

    private IEnumerator ParticleCycle()
    {
        Debug.Log("test1");
        while (true)
        {
            Debug.Log("test2");
            // 6~10�� ���
            float appearDelay = Random.Range(appearDelayMin, appearDelayMax);
            yield return new WaitForSeconds(appearDelay);

            // ������Ʈ Ȱ��ȭ �� ��ƼŬ ���
            guest.gameObject.SetActive(true);
            particleSystem.Play();

            // ��ƼŬ ��� �ð���ŭ ���
            float particleDuration = particleSystem.main.duration;
            yield return new WaitForSeconds(particleDuration);

            // 8~12�� ���
            float disappearDelay = Random.Range(disappearDelayMin, disappearDelayMax);
            yield return new WaitForSeconds(disappearDelay);

            // ��ƼŬ �ٽ� ��� �� ��Ȱ��ȭ
            particleSystem.Play();
            yield return new WaitForSeconds(particleDuration);
            guest.gameObject.SetActive(false);
        }
    }
}
