using UnityEngine;

namespace Genesis01
{
    public class TestGemini : MonoBehaviour
    {
        [TextArea]
        public string prompt = "Hello, tell me a short story about a brave robot.";

        [ContextMenu("Send Test Request")]
        public void SendTestRequest()
        {
            if (GeminiAPIManager.Instance == null)
            {
                Debug.LogError("GeminiAPIManager instance not found! Please add the GeminiAPIManager script to the scene.");
                return;
            }

            Debug.Log("Sending request to Gemini...");
            GeminiAPIManager.Instance.GenerateContent(prompt, 
                (response) => 
                {
                    Debug.Log($"Gemini Response:\n{response}");
                },
                (error) =>
                {
                    Debug.LogError($"Request failed: {error}");
                }
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendTestRequest();
            }
        }
    }
}
