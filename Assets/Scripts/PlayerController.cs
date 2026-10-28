using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;

    [Header("Combate")]
    [SerializeField] private int vida = 100;
    [SerializeField] private int dano = 25;
    [SerializeField] private float alcanceAtaque = 1.5f;
    [SerializeField] private EnemyController inimigo;

    private const float DuracaoReacao = 0.2f;

    private SpriteRenderer sprite;
    private Color corOriginal;
    private float fimDaReacao;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        corOriginal = sprite.color;
    }

    private void Update()
    {
        if (vida <= 0)
            return;

        Mover();

        if (Input.GetKeyDown(KeyCode.Space))
            Atacar();
    }

    private void LateUpdate()
    {
        sprite.color = Time.time < fimDaReacao
            ? Color.red
            : corOriginal;
    }

    private void Mover()
    {
        Vector2 direcao = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        direcao = Vector2.ClampMagnitude(direcao, 1f);

        transform.position +=
            (Vector3)(direcao * velocidade * Time.deltaTime);
    }

    private void Atacar()
    {
        if (inimigo == null || !inimigo.isActiveAndEnabled)
            return;

        float distancia = Vector2.Distance(
            transform.position,
            inimigo.transform.position
        );

        if (distancia <= alcanceAtaque)
            inimigo.ReceberDano(dano);
    }

    public void ReceberDano(int quantidade)
    {
        if (vida <= 0 || quantidade <= 0)
            return;

        vida = Mathf.Max(0, vida - quantidade);
        fimDaReacao = Time.time + DuracaoReacao;

        if (vida == 0)
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, alcanceAtaque);
    }
}