using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum Estado
    {
        Idle,
        Move,
        Attack
    }

    [Header("Jogador")]
    [SerializeField] private PlayerController jogador;

    [Header("Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float raioDeteccao = 4f;
    [SerializeField] private float raioPerseguicao = 7f;

    [Header("Combate")]
    [SerializeField] private int vida = 100;
    [SerializeField] private int danoAtaque = 10;
    [SerializeField] private float alcanceAtaque = 1f;
    [SerializeField] private float intervaloAtaque = 1f;

    [Header("Estado")]
    [SerializeField] private Estado estadoAtual = Estado.Idle;

    private const float DuracaoReacao = 0.2f;
    private const float DistanciaChegada = 0.02f;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private Animator animator;
    private SpriteRenderer sprite;
    private Color corOriginal;
    private Vector3 posicaoInicial;

    private bool retornando;
    private float proximoAtaque;
    private float fimDaReacao;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        corOriginal = sprite.color;
        posicaoInicial = transform.position;
        estadoAtual = Estado.Idle;
    }

    private void Start()
    {
        animator.Play(Idle, 0, 0f);
    }

    private void Update()
    {
        if (retornando)
        {
            Retornar();
            return;
        }

        if (!JogadorAtivo())
        {
            if (estadoAtual != Estado.Idle)
                Retornar();

            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            jogador.transform.position
        );

        if (estadoAtual == Estado.Idle && distancia > raioDeteccao)
            return;

        if (distancia > raioPerseguicao)
        {
            Retornar();
            return;
        }

        OlharPara(jogador.transform.position);

        if (distancia <= alcanceAtaque)
        {
            MudarEstado(Estado.Attack);
            Atacar();
        }
        else
        {
            MudarEstado(Estado.Move);
            Perseguir(distancia);
        }
    }

    private void LateUpdate()
    {
        sprite.color = Time.time < fimDaReacao
            ? Color.red
            : corOriginal;
    }

    private bool JogadorAtivo()
    {
        return jogador != null && jogador.gameObject.activeInHierarchy;
    }

    private void MudarEstado(Estado novoEstado)
    {
        if (estadoAtual == novoEstado)
            return;

        estadoAtual = novoEstado;

        int animacao = novoEstado == Estado.Move ? Move : Idle;
        animator.Play(animacao, 0, 0f);
    }

    private void OlharPara(Vector3 destino)
    {
        float direcao = destino.x - transform.position.x;

        if (Mathf.Abs(direcao) > 0.01f)
            sprite.flipX = direcao < 0f;
    }

    private void Perseguir(float distancia)
    {
        float passo = Mathf.Min(
            velocidade * Time.deltaTime,
            distancia - alcanceAtaque
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            jogador.transform.position,
            passo
        );
    }

    private void Atacar()
    {
        if (Time.time < proximoAtaque)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        animator.Play(Attack, 0, 0f);
        jogador.ReceberDano(danoAtaque);
    }

    private void Retornar()
    {
        retornando = true;

        MudarEstado(Estado.Move);
        OlharPara(posicaoInicial);

        transform.position = Vector3.MoveTowards(
            transform.position,
            posicaoInicial,
            velocidade * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, posicaoInicial) > DistanciaChegada)
            return;

        transform.position = posicaoInicial;
        retornando = false;

        MudarEstado(Estado.Idle);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioDeteccao);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioPerseguicao);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, alcanceAtaque);
    }
}