// EffectManager.cs
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap�N���X�𗘗p���邽�߂ɒǉ�
using System.Collections; // �R���[�`�����g�����߂ɒǉ�
/// <summary>
/// �Q�[�����̃G�t�F�N�g�i�p�[�e�B�N���Ȃǁj�̍Đ��ƊǗ����s���N���X
/// </summary>
public class EffectManager : MonoBehaviour
{
    // �V���O���g���p�^�[���̎���
    public static EffectManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("�G�t�F�N�g�̍��W�ϊ��̊�ƂȂ�^�C���}�b�v�BLevelManager��BlockTilemap�Ȃǂ�ݒ肵�Ă��������B")]
    public Tilemap referenceTilemap; // ���[���h���W�ւ̕ϊ��ɗ��p����

    [Tooltip("�G�t�F�N�g��Ǐ]������Ώۂ�Transform�B�ʏ�̓v���C���[��ݒ肵�܂��B")]
    public Transform followTarget; // �G�t�F�N�g���Ǐ]����Ώ�

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// �A�C�e���f�[�^�ƃO���b�h���W����A�A�C�e���擾�G�t�F�N�g���Đ�����
    /// </summary>
    /// <param name="itemData">�擾���ꂽ�A�C�e���̃f�[�^</param>
    /// <param name="gridPosition">�A�C�e�������݂����O���b�h���W</param>
    public void PlayItemAcquisitionEffect(ItemData itemData, Vector3Int gridPosition)
    {
        // ItemData�܂��́A���̒��ɃG�t�F�N�g�v���n�u���ݒ肳��Ă��Ȃ���Ή������Ȃ�
        if (itemData == null || itemData.acquisitionEffectPrefab == null)
        {
            return;
        }

        // ���W�ϊ��̊�ƂȂ�^�C���}�b�v�����ݒ�̏ꍇ�͌x�����o��
        if (referenceTilemap == null)
        {
            Debug.LogWarning("EffectManager��referenceTilemap���ݒ肳��Ă��܂���B�G�t�F�N�g�͌��_�ɕ\������܂��B");
            // ��^�C���}�b�v���Ȃ��Ă��A�Ƃ肠�������_�ŃG�t�F�N�g���Đ�����
            PlayEffect(itemData.acquisitionEffectPrefab, Vector3.zero);
            return;
        }

        // �O���b�h���W���A���̃Z���̒����̃��[���h���W�ɕϊ�����
        Vector3 worldPosition = referenceTilemap.GetCellCenterWorld(gridPosition);

        // ������PlayEffect���\�b�h���Ăяo���āA�w��̍��W�ŃG�t�F�N�g���Đ�
        PlayEffect(itemData.acquisitionEffectPrefab, worldPosition);
    }

    /// <summary>
    /// �w�肳�ꂽ�v���n�u����G�t�F�N�g�𐶐����A�w��̈ʒu�ōĐ�����
    /// </summary>
    /// <param name="effectPrefab">�Đ�����G�t�F�N�g��GameObject�v���n�u</param>
    /// <param name="position">�G�t�F�N�g���Đ����郏�[���h���W</param>
    public void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayEffect was called with a null prefab.");
            return;
        }

        // ���������G�t�F�N�g�̃C���X�^���X��ێ�����
        GameObject effectInstance = Instantiate(effectPrefab, position, Quaternion.identity);

        // ���������G�t�F�N�g��ParticleSystem�����Ă��邩�`�F�b�N
        ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // ParticleSystem�̍Đ����I��������ɃI�u�W�F�N�g��j������
            // ps.main.duration�������ƁA�p�[�e�B�N���̐�������(startLifetime)���l������Ȃ����߁A
            // duration��startLifetime�̍ő�l����邱�ƂŁA�����悻�̏I�����Ԃ�S�ۂ��܂��B
            // ����ł������Ȃ��ꍇ�́A�p�[�e�B�N���v���n�u��StopAction��"Destroy"�ɐݒ肷��̂��ł��m���ł��B
            float lifeTime = Mathf.Max(ps.main.duration, ps.main.startLifetime.constantMax);
            Destroy(effectInstance, lifeTime);
        }
        else
        {
            // �p�[�e�B�N���V�X�e�����Ȃ��ꍇ�A5�b��ɏ�������i�ی��j
            Destroy(effectInstance, 5f);
            Debug.LogWarning($"The effect '{effectInstance.name}' does not have a ParticleSystem component. It will be destroyed in 5 seconds.");
        }
    }
    /// <summary>
    /// �w�肳�ꂽ�ΏۂɒǏ]����G�t�F�N�g����莞�ԍĐ����܂��B
    /// </summary>
    /// <param name="effectPrefab">�Đ�����G�t�F�N�g�̃v���n�u</param>
    /// <param name="duration">�Đ����ԁi�b�j</param>
    public void PlayFollowEffect(GameObject effectPrefab, float duration)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayFollowEffect was called with a null prefab.");
            return;
        }
        // �Ǐ]�Ώۂ��ݒ肳��Ă��Ȃ��ꍇ�͌x�����o���A�����𒆒f
        if (followTarget == null)
        {
            Debug.LogWarning("EffectManager��Follow Target���ݒ肳��Ă��܂���B�Ǐ]�G�t�F�N�g���Đ��ł��܂���B");
            return;
        }

        // �Ǐ]�Ǝ��Ԍo�ߌ�̔j�����s���R���[�`�����J�n
        StartCoroutine(FollowAndDestroyCoroutine(effectPrefab, duration));
    }

    /// <summary>
    /// �G�t�F�N�g��Ǐ]�����A�w�莞�Ԍ�ɔj������R���[�`��
    /// </summary>
    private IEnumerator FollowAndDestroyCoroutine(GameObject effectPrefab, float duration)
    {
        // �Ǐ]�Ώۂ̎q�I�u�W�F�N�g�Ƃ��ăG�t�F�N�g�𐶐��B����ɂ��A�Ώۂ̈ړ��Ɏ����ŒǏ]����B
        GameObject effectInstance = Instantiate(effectPrefab, followTarget.position, Quaternion.identity, followTarget);

        // �w�肳�ꂽ���Ԃ����ҋ@
        yield return new WaitForSeconds(duration);

        // �ҋ@��A�G�t�F�N�g�I�u�W�F�N�g���܂����݂��Ă���΁i���炩�̗��R�Ő�ɔj������Ă��Ȃ����m�F�j�j������
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}