using UnityEngine;

public class BombParticleController : MonoBehaviour // ������ ": MonoBehaviour" ��ǉ�
{
    private ParticleSystem explosionParticleSystem;

    void Awake()
    {
        // ������GetComponent�����삷�邽�߂ɂ́A���̃X�N���v�g��ParticleSystem�Ɠ���GameObject�ɃA�^�b�`����Ă��邩�A
        // ���邢��ParticleSystem���q�I�u�W�F�N�g�ɂ���AGetComponentInChildren�Ȃǂ��g���K�v������܂��B
        // ��ʓI�ɂ́A�p�[�e�B�N���V�X�e��������GameObject��ɂ��邱�Ƃ�z�肵�Ă��܂��B
        explosionParticleSystem = GetComponent<ParticleSystem>();

        // Null�`�F�b�N��ǉ����āA�p�[�e�B�N���V�X�e����������Ȃ������ꍇ�̑Ώ�����������Ƃ�茘�S�ɂȂ�܂��B
        if (explosionParticleSystem == null)
        {
            Debug.LogError("BombParticleController: ParticleSystem not found on this GameObject.", this);
        }
    }

    /// <summary>
    /// �p�[�e�B�N���V�X�e�����Đ����A��莞�Ԍ�Ɏ��g��j������
    /// </summary>
    public void PlayAndDestroy()
    {
        if (explosionParticleSystem != null)
        {
            explosionParticleSystem.Play();
            // �p�[�e�B�N���V�X�e���̍Đ����I�������GameObject��j������
            // explosionParticleSystem.main.duration �̓p�[�e�B�N���V�X�e���̌p�����ԁi�b�j�ł��B
            // ���[�v���Ă���p�[�e�B�N���V�X�e���̏ꍇ�A����̓��[�v1��̎��ԂɂȂ�܂��B
            // stopAction��Destroy�̏ꍇ�͎����I�ɔj������܂����A�����I��Destroy���Ăяo�����ƂŐ���ł��܂��B
            Destroy(gameObject, explosionParticleSystem.main.duration);
        }
        else
        {
            Debug.LogWarning("BombParticleController: Cannot play and destroy because ParticleSystem is null.");
            // �p�[�e�B�N���V�X�e�����Ȃ��ꍇ�ł�GameObject��j���������ꍇ�́A
            // Destroy(gameObject); �ȂǂƋL�q���邱�Ƃ������ł��܂��B
        }
    }
}