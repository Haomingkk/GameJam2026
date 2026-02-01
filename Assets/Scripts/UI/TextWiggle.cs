using UnityEngine;
using UnityEngine.EventSystems;

public class TextWiggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Adjust these in the Inspector to control the wiggle effect
    public float amplitude = 0.5f; // The maximum distance the object will move from its start position
    public float frequency = 1f;   // The speed of the wiggle
    public bool useLocalPosition = true; // Wiggle relative to parent or in world space

    private Vector3 startPosition;
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;
    public bool isPause = false;

    void Start()
    {
        // Store the object's initial position
        if (useLocalPosition)
        {
            startPosition = transform.localPosition;
        }
        else
        {
            startPosition = transform.position;
        }

        // Generate random start positions in the Perlin noise space to avoid synchronized movement
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        if (!isPause)
        {

            // Calculate noise values for each axis using Time.time to "scroll" through the noise
            float x = Mathf.PerlinNoise(noiseOffsetX + Time.time * frequency, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(noiseOffsetY + Time.time * frequency, 0f) * 2f - 1f;
            float z = Mathf.PerlinNoise(noiseOffsetZ + Time.time * frequency, 0f) * 2f - 1f;

            // The Mathf.PerlinNoise function returns a value between 0 and 1.
            // We transform it to the range -1 to 1 by multiplying by 2 and subtracting 1.

            // Apply the amplitude and combine with the starting position
            Vector3 wigglePos = new Vector3(x, y, z) * amplitude;
            Vector3 newPosition = startPosition + wigglePos;

            // Update the object's position
            if (useLocalPosition)
            {
                transform.localPosition = newPosition;
            }
            else
            {
                transform.position = newPosition;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyPosition(startPosition);
        isPause = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPause = false;
    }

    void ApplyPosition(Vector3 pos)
    {
        if (useLocalPosition) transform.localPosition = pos;
        else transform.position = pos;
    }
}
