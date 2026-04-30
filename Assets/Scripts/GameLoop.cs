using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLoop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject himGameObject;
    [SerializeField] private GameObject textBoxGameObject;
    [SerializeField] private GameObject qteTextGameObject;
    [SerializeField] private GameObject stopTextBoxGameObject;

    [Header("QTE Settings")]
    [SerializeField] private float qteChance = 0.25f;
    [SerializeField] private float qteWindow = 2f;
    [SerializeField] private int climbsBeforeQTE = 2;

    [Header("Movement Settings")]
    [SerializeField] private float normalMoveAmount = 5f;
    [SerializeField] private float doubleMoveAmount = 10f;

    [Header("STOP Settings")]
    [SerializeField] private float stopDelay = 10f;
    [SerializeField] private float stopCheckDuration = 2f;
    [SerializeField] private string stopDialogue = "STOP";
    [SerializeField] private int climbsBeforeStop = 2;

    [Header("Detection Settings")]
    [SerializeField] private float moveThreshold = 0.1f;

    // Private state
    private HIM himScript;
    private GameObject textBox;
    private GameObject qteText;
    private GameObject stopTextBox;
    private Vector3 lastPlayerPosition;
    public bool GameOver { get; private set; } = false;

    // Active phase tracking
    private GamePhase currentPhase = GamePhase.Idle;
    private float phaseTimer = 0f;

    // QTE state
    private float qteTimeRemaining = 0f;

    // Climb tracking
    private int climbCount = 0;
    private bool stopTriggered = false;

    // Coroutines
    private Coroutine stopCoroutine;    

    private enum GamePhase
    {
        Idle,
        IntroText,
        QTEWindow,
        QTEActive,
        Stop,
        GameOverPhase
    }

    private void Start()
    {
        himScript = himGameObject != null ? himGameObject.GetComponent<HIM>() : FindObjectOfType<HIM>();
        textBox = textBoxGameObject;
        qteText = qteTextGameObject;
        stopTextBox = stopTextBoxGameObject;

        if (textBox != null) textBox.SetActive(false);
        if (qteText != null) qteText.SetActive(false);
        if (stopTextBox != null) stopTextBox.SetActive(false);
    }

    private void Update()
    {
        if (GameOver)
        {
            Debug.LogWarning("Game Over");
            return;
        }

        phaseTimer += Time.deltaTime;

        switch (currentPhase)
        {
            case GamePhase.IntroText:
                HandleIntroTextPhase();
                break;
            case GamePhase.QTEWindow:
                HandleQTEWindowPhase();
                break;
            case GamePhase.QTEActive:
                HandleQTEActivePhase();
                break;
            case GamePhase.Stop:
                HandleStopPhase();
                break;
        }
    }

    private void HandleIntroTextPhase()
    {
        if (phaseTimer >= 3f)
        {
            ExitIntroTextPhase();
        }
    }

    private void HandleQTEWindowPhase()
    {
        if (HasPlayerMoved())
        {
            EndGame("Player moved during climb attempt");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AttemptClimb();
        }
    }

    private void HandleQTEActivePhase()
    {
        if (HasPlayerMoved())
        {
            EndGame("Player moved during QTE");
            return;
        }

        qteTimeRemaining -= Time.deltaTime;

        if (qteTimeRemaining <= 0f)
        {
            QTETimeout();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            QTESuccess();
        }
    }

    private void HandleStopPhase()
    {
        if (HasPlayerMoved())
        {
            EndGame("Player moved during STOP");
            return;
        }

        if (phaseTimer >= stopCheckDuration)
        {
            ExitStopPhase();
        }
    }

    private bool HasPlayerMoved()
    {
        if (Camera.main == null)
            return false;

        return Vector3.Distance(Camera.main.transform.position, lastPlayerPosition) > moveThreshold;
    }

    public void ActivateClimbSequence(GameObject displayTextBox, string displayText, float delayBeforeClimb)
    {
        textBox = displayTextBox;
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        EnterIntroTextPhase(displayText, delayBeforeClimb);
        StartSTOPTimer();
    }

    // ========== INTRO TEXT PHASE ==========
    private void EnterIntroTextPhase(string text, float delayBeforeQTE)
    {
        currentPhase = GamePhase.IntroText;
        phaseTimer = 0f;

        if (textBox != null)
        {
            textBox.SetActive(true);
            TextMeshProUGUI textComponent = textBox.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = text;
        }

        // Schedule QTE start - but only if we haven't hit the climb threshold yet
        if (climbCount < climbsBeforeQTE)
        {
            Invoke(nameof(EnterQTEWindowPhase), 3f + delayBeforeQTE);
        }
        else
        {
            // After intro, go to idle if we've already had enough climbs
            Invoke(nameof(ExitIntroTextPhase), 3f);
        }
    }

    private void ExitIntroTextPhase()
    {
        if (textBox != null)
            textBox.SetActive(false);

        // Update last position so player can move freely after intro
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        currentPhase = GamePhase.Idle;
    }

    // ========== QTE WINDOW PHASE ==========
    private void EnterQTEWindowPhase()
    {
        if (GameOver || climbCount >= climbsBeforeQTE)
            return;

        currentPhase = GamePhase.QTEWindow;
        phaseTimer = 0f;

        // Update last position when entering QTE window
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        if (qteText != null)
            qteText.SetActive(true);

        Debug.Log("GameLoop: QTE Window open! Press Space to climb!");
    }

    private void ExitQTEWindowPhase()
    {
        if (qteText != null)
            qteText.SetActive(false);

        currentPhase = GamePhase.Idle;
    }

    private void AttemptClimb()
    {
        if (Random.value < qteChance)
        {
            // QTE triggered
            StartQTE();
        }
        else
        {
            // Normal climb
            ExitQTEWindowPhase();
            MovePlayerUp(normalMoveAmount);
            IncrementClimbCount();
            Debug.Log("GameLoop: Normal climb!");
        }
    }

    // ========== QTE ACTIVE PHASE ==========
    private void StartQTE()
    {
        currentPhase = GamePhase.QTEActive;
        phaseTimer = 0f;
        qteTimeRemaining = qteWindow;

        if (qteText != null)
            qteText.SetActive(true);

        Debug.Log($"GameLoop: QTE Active! Press E in {qteWindow} seconds!");
    }

    private void QTESuccess()
    {
        if (qteText != null)
            qteText.SetActive(false);

        currentPhase = GamePhase.Idle;

        Debug.Log("GameLoop: QTE Success! Moving up double!");
        MovePlayerUp(doubleMoveAmount);
        IncrementClimbCount();
    }

    private void QTETimeout()
    {
        EndGame("QTE Failed - Time expired");
    }

    private void IncrementClimbCount()
    {
        climbCount++;
        Debug.Log($"GameLoop: Climb count: {climbCount}/{climbsBeforeStop}");

        if (climbCount >= climbsBeforeQTE)
        {
            Debug.Log($"GameLoop: Reached {climbsBeforeQTE} climbs - QTE disabled, awaiting STOP");
        }
    }

    // ========== STOP PHASE ==========
    private void StartSTOPTimer()
    {
        if (stopCoroutine != null)
            StopCoroutine(stopCoroutine);

        stopCoroutine = StartCoroutine(STOPTimerRoutine());
    }

    private IEnumerator STOPTimerRoutine()
    {
        Debug.Log($"GameLoop: STOP timer started - will trigger after {stopDelay:F2} seconds (requires {climbsBeforeStop} climbs first)");

        yield return new WaitForSeconds(stopDelay);

        if (GameOver || stopTriggered)
            yield break;

        if (climbCount < climbsBeforeStop)
        {
            Debug.Log($"GameLoop: STOP timer ready but waiting for {climbsBeforeStop - climbCount} more climb(s)");
            yield break;
        }

        EnterStopPhase();
    }

    private void EnterStopPhase()
    {
        stopTriggered = true;
        currentPhase = GamePhase.Stop;
        phaseTimer = 0f;
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        // Show STOP text in dedicated textbox
        if (stopTextBox != null)
        {
            stopTextBox.SetActive(true);
            TextMeshProUGUI stopTextComponent = stopTextBox.GetComponent<TextMeshProUGUI>();
            if (stopTextComponent != null)
                stopTextComponent.text = stopDialogue;
        }

        // Show HIM
        if (himScript != null)
            himScript.ShowHIM(stopDialogue);

        Debug.Log("GameLoop: STOP! Don't move!");
    }

    private void ExitStopPhase()
    {
        // Hide STOP textbox
        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        if (himScript != null)
            himScript.HideTextBox();

        // Update last position so player can move freely after STOP
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;

        currentPhase = GamePhase.Idle;
        Debug.Log("GameLoop: STOP phase ended. Player can move freely now!");
    }

    // ========== MOVEMENT & GAME OVER ==========
    private void MovePlayerUp(float amount)
    {
        if (Camera.main != null)
            Camera.main.transform.position += Vector3.up * amount;
    }

    public void LockPlayerMovement(bool isLocked)
    {
        // Implement in Player script if needed
    }

    private void EndGame(string reason)
    {
        if (GameOver)
            return;

        GameOver = true;
        currentPhase = GamePhase.GameOverPhase;

        CancelInvoke();
        if (stopCoroutine != null)
            StopCoroutine(stopCoroutine);

        if (qteText != null)
            qteText.SetActive(false);

        if (textBox != null)
            textBox.SetActive(false);

        if (stopTextBox != null)
            stopTextBox.SetActive(false);

        Debug.Log($"GameLoop: GAME OVER - {reason}");
    }
}