
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class DuelPlayer : NetworkBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private CharacterController controller;

    [Header("Aparência")]
    [SerializeField] private MeshRenderer bodyRenderer;

    [Header("Basquete")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private GameObject bolaNaMao;

    [Networked, OnChangedRender(nameof(ApplyColor))]
    public byte TeamColor { get; set; }

    private bool firePending;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            TeamColor = (byte)(
                Runner.IsSharedModeMasterClient ? 0 : 1
            );
        }

        ApplyColor();
    }

    private void ApplyColor()
    {
        if (bodyRenderer == null)
            return;

        bodyRenderer.material.color =
            TeamColor == 0 ? Color.cyan : Color.red;
    }

    private void Update()
    {
        if (!HasStateAuthority)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            firePending = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        DuelMatch match = DuelMatch.Instance;

        if (match == null || !match.CanPlay)
        {
            firePending = false;
            return;
        }

        Move();
        Aim();

        if (firePending)
        {
            firePending = false;
            Arremessar();
        }
    }

    public override void Render()
    {
        if (bolaNaMao == null)
            return;

        DuelMatch match = DuelMatch.Instance;
        BasketScore placar = BasketScore.Instance;

        bool mostrarBola =
            match != null &&
            match.CanPlay &&
            placar != null &&
            placar.PodeArremessar(Object.StateAuthority);

        if (bolaNaMao.activeSelf != mostrarBola)
            bolaNaMao.SetActive(mostrarBola);
    }

    private void Move()
    {
        if (Keyboard.current == null)
            return;

        float x =
            (Keyboard.current.dKey.isPressed ? 1 : 0) -
            (Keyboard.current.aKey.isPressed ? 1 : 0);

        float z =
            (Keyboard.current.wKey.isPressed ? 1 : 0) -
            (Keyboard.current.sKey.isPressed ? 1 : 0);

        Vector3 direction =
            new Vector3(x, 0, z).normalized;

        Vector3 velocity =
            direction * speed + Vector3.down * 2f;

        controller.Move(velocity * Runner.DeltaTime);
    }

    private void Aim()
    {
        if (Camera.main == null || Mouse.current == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        Plane plane = new Plane(
            Vector3.up,
            transform.position
        );

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 direction = point - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }
    }

    private void Arremessar()
    {
        if (muzzle == null)
            return;

        BasketScore placar = BasketScore.Instance;

        if (placar == null ||
            !placar.PodeArremessar(Runner.LocalPlayer))
            return;

        Vector3 direcao = transform.forward;
        direcao.y = 0;

        placar.RPC_Arremessar(
            muzzle.position,
            direcao.normalized
        );
    }
}
