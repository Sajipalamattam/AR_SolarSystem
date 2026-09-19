using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class OrbitController : MonoBehaviour
{
    private PlanetOrbit[] planets;
    private bool isOrbiting = false;

    private Slider speedSlider;
    private Text speedLabel;

    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    private float rotationSensitivity = 0.3f;

    private float zoomSpeed = 0.01f;
    private float minScale = 0.5f;
    private float maxScale = 2.0f;

    // Information panel
    private GameObject infoPanel;
    private Text infoText;

    // Planet information
    private Dictionary<string, string> planetInfo = new Dictionary<string, string>()
    {
        {
            "Sun",
            "The Sun is the star at the center of our Solar System.\n\n" +
            "Temperature: ~5,500°C at surface\n" +
            "Type: G-type main-sequence star"
        },
        {
            "Mercury",
            "Mercury is the smallest planet and the closest planet to the Sun.\n\n" +
            "Distance from Sun: ~57.9 million km\n" +
            "Diameter: ~4,879 km"
        },
        {
            "Venus",
            "Venus is the second planet from the Sun and has a very thick atmosphere.\n\n" +
            "Distance from Sun: ~108.2 million km\n" +
            "Diameter: ~12,104 km"
        },
        {
            "Earth",
            "Earth is the third planet from the Sun and the only known planet with life.\n\n" +
            "Distance from Sun: ~149.6 million km\n" +
            "Diameter: ~12,742 km"
        },
        {
            "Mars",
            "Mars is known as the Red Planet because of iron minerals on its surface.\n\n" +
            "Distance from Sun: ~227.9 million km\n" +
            "Diameter: ~6,779 km"
        },
        {
            "Jupiter",
            "Jupiter is the largest planet in our Solar System.\n\n" +
            "Distance from Sun: ~778.5 million km\n" +
            "Diameter: ~139,820 km"
        },
        {
            "Saturn",
            "Saturn is a gas giant famous for its spectacular ring system.\n\n" +
            "Distance from Sun: ~1.43 billion km\n" +
            "Diameter: ~116,460 km"
        },
        {
            "Uranus",
            "Uranus is an ice giant that rotates on its side.\n\n" +
            "Distance from Sun: ~2.87 billion km\n" +
            "Diameter: ~50,724 km"
        },
        {
            "Neptune",
            "Neptune is the farthest recognized planet from the Sun.\n\n" +
            "Distance from Sun: ~4.50 billion km\n" +
            "Diameter: ~49,244 km"
        }
    };

    void Start()
    {
        planets = GetComponentsInChildren<PlanetOrbit>();

        foreach (PlanetOrbit planet in planets)
        {
            planet.enabled = false;

            // Add collider automatically for tapping
            if (planet.GetComponent<Collider>() == null)
            {
                SphereCollider collider = planet.gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.5f;
            }
        }

        CreateSpeedSlider();
        CreateInfoPanel();
    }

    void Update()
{
    HandlePlanetTap();
    HandleRotation();
    HandleZoom();
}

    // =========================================================
    // ORBIT CONTROL
    // =========================================================

    public void ToggleOrbit()
    {
        isOrbiting = !isOrbiting;

        foreach (PlanetOrbit planet in planets)
        {
            planet.enabled = isOrbiting;
        }
    }

    // =========================================================
    // SPEED SLIDER
    // =========================================================

    void CreateSpeedSlider()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        GameObject sliderObject = new GameObject("Orbit Speed Slider");
        sliderObject.transform.SetParent(canvas.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();

        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0f);

        sliderRect.anchoredPosition = new Vector2(0f, 65f);
        sliderRect.sizeDelta = new Vector2(280f, 35f);

        Image trackImage = sliderObject.AddComponent<Image>();
        trackImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        speedSlider = sliderObject.AddComponent<Slider>();

        speedSlider.minValue = 0.25f;
        speedSlider.maxValue = 3f;
        speedSlider.value = 1f;

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObject.transform, false);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(28f, 28f);

        speedSlider.handleRect = handleRect;
        speedSlider.targetGraphic = handleImage;

        // Speed text
        GameObject labelObject = new GameObject("Speed Label");
        labelObject.transform.SetParent(sliderObject.transform, false);

        RectTransform labelRect = labelObject.AddComponent<RectTransform>();

        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);

        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(260f, 35f);

        speedLabel = labelObject.AddComponent<Text>();

        speedLabel.text = "Orbit Speed: 1.0x";
        speedLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        speedLabel.fontSize = 20;
        speedLabel.alignment = TextAnchor.MiddleCenter;
        speedLabel.color = Color.white;

        labelObject.transform.SetAsLastSibling();

        speedSlider.onValueChanged.AddListener(ChangeSpeed);
    }

    void ChangeSpeed(float multiplier)
    {
        foreach (PlanetOrbit planet in planets)
        {
            planet.SetSpeed(multiplier);
        }

        if (speedLabel != null)
        {
            speedLabel.text = "Orbit Speed: " +
                              multiplier.ToString("0.0") + "x";
        }
    }

    // =========================================================
    // PLANET INFORMATION PANEL
    // =========================================================

    void CreateInfoPanel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        // Main panel
        infoPanel = new GameObject("Planet Info Panel");
        infoPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = infoPanel.AddComponent<Image>();

        // Dark transparent background
        panelImage.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        panelRect.anchoredPosition = new Vector2(0f, 80f);
        panelRect.sizeDelta = new Vector2(330f, 230f);

        // Information text
        GameObject textObject = new GameObject("Planet Info Text");
        textObject.transform.SetParent(infoPanel.transform, false);

        infoText = textObject.AddComponent<Text>();

        infoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        infoText.fontSize = 18;
        infoText.alignment = TextAnchor.UpperCenter;
        infoText.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();

        textRect.anchorMin = new Vector2(0.05f, 0.15f);
        textRect.anchorMax = new Vector2(0.95f, 0.90f);

        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Close button
        GameObject closeButton = new GameObject("Close Button");
        closeButton.transform.SetParent(infoPanel.transform, false);

        Image closeImage = closeButton.AddComponent<Image>();
        closeImage.color = Color.white;

        RectTransform closeRect = closeButton.GetComponent<RectTransform>();

        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);

        closeRect.anchoredPosition = new Vector2(0f, 15f);
        closeRect.sizeDelta = new Vector2(110f, 35f);

        // Close button text
        GameObject closeTextObject = new GameObject("Close Text");
        closeTextObject.transform.SetParent(closeButton.transform, false);

        Text closeText = closeTextObject.AddComponent<Text>();

        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.text = "CLOSE";
        closeText.fontSize = 18;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.black;

        RectTransform closeTextRect =
            closeTextObject.GetComponent<RectTransform>();

        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;

        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        // Button component
        Button button = closeButton.AddComponent<Button>();
        button.onClick.AddListener(CloseInfoPanel);

        // Hide initially
        infoPanel.SetActive(false);
    }

    void ShowPlanetInfo(string planetName)
    {
        if (infoPanel == null || infoText == null)
            return;

        if (planetInfo.ContainsKey(planetName))
        {
            infoText.text = planetName.ToUpper() +
                            "\n\n" +
                            planetInfo[planetName];

            infoPanel.SetActive(true);
        }
        else
        {
            Debug.Log("No information found for: " + planetName);
        }
    }

    void CloseInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    // =========================================================
    // PLANET TAP DETECTION
    // =========================================================

    void HandlePlanetTap()
{
    // ==========================================
    // MOUSE INPUT - FOR UNITY EDITOR TESTING
    // ==========================================

    if (Mouse.current != null &&
        Mouse.current.leftButton.wasPressedThisFrame)
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PlanetOrbit planet =
                hit.collider.GetComponentInParent<PlanetOrbit>();

            if (planet != null)
            {
                ShowPlanetInfo(planet.gameObject.name);
            }
        }
    }


    // ==========================================
    // TOUCH INPUT - FOR ANDROID PHONE
    // ==========================================

    if (Touchscreen.current != null &&
        Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
    {
        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PlanetOrbit planet =
                hit.collider.GetComponentInParent<PlanetOrbit>();

            if (planet != null)
            {
                ShowPlanetInfo(planet.gameObject.name);
            }
        }
    }
}

