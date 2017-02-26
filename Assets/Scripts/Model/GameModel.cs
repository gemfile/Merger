using System.Collections.Generic;

namespace com.Gemfile.Merger
{
    public interface IGameModel: IBaseModel
    {
        List<PhaseOfGame> PhasesOfGame { get; }
        int CurrentIndexOfPhase { get; set; }
    }

    [System.Serializable]
    public class GameModel: BaseModel, IGameModel 
    {
        public List<PhaseOfGame> PhasesOfGame { 
			get { return phasesOfGame; }
		}
        List<PhaseOfGame> phasesOfGame;

		public int CurrentIndexOfPhase { 
			get { return currentIndexOfPhase; } 
			set { currentIndexOfPhase = value; }
		}
        int currentIndexOfPhase;

        public GameModel()
        {
            phasesOfGame = new List<PhaseOfGame>{ PhaseOfGame.FILL, PhaseOfGame.WAIT, PhaseOfGame.PLAY };
			currentIndexOfPhase = phasesOfGame.IndexOf(PhaseOfGame.FILL);
        }

		public override void Init()
		{
			base.Init();
		}
    }
}
