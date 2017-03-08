using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace com.Gemfile.Merger
{
	public interface ICardView: IBaseView
	{
		void SetValue(string key, string value);
		void SetSuit(string name);
		void SetVisibleOfChild(string key, bool visible);
		void SetCharacter(Transform character);
		void SetVisibleOfValues(bool visible);
		void SetVisibleOfResource(bool visible);
		Transform Character { get; }
		TextMesh GetText(string key);
		void CallAnimation(string name);
		void SetGettingEffect(string nameAffected, int valueAffected);
		void Highlight(bool visible, Color32 color);
		List<ICapturedCardView> GetCapturedCards();
	}

	public class CardView: BaseView, ICardView
	{
		[SerializeField]
		SpriteOutline spriteOutline;
		[SerializeField]
		TextMesh valueText;
		[SerializeField]
		TextMesh nameText;
		[SerializeField]
		Transform suits;
		[SerializeField]
		Transform deck;
		public Transform Character { get { return character; } }
		Transform character;
		Animator animator;
		
		public void SetCharacter(Transform character)
		{
			this.character = character;
			this.animator = character.GetComponent<Animator>();
		}
		
		public void SetValue(string key, string value)
		{
			transform.GetChild(0).Find(key).GetComponent<TextMesh>().text = value;
		}

		public void Highlight(bool visible, Color32 color)
		{
			spriteOutline.enabled = visible;
			spriteOutline.color = color;
		}

		public void SetVisibleOfChild(string key, bool visible)
		{
			deck.Find(key).gameObject.SetActive(visible);
		}

		public void SetVisibleOfValues(bool visible)
		{
			valueText.gameObject.SetActive(visible);
			nameText.gameObject.SetActive(visible);
		}

		public void SetVisibleOfResource(bool visible)
		{
			character.gameObject.SetActive(visible);
		}

		public void SetSuit(string name)
		{
			foreach (Transform suit in suits)
			{
				suit.gameObject.SetActive(false);
			}

			var targetSuit = "";
			switch (name)
			{
				case "Potion": targetSuit = "Heart"; break;
				case "Coin": targetSuit = "Diamond"; break;
				case "Weapon": targetSuit = "Club"; break;
				case "Magic": targetSuit = "Club"; break;
				case "Monster": targetSuit = "Spade"; break;
			}

			if (targetSuit != "")
			{
				suits.Find(targetSuit).gameObject.SetActive(true);
			}
		}
		
		public void CallAnimation(string name)
		{
			if (animator != null) 
			{
				animator.SetTrigger(name);
			}
		}

		public void SetGettingEffect(string nameAffected, int valueAffected)
		{
			TextMesh valueText = GetText(nameAffected);

			var delay = 0.8f;
			var sequence = DOTween.Sequence();
			sequence.SetDelay(delay);
			sequence.AppendCallback(() => {
				SetValue(nameAffected, (valueAffected >= 0 ? "+" : "") + valueAffected.ToString());
				SetVisibleOfText(nameAffected, true);
			});
			sequence.Append(
				DOTween.To(
					() => valueText.color,
					x => valueText.color = x,
					new Color(valueText.color.r, valueText.color.g, valueText.color.b, 1),
					.8f
				).From().SetEase(Ease.InCubic)
			);
			sequence.Insert(
				delay, 
				valueText.transform.DOLocalMoveY(valueText.transform.localPosition.y - 0.04f, 0.8f).From()
			);
			sequence.AppendCallback(() => {
				SetVisibleOfText(nameAffected, false);
			});
		}

		void SetVisibleOfText(string key, bool visible)
		{
			deck.Find(key).GetComponent<TextMesh>().gameObject.SetActive(visible);
		}

		public TextMesh GetText(string key)
		{
			return deck.Find(key).GetComponent<TextMesh>();
		}

		public List<ICapturedCardView> GetCapturedCards()
		{
			var childCount = transform.childCount;
			var capturedCards = new List<ICapturedCardView>();
			for(var i = 2; i < childCount; i++)
			{
				capturedCards.Add(transform.GetChild(i).GetComponent<ICapturedCardView>());
			}
			return capturedCards;
		}
	}
}