// =========================================================
// DRAG TO ROTATE SOLAR SYSTEM
// =========================================================

void HandleRotation()
{
    // ==========================================
    // MOUSE - UNITY EDITOR
    // ==========================================

    if (Mouse.current != null)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Don't start rotation when clicking UI
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            lastTouchPosition =
                Mouse.current.position.ReadValue();

            isDragging = true;
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 currentPosition =
                Mouse.current.position.ReadValue();

            Vector2 delta =
                currentPosition - lastTouchPosition;

            RotateSolarSystem(delta);

            lastTouchPosition = currentPosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }


    // ==========================================
    // TOUCH - ANDROID
    // ==========================================

    if (Touchscreen.current != null &&
        Touchscreen.current.touches.Count > 0)
    {
        var touch =
            Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            lastTouchPosition =
                touch.position.ReadValue();

            isDragging = true;
        }

        if (touch.press.isPressed && isDragging)
        {
            Vector2 currentPosition =
                touch.position.ReadValue();

            Vector2 delta =
                currentPosition - lastTouchPosition;

            RotateSolarSystem(delta);

            lastTouchPosition = currentPosition;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }
}


// =========================================================
// ROTATE SOLAR SYSTEM
// =========================================================

void RotateSolarSystem(Vector2 delta)
{
    float horizontalRotation =
        -delta.x * rotationSensitivity;

    float verticalRotation =
        delta.y * rotationSensitivity;

    transform.Rotate(
        Vector3.up,
        horizontalRotation,
        Space.World
    );

    transform.Rotate(
        Vector3.right,
        verticalRotation,
        Space.Self
    );
}

// =========================================================
// PINCH TO ZOOM
// =========================================================

void HandleZoom()
{
    // Only zoom when two fingers are touching
    if (Touchscreen.current == null)
        return;

    var touches = Touchscreen.current.touches;

    if (touches.Count < 2)
        return;

    var touch0 = touches[0];
    var touch1 = touches[1];

    if (!touch0.press.isPressed || !touch1.press.isPressed)
        return;

    // Current distance between fingers
    Vector2 currentPos0 = touch0.position.ReadValue();
    Vector2 currentPos1 = touch1.position.ReadValue();

    float currentDistance =
        Vector2.Distance(currentPos0, currentPos1);

    // Previous distance between fingers
    Vector2 previousPos0 =
        currentPos0 - touch0.delta.ReadValue();

    Vector2 previousPos1 =
        currentPos1 - touch1.delta.ReadValue();

    float previousDistance =
        Vector2.Distance(previousPos0, previousPos1);

    // Difference between current and previous distance
    float difference =
        currentDistance - previousDistance;

    // Convert difference into scale change
    float scaleChange =
        difference * zoomSpeed;

    Vector3 newScale =
        transform.localScale +
        Vector3.one * scaleChange;

    // Keep zoom within limits
    float newScaleValue =
        Mathf.Clamp(
            newScale.x,
            minScale,
            maxScale
        );

    transform.localScale =
        new Vector3(
            newScaleValue,
            newScaleValue,
            newScaleValue
        );
}
}