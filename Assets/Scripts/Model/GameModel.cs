using System;
using System.Collections.Generic;

namespace com.Gemfile.Merger
{
    interface IGameModel
    {
        Queue<ICardModel> DeckQueue { get; set; }
        int CountOfFields { get; }
        Dictionary<int, ICardModel> Fields { get; }
        PlayerModel Player { get; }
        List<ICardModel> CardsData { get; }
		List<PhaseOfGame> PhasesOfGame { get; }
        int CurrentIndexOfPhase { get; set; }
		int Rows { get; }
		int Cols { get; }
		void Init();
    }

	public class CardData 
	{
		internal string type;
		internal int value;
		internal string resourceName;
		internal string cardName;
	}

    [System.Serializable]
    class GameModel: IGameModel 
    {
        public Queue<ICardModel> DeckQueue {
            get { return deckQueue; }
            set { deckQueue = value; }
        }
        Queue<ICardModel> deckQueue;

        public int CountOfFields {
            get { return countOfFields; }
        }
        readonly int countOfFields;

		public int Cols {
			get { return cols; }
		}
        readonly int cols;
		public int Rows {
			get { return rows; }
		}
        readonly int rows;

        public Dictionary<int, ICardModel> Fields {
            get { return fields; }
        }
        Dictionary<int, ICardModel> fields;
        
        public PlayerModel Player {
            get { return player; }
            set { player = value; }
        }
        PlayerModel player;

        public List<ICardModel> CardsData {
            get { return cardsData; }
            set { cardsData = value; }
        }
        List<ICardModel> cardsData;

		public List<PhaseOfGame> PhasesOfGame { 
			get { return phasesOfGame; }
		}
        List<PhaseOfGame> phasesOfGame;
		public int CurrentIndexOfPhase { 
			get { return currentIndexOfPhase; } 
			set { currentIndexOfPhase = value; }
		}
        int currentIndexOfPhase;

        internal GameModel()
        {
			rows = 3;
			cols = 3;
			countOfFields = rows * cols;

			fields = new Dictionary<int, ICardModel>();
			
			phasesOfGame = new List<PhaseOfGame>{ PhaseOfGame.FILL, PhaseOfGame.PLAY };
			currentIndexOfPhase = phasesOfGame.IndexOf(PhaseOfGame.FILL);
        }

		public void Init()
		{
            player = MakePlayerModel();
            cardsData = MakeCardsData();
		}

        PlayerModel MakePlayerModel()
        {
            return new PlayerModel(new CardData { type="Player", value=13, resourceName="Worrior", cardName="Worrior" });
        }

        List<ICardModel> MakeCardsData()
		{
			var cardDataList = new List<CardData> {
				// Potion 9
				new CardData { type="Potion", value=2, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=3, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=4, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=5, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=6, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=7, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=8, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=9, resourceName="Potion", cardName="Potion" },
				new CardData { type="Potion", value=10, resourceName="Potion", cardName="Potion" },

				// Monster 18
				new CardData { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData { type="Monster", value=2, resourceName="Slime.B", cardName="Slime.B" },
				new CardData { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData { type="Monster", value=3, resourceName="Rat", cardName="Rat" },
				new CardData { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData { type="Monster", value=4, resourceName="Bat", cardName="Bat" },
				new CardData { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData { type="Monster", value=5, resourceName="Snake", cardName="Snake" },
				new CardData { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData { type="Monster", value=6, resourceName="GoblinHammer", cardName="Goblin\nHammer" },
				new CardData { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData { type="Monster", value=7, resourceName="Skeleton", cardName="Skeleton" },
				new CardData { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData { type="Monster", value=8, resourceName="OrcSpear", cardName="OrcSpear" },
				new CardData { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData { type="Monster", value=9, resourceName="OrcKnife", cardName="OrcKnife" },
				new CardData { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },
				new CardData { type="Monster", value=10, resourceName="Minotaur", cardName="Minotaur" },

				// Coin 9
				new CardData { type="Coin", value=2, resourceName="Coin1", cardName="Coin" },
				new CardData { type="Coin", value=3, resourceName="Coin1", cardName="Coin" },
				new CardData { type="Coin", value=4, resourceName="Coin2", cardName="Coin" },
				new CardData { type="Coin", value=5, resourceName="Coin2", cardName="Coin" },
				new CardData { type="Coin", value=6, resourceName="Coin3", cardName="Coin" },
				new CardData { type="Coin", value=7, resourceName="Coin3", cardName="Coin" },
				new CardData { type="Coin", value=8, resourceName="Coin4", cardName="Coin" },
				new CardData { type="Coin", value=9, resourceName="Coin4", cardName="Coin" },
				new CardData { type="Coin", value=10, resourceName="Coin5", cardName="Coin" },

				// Weapon 11
				new CardData { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData { type="Weapon", value=2, resourceName="Knife", cardName="Knife" },
				new CardData { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData { type="Weapon", value=3, resourceName="Club", cardName="Club" },
				new CardData { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData { type="Weapon", value=4, resourceName="Sword", cardName="Sword" },
				new CardData { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData { type="Weapon", value=5, resourceName="Axe", cardName="Axe" },
				new CardData { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData { type="Weapon", value=6, resourceName="Hammer", cardName="Hammer" },
				new CardData { type="Weapon", value=7, resourceName="IronMace", cardName="IronMace" },

				// Magic 5
				new CardData { type="Magic", value=2, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=3, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=4, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=5, resourceName="Magic", cardName="Magic" },
				new CardData { type="Magic", value=6, resourceName="Magic", cardName="Magic" },
			};

			var deckList = new List<ICardModel>();
			cardDataList.ForEach(cardData => {
				var cardModel = (CardModel)Activator.CreateInstance(
					Type.GetType("com.Gemfile.Merger." + cardData.type + "Model"),
					cardData
				);
				deckList.Add(cardModel);
			});

			return deckList;
		}
    }
}
