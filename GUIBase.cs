﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GUIBase : MonoBehaviour{
	private const int LayerUI = 5;
	public Camera UICamera;
	GameObject UICanvas;

	public enum textalign{
		Left, Middle, Right
	}

	public enum alignment{
		StretchAll, StretchRight, StretchCenter, StretchLeft, 
		TopStretch, MiddleStretch, BottomStretch, 
		TopLeft, TopCenter, TopRight, 
		MiddleLeft, MiddleCenter, MiddleRight, 
		BottomLeft, BottomCenter, BottomRight
	}

	// Event Handler
	public delegate void OnEventTrigger(GameObject g);
	public static event OnEventTrigger onEventTriggerDownListener, onEventTriggerUpListener;

	public delegate void OnButtonClickEvent(GameObject g);
	public static event OnButtonClickEvent onButtonClickListener;

	public delegate void OnRaycastHitEvent(Collider col);
	public static event OnRaycastHitEvent onRaycastHitListener;

	void Start(){
		//Camera, canvas and eventsystem is absolutly required for everything else to work
		CreateUICamera ();
		CreateCanvas ();
		CreateEventSystem ();
	}

	void Update(){
		if (Input.GetMouseButtonDown(0)){ // if left button pressed...
			Ray ray = UICamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)){
				RaycastHitListener (hit.collider);
			}
		}
	}

	//Button click listener
	void ButtonClickListener(GameObject go){
		if(onButtonClickListener!=null)
			onButtonClickListener (go);
	}

	void RaycastHitListener(Collider col){
		if(onRaycastHitListener!=null)
			onRaycastHitListener (col);
	}

	void EventTriggerDownListener(BaseEventData baseEvent){
		if(onEventTriggerDownListener!=null)
			onEventTriggerDownListener (baseEvent.selectedObject.gameObject);
	}

	void EventTriggerUpListener(BaseEventData baseEvent){
		if(onEventTriggerUpListener!=null)
			onEventTriggerUpListener (baseEvent.selectedObject.gameObject);
	}

	private void CreateUICamera() {
		GameObject camera = new GameObject("UICamera");
		UICamera = camera.AddComponent<Camera> ();
		camera.AddComponent<GUILayer> ();
		camera.AddComponent<FlareLayer> ();
		UICamera.clearFlags = CameraClearFlags.Depth;
		UICamera.cullingMask = 1 << 5; //Render UI only
	}

	private void CreateCanvas() {
		// create the canvas
		UICanvas = new GameObject("UICanvas");
		UICanvas.layer = LayerUI;

		Canvas myCanvas = UICanvas.AddComponent<Canvas>();
		myCanvas.renderMode = RenderMode.ScreenSpaceCamera;																//setting canvas render mode
		myCanvas.worldCamera = UICamera;
		myCanvas.pixelPerfect = true;

		CanvasScaler canvasScaler = UICanvas.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
		canvasScaler.referenceResolution = new Vector2(1280, 800);

		GraphicRaycaster canvasRayc = UICanvas.AddComponent<GraphicRaycaster>();										//This is very inportant for events to work
		//UICanvas.transform.localPosition = new Vector3 (0,0,0);
		//UICanvas.transform.localScale = new Vector3 (1,1,1);
	}

	private void CreateEventSystem() {																						//This is very inportant for events to work
		GameObject esObject = new GameObject("EventSystem");

		EventSystem esClass = esObject.AddComponent<EventSystem>();
		esClass.sendNavigationEvents = true;
		esClass.pixelDragThreshold = 5;

		StandaloneInputModule stdInput = esObject.AddComponent<StandaloneInputModule>();
		stdInput.horizontalAxis = "Horizontal";
		stdInput.verticalAxis = "Vertical";

		TouchInputModule touchInput = esObject.AddComponent<TouchInputModule>();
	}

	public GameObject CreatePanel(string name, Color? color = null){
		if (color == null) color = new Color(0,0,0);
		GameObject panel = new GameObject(name);
		panel.transform.SetParent(UICanvas.transform);	
		panel.transform.SetAsLastSibling();
		RectTransform grt = panel.AddComponent<RectTransform> ();
		grt.sizeDelta = new Vector2 (0,0);
		Anchor_Presets_StretchAll (grt);
		//
		Image image = panel.AddComponent<Image>();
		image.color = (Color32)color;
		return panel;
	}

	public GameObject CreateButton(string name, Transform parent, alignment align, Vector2 size, Vector3 position, Color? color = null){
		if (color == null) color = new Color(0,0,0);
		GameObject Button = new GameObject(name);
		RectTransform brt = Button.AddComponent<RectTransform> ();
		Button.AddComponent<CanvasRenderer>();
		brt.sizeDelta = size;
		Button.transform.SetParent(parent);	
		Button.transform.SetAsLastSibling();																				//push it to the bottom so it should render last
		Set_Anchor_Preset(align, brt);
		brt.anchoredPosition = position;
		Image img = Button.AddComponent<Image>();	
		img.type = Image.Type.Sliced;
		img.color = (Color32)color;
		Button btn = Button.AddComponent<Button>();
		btn.onClick.AddListener(()=>{ButtonClickListener(Button);});
		return Button;
	}

	//Compile time constants can only be for primitive types like enums, float, double, strings so passing null for other data type
	public GameObject CreateText(string name, Transform parent, Font font, alignment? align = null, Vector2? size = null, Vector3? position = null, textalign? talign = null, int? fontSize = null, Color? color = null){
		//Handle non given values 
		if (fontSize == null) fontSize = 22;
		if (size == null) size = new Vector2(150, 50);
		if (position == null) position = new Vector3(0,0,0);
		if (align == null) align = alignment.MiddleCenter;
		if (talign == null) talign = textalign.Middle;
		if (color == null) color = new Color(0,0,0);
		//Now safe to proceed
		GameObject Text = new GameObject (name);//add listener to the button
		RectTransform trt = Text.AddComponent<RectTransform> ();
		trt.sizeDelta = (Vector2)size;
		Text.transform.SetParent(parent);	
		Set_Anchor_Preset((alignment)align, trt);
		trt.anchoredPosition = (Vector3)position;																//Center allign to parent
		Text text = Text.AddComponent<Text>();
		if(talign==textalign.Left)text.alignment = TextAnchor.MiddleLeft;														//align text to left
		if(talign==textalign.Middle)text.alignment = TextAnchor.MiddleCenter;													//align text to middle
		if(talign==textalign.Right)text.alignment = TextAnchor.MiddleRight;														//align text to right
		text.text = name;
		text.font = font;																								//assign font
		text.fontSize = (int)fontSize;
		text.color = (Color32)color;
		return Text;
	}

	public GameObject CreateSprite(string name, Transform parent, alignment? align = null, Vector2? size = null, Vector3? position = null, Color? color = null){
		//Handle non given values 
		if (size == null) size = new Vector2(150, 50);
		if (position == null) position = new Vector3(0,0,0);
		if (align == null) align = alignment.MiddleCenter;
		if (color == null) color = new Color(1,1,1);
		GameObject img = new GameObject(name);
		RectTransform irt = img.AddComponent<RectTransform> ();
		irt.sizeDelta = (Vector2)size;
		img.transform.SetParent(parent);
		Set_Anchor_Preset ((alignment)align, irt);
		irt.anchoredPosition = (Vector3)position;
		Image image = img.AddComponent<Image>();
		image.type = Image.Type.Sliced;
		image.color = (Color32)color;
		return img;
	}

	public GameObject CreateImage(string name, Transform parent, Sprite texture, alignment? align = null, Vector2? size = null, Vector3? position = null){
		//Handle non given values 
		if (size == null) size = new Vector2(150, 50);
		if (position == null) position = new Vector3(0,0,0);
		if (align == null) align = alignment.MiddleCenter;
		GameObject img = new GameObject(name);
		RectTransform irt = img.AddComponent<RectTransform> ();
		irt.sizeDelta = (Vector2)size;
		img.transform.SetParent(parent);
		Set_Anchor_Preset ((alignment)align, irt);
		irt.anchoredPosition = (Vector3)position;
		Image image = img.AddComponent<Image>();
		image.sprite = texture;
		return img;
	}

	public void AddEventTrigger(GameObject go){
		go.gameObject.AddComponent<EventTrigger> ();									//Adding EventListener Component to acc button.

		EventTrigger.Entry entry1 = new EventTrigger.Entry ();
		entry1.eventID = EventTriggerType.PointerDown;
		entry1.callback.AddListener (new UnityEngine.Events.UnityAction<BaseEventData>(EventTriggerDownListener));
		go.GetComponent<EventTrigger> ().triggers.Add (entry1);

		EventTrigger.Entry entry2 = new EventTrigger.Entry ();
		entry2.eventID = EventTriggerType.PointerUp;
		entry2.callback.AddListener (new UnityEngine.Events.UnityAction<BaseEventData>(EventTriggerUpListener));
		go.GetComponent<EventTrigger>().triggers.Add(entry2);
	}

	public GameObject CreateScrollableContent(string name, GameObject panel, alignment? align = null, Vector2? size = null){
		if (size == null) size = new Vector2(150, 50);
		if (align == null) align = alignment.TopStretch;

		ScrollRect sr = panel.AddComponent<ScrollRect> ();

		GameObject content = new GameObject (name);
		RectTransform cr = content.AddComponent<RectTransform> ();
		cr.sizeDelta = (Vector2)size;
		content.transform.parent = panel.transform;
		Set_Anchor_Preset ((alignment)align, cr);
		VerticalLayoutGroup vl = content.AddComponent<VerticalLayoutGroup> ();
		ContentSizeFitter sf = content.AddComponent<ContentSizeFitter> ();

		vl.padding.top = 10;
		vl.padding.bottom = 10;
		vl.padding.left = 10;
		vl.padding.right = 10;
		vl.spacing = 30;

		sr.horizontal = false;
		sr.content = cr;
		sf.verticalFit = ContentSizeFitter.FitMode.MinSize;
		return content;
	}

	public GameObject CreateScrollBar(Transform parent, Vector3? position = null, Vector2? size = null, Color? baseColor = null, Color? handleColor = null){
		if (size == null) size = new Vector2(150, 20);
		if (position == null) position = new Vector3(0,0,0);
		if (baseColor == null) baseColor = new Color(0,0,0);
		if (handleColor == null) handleColor = new Color(1,1,1);
		GameObject scrollBar = new GameObject ("ScrollBar");
		RectTransform srt = scrollBar.AddComponent<RectTransform> ();
		srt.sizeDelta = (Vector2)size;
		scrollBar.transform.SetParent (parent);

		scrollBar.AddComponent<CanvasRenderer>();
		Image image = scrollBar.AddComponent<Image>();
		image.color = (Color32)baseColor;
		Scrollbar sb = scrollBar.AddComponent<Scrollbar>();

		GameObject slingArea = new GameObject ("SlingArea");
		RectTransform sart = slingArea.AddComponent<RectTransform> ();
		sart.sizeDelta = new Vector2(-5, -5);
		slingArea.transform.SetParent (scrollBar.transform);
		Anchor_Presets_StretchAll (sart);

		GameObject handle = new GameObject ("Handle");
		RectTransform hrt = handle.AddComponent<RectTransform> ();
		Image img = handle.AddComponent<Image>();
		img.color = (Color32)handleColor;
		hrt.sizeDelta = new Vector2(0, 0);
		handle.transform.SetParent (slingArea.transform);

		sb.targetGraphic = (Graphic)img;
		sb.handleRect = hrt;
		sb.direction = Scrollbar.Direction.BottomToTop;
		srt.anchoredPosition = (Vector3)position;
		return scrollBar;
	}

	public void  MakeItemScrollable(GameObject go, int? minHeight = null){
		if (minHeight == null) minHeight = 75;
		LayoutElement le = go.AddComponent<LayoutElement> ();
		le.minHeight = (float)minHeight;
	}

	public GameObject CreateQuad(Vector3? size = null, Vector3? position = null, Quaternion? rotation = null){
		if(size == null)	size = new Vector3(10,10,1);
		if(position == null)	position = new Vector3(0,0,11);
		if(rotation == null)	rotation = new Quaternion(0,0,0,0);
		GameObject interstitialQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		interstitialQuad.layer = LayerUI;
		interstitialQuad.transform.localScale = (Vector3)size;
		interstitialQuad.transform.localPosition = (Vector3)position;
		interstitialQuad.transform.localRotation = (Quaternion) rotation;
		interstitialQuad.transform.SetParent(UICamera.transform);
		return interstitialQuad;
	}

	public Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}

	//Anchor presets
	void Set_Anchor_Preset(alignment align, RectTransform rt){
		switch (align) {
		case alignment.StretchAll:
			Anchor_Presets_StretchAll(rt);
			break;
		case alignment.StretchRight:
			Anchor_Presets_StretchRight(rt);
			break;
		case alignment.StretchCenter:
			Anchor_Presets_StretchCenter(rt);
			break;
		case alignment.StretchLeft:
			Anchor_Presets_StretchLeft(rt);
			break;
		case alignment.TopStretch:
			Anchor_Presets_TopStretch(rt);
			break;
		case alignment.MiddleStretch:
			Anchor_Presets_MiddleStretch(rt);
			break;
		case alignment.BottomStretch:
			Anchor_Presets_BottomStretch(rt);
			break;
		case alignment.TopLeft:
			Anchor_Presets_TopLeft(rt);
			break;
		case alignment.TopCenter:
			Anchor_Presets_TopCenter(rt);
			break;
		case alignment.TopRight:
			Anchor_Presets_TopRight(rt);
			break;
		case alignment.MiddleLeft:
			Anchor_Presets_MiddleLeft(rt);
			break;
		case alignment.MiddleCenter:
			Anchor_Presets_MiddleCenter(rt);
			break;
		case alignment.MiddleRight:
			Anchor_Presets_MiddleRight(rt);
			break;
		case alignment.BottomLeft:
			Anchor_Presets_BottomLeft(rt);
			break;
		case alignment.BottomCenter:
			Anchor_Presets_BottomCenter(rt);
			break;
		case alignment.BottomRight:
			Anchor_Presets_BottomRight(rt);
			break;
		}
	}

	void Anchor_Presets_StretchAll(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0);
		rt.anchorMax = new Vector2 (1,1);

	}

	void Anchor_Presets_StretchRight(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (1,0);
		rt.anchorMax = new Vector2 (1,1);

	}

	void Anchor_Presets_StretchCenter(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0.5f,0);
		rt.anchorMax = new Vector2 (0.5f,1);

	}

	void Anchor_Presets_StretchLeft(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0);
		rt.anchorMax = new Vector2 (0,1);

	}

	void Anchor_Presets_TopStretch(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,1);
		rt.anchorMax = new Vector2 (1,1);

	}

	void Anchor_Presets_MiddleStretch(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0.5f);
		rt.anchorMax = new Vector2 (1,0.5f);

	}

	void Anchor_Presets_BottomStretch(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0);
		rt.anchorMax = new Vector2 (1,0);

	}

	void Anchor_Presets_TopLeft(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,1);
		rt.anchorMax = new Vector2 (0,1);
	}

	void Anchor_Presets_TopCenter(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0.5f,1);
		rt.anchorMax = new Vector2 (0.5f,1);
	}

	void Anchor_Presets_TopRight(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (1,1);
		rt.anchorMax = new Vector2 (1,1);
	}

	void Anchor_Presets_MiddleLeft(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0.5f);
		rt.anchorMax = new Vector2 (0,0.5f);
	}

	void Anchor_Presets_MiddleCenter(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0.5f,0.5f);
		rt.anchorMax = new Vector2 (0.5f,0.5f);
	}

	void Anchor_Presets_MiddleRight(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (1,0.5f);
		rt.anchorMax = new Vector2 (1,0.5f);
	}

	void Anchor_Presets_BottomLeft(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0,0);
		rt.anchorMax = new Vector2 (0,0);
	}

	void Anchor_Presets_BottomCenter(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (0.5f,0);
		rt.anchorMax = new Vector2 (0.5f,0);
	}

	void Anchor_Presets_BottomRight(RectTransform rt){
		rt.anchoredPosition = new Vector2 (0,0);
		rt.anchorMin = new Vector2 (1,0);
		rt.anchorMax = new Vector2 (1,0);
	}
}
