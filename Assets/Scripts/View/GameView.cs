using System.Collections.Generic;
using UnityEngine;
using Scripts.Util;

public class GameView : MonoBehaviour {
	string[] deckNames;
	GameObject worrior;
	List<GameObject> fields;

	public void Prepare() {
		deckNames = new string[] { "BlueDeck", "WhiteDeck" };
//		worrior = ResourceCache.Instantiate("Worrior", transform);
		fields = new List<GameObject>();
	}

	public void MakeField(string type, int value, string resourceName, string cardName) {
		var field = new GameObject();
		field.transform.SetParent(transform);
		field.name = type;

		var deck = ResourceCache.Instantiate(deckNames[Random.Range(0, 2)], transform);
		deck.transform.SetParent(field.transform);
		deck.transform.Find("Name").GetComponent<TextMesh>().text = cardName;
		deck.transform.Find("Value").GetComponent<TextMesh>().text = value.ToString();

		var card = ResourceCache.Instantiate(resourceName, transform);
		card.transform.SetParent(field.transform);

		fields.Add(field);

		Align();
	}

	void Align() {
		int count = 0;
		Bounds fieldSize = fields[0].GetBounds();
		foreach (var field in fields) {
			field.transform.localPosition = new Vector2((fieldSize.size.x + 0.1f) * count, field.transform.localPosition.y);
			count++;
		}

		Bounds bounds = gameObject.GetBounds();
		Debug.Log($"hoi {bounds.size.x} and {fieldSize.extents.x}");
//		var screenX = Camera.main.ScreenToWorldPoint(new Vector3(1080, 0, 0)).x;
		transform.localPosition = new Vector2(
			fieldSize.extents.x - bounds.size.x/2,
			transform.localPosition.y
		);
	}
}
