using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Climb : MonoBehaviour
{
    [Tooltip("Text box GameObject for displaying text.")]
    [SerializeField]
    private GameObject textBoxGameObject;

    [Tooltip("Movement threshold to detect player movement (meters).")]
    [SerializeField]
    private float moveThreshold = 0.1f;

    private GameObject textBox;
    private Vector3 lastPlayerPosition;
    private bool textActive;
    private bool climbTriggered = false;
    private bool movementLocked = false;
    public bool GameOver = false;

    private void Start()
    {
        if (textBoxGameObject != null)
        {
            textBox = textBoxGameObject;
            textBox.SetActive(false);
        }
    }

    private void Update()
    {
        if (GameOver)
            return;

        // Check if player moved while intro text is active -> immediate Game Over
        if (textActive)
        {
            if (Camera.main != null && Vector3.Distance(Camera.main.transform.position, lastPlayerPosition) > moveThreshold)
            {
                Debug.Log("Climb: Player moved during intro text! Game Over.");
                textActive = false;
                GameOver = true;
                movementLocked = true;

                if (textBox != null)
                {
                    textBox.SetActive(false);
                }

                return;
            }
        }
    }

    public void LockPlayerMovement(bool isLocked)
    {
        movementLocked = isLocked;
    }

    public void ActivateClimb(GameObject displayTextBox, string displayText, float delayBeforeClimb)
    {
        if (climbTriggered)
            return;

        climbTriggered = true;
        textBox = displayTextBox;
        lastPlayerPosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        
        if (textBox != null)
        {
            textBox.SetActive(true);
        }
        
        StartCoroutine(TextBoxRoutine(displayText, delayBeforeClimb));
    }

    private IEnumerator TextBoxRoutine(string text, float delayBeforeClimb)
    {
        textActive = true;
        if (textBox != null)
        {
            TextMeshProUGUI textComponent = textBox.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }   
        yield return new WaitForSeconds(3f);

        if (GameOver)
            yield break;

        textActive = false;
        if (textBox != null)
        {
            textBox.SetActive(false);
        }

        Debug.Log($"Climb: Waiting {delayBeforeClimb} seconds before movement enabled...");
        yield return new WaitForSeconds(delayBeforeClimb);

        if (GameOver)
            yield break;

        movementLocked = false;
        Debug.Log("Climb: Player can now move freely!");
    }
    private void MovePlayerUp(float amount)
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position += Vector3.up * amount;
        }
    }
}
