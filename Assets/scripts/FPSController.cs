using Photon.Pun;
using UnityEngine;
using Photon.Realtime; // For Player type

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    [Header("Movement Settings")]
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Look Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    [Header("Drag and Drop")]
    public float pickUpDistance = 5f;
    public float throwForce = 10f;
    public Transform holdPosition;
    public LayerMask draggableLayer;

    [Header("State")]
    public bool canMove = true; // GameManager will set this to false at game end

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private bool isHolding = false;

    void Start()
    {
        if (photonView.IsMine)
        {
            characterController = GetComponent<CharacterController>();
            // Cursor initially locked by GameManager upon game start if Mover
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;

            if (holdPosition == null)
            {
                GameObject holdPosGO = new GameObject("HoldPosition");
                holdPosGO.transform.SetParent(playerCamera.transform);
                holdPosGO.transform.localPosition = new Vector3(0, 0, 1.5f);
                holdPosition = holdPosGO.transform;
            }

            if (GameManager.Instance != null && GameManager.Instance.GetComponent<PhotonView>() != null)
            {
                GameManager.Instance.GetComponent<PhotonView>().RPC("RPC_PlayerIsReadyInScene", RpcTarget.MasterClient);
                Debug.Log($"FPSController ({gameObject.name}): Sent RPC_PlayerIsReadyInScene to MasterClient.");
            }
            else
            {
                Debug.LogError($"FPSController ({gameObject.name}): GameManager instance or its PhotonView not found.");
            }
        }
        else
        {
            if (playerCamera) playerCamera.gameObject.SetActive(false);
            var audioListeners = GetComponentsInChildren<AudioListener>();
            foreach (var al in audioListeners) al.enabled = false;
            enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (!canMove) // If GameManager has disabled movement (e.g., game over)
        {
            if (isHolding && heldObject != null) // If holding something when movement is disabled
            {
                PhotonView pvHeld = heldObject.GetComponent<PhotonView>();
                if (pvHeld != null && pvHeld.IsMine) // Only if we own it
                {
                    LocalDrop(false); // Drop it without throwing
                }
                else // Not owner or invalid object, just clear local state
                {
                    isHolding = false;
                    heldObject = null;
                    heldObjectRb = null;
                }
            }
            // GameManager handles cursor state when canMove is false.
            return; // Stop further processing
        }

        // If canMove is true:
        HandleMovement();
        HandleMouseLook();
        HandleDragAndDrop();

        // Ensure cursor is locked if player can move (GameManager handles initial lock at game start)
        // This is a safeguard if something else unlocks it during play.
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Playing)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        if (Input.GetButton("Jump") && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    private void HandleDragAndDrop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isHolding) TryPickUpObject();
            else PlayerInitiatedDrop(false);
        }
        if (Input.GetMouseButtonDown(1) && isHolding) PlayerInitiatedDrop(true);

        if (isHolding && heldObject != null)
        {
            PhotonView pvHeld = heldObject.GetComponent<PhotonView>();
            if (pvHeld != null && pvHeld.IsMine)
            {
                heldObject.transform.position = holdPosition.position;
                heldObject.transform.rotation = holdPosition.rotation;
            }
            else if (pvHeld != null && !pvHeld.IsMine)
            {
                pvHeld.RequestOwnership();
            }
        }
    }

    private void TryPickUpObject()
    {
        if (isHolding) return;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickUpDistance, draggableLayer))
        {
            PhotonView pv = hit.collider.gameObject.GetComponent<PhotonView>();
            if (pv == null) { Debug.LogError($"{hit.collider.name} needs PhotonView!"); return; }

            heldObject = hit.collider.gameObject;
            heldObjectRb = heldObject.GetComponent<Rigidbody>();
            isHolding = true;

            if (!pv.IsMine) pv.RequestOwnership();
            else if (heldObjectRb != null) heldObjectRb.isKinematic = true;
        }
    }

    private void PlayerInitiatedDrop(bool shouldThrow)
    {
        if (!isHolding || heldObject == null) return;
        PhotonView pvHeld = heldObject.GetComponent<PhotonView>();
        GameObject objectToDrop = heldObject;
        isHolding = false; heldObject = null; heldObjectRb = null;
        Debug.Log($"FPSController: Player initiated drop for {objectToDrop?.name}. Local state cleared.");
        if (pvHeld != null && pvHeld.IsMine)
        {
            Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                if (shouldThrow)
                {
                    rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
                    rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_ServerForceDrop()
    {
        if (!photonView.IsMine) return;
        Debug.Log($"FPSController ({photonView.ViewID}): Received RPC_ServerForceDrop.");
        if (isHolding && heldObject != null) LocalDrop(false);
        else if (isHolding && heldObject == null)
        {
            isHolding = false;
            Debug.LogWarning($"FPSController ({photonView.ViewID}): RPC_ServerForceDrop - was holding but heldObject is null.");
        }
    }

    private void LocalDrop(bool shouldApplyThrowPhysicsIfOwner)
    {
        if (!isHolding || heldObject == null) return;
        PhotonView pvHeld = heldObject.GetComponent<PhotonView>();
        GameObject objectToDrop = heldObject;
        isHolding = false; heldObject = null; heldObjectRb = null;
        Debug.Log($"FPSController: LocalDrop for {objectToDrop?.name}. Local state cleared.");
        if (pvHeld != null && pvHeld.IsMine)
        {
            Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                if (shouldApplyThrowPhysicsIfOwner)
                {
                    rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
                    rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
                }
            }
        }
    }

    public bool IsHoldingObject(GameObject obj) { return isHolding && heldObject == obj; }
    public string GetHeldObjectNameForDebug() { return heldObject != null ? heldObject.name : "null"; }

    public override void OnEnable() { base.OnEnable(); PhotonNetwork.AddCallbackTarget(this); }
    public override void OnDisable() { base.OnDisable(); PhotonNetwork.RemoveCallbackTarget(this); }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (heldObject != null && targetView.gameObject == heldObject)
        {
            Debug.Log($"FPSController: Ownership request for held {targetView.gameObject.name} by {requestingPlayer.NickName}. Transferring.");
            targetView.TransferOwnership(requestingPlayer);
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (heldObject != null && targetView.gameObject == heldObject)
        {
            if (targetView.IsMine && isHolding)
            {
                Debug.Log($"FPSController: Ownership of {targetView.gameObject.name} confirmed/gained. Ensuring kinematic.");
                if (heldObjectRb == null) heldObjectRb = heldObject.GetComponent<Rigidbody>();
                if (heldObjectRb != null) heldObjectRb.isKinematic = true;
            }
            else if (!targetView.IsMine && isHolding)
            {
                Debug.Log($"FPSController: Lost ownership of held {targetView.gameObject.name}. Clearing local state.");
                isHolding = false; heldObject = null; heldObjectRb = null;
            }
        }
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.LogWarning($"FPSController: Ownership transfer FAILED for {targetView.ViewID} from {senderOfFailedRequest.NickName}");
        if (heldObject != null && targetView.gameObject == heldObject && isHolding && !targetView.IsMine)
        {
            Debug.LogWarning($"FPSController: Ownership transfer failed for held {heldObject.name}. Clearing local state.");
            isHolding = false; heldObject = null; heldObjectRb = null;
        }
    }
}