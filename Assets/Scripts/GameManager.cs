using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Correct Number")]
    public int CorrectNumber; //The number displayed on NumberBalloonText. Randomised when void 'RollNewNumber' is called.
    public TMP_Text NumberBalloonText; //The textbox on the balloon that displays the correct number.
    public List<int> RemainingNumbers; //A list containing every number the player can still roll.
    public GameObject BalloonParent; //An object that shifts the balloon position every time the player 'ChoseRight'.
    public GameObject Camera; //Camera checks if player is looking at the balloon before allowing them to choose an answer.
    public GameObject PopManager; //Instantiates a firework particle effect at the balloon's location when it pops.
    
    [Header("Score")]
    public int Score; //Goes up every time the player chooses right.
    public TMP_Text ScoreText; //Displays Score.
    public GameObject FeedbackText; //Displays "Good Job!" when correct, "Try Again..." when wrong. [See: FeedbackText.cs]
    public bool[] ListOfErrors; //An array of Booleans, one for each question. When the player gets a question wrong, that question is marked as True.
    int HowManyWrong; //Checks how many booleans in ListOfErrors are true. 
    public TMP_Text FinalPercentageText; //Uses HowManyWrong to calcuate what percentage of questions they got wrong.

    [Header("Interactive UI")]
    public TMP_Text RightNumberText; //The button that displays the correct number.
    public TMP_Text WrongNumberText1; //The button that displays the wrong number.
    public TMP_Text WrongNumberText2; //Another button that displays the wrong number.
    public GameObject UIBottomBar; //Contains the button shuffling script.
    public GameObject WinScreen; //Is enabled when game is cleared. Displays score and replay button.
    [Header("Audio")]
    public GameObject AudioManager; //Contains all the sound effects.
    public GameObject LanuageManager; //Contains what language the player chooses at the start.
    public int LanuageValue;//The language that the player chose. 0 is English, 2 is French, 3 is Spanish, 4 is Italian. 
    public GameObject[] NumberVOManager; //Contains all the voice-overs which introduce the numbers.
    public GameObject[] FeedbackVOManager; //Contains all the voice-overs which occur when the player chooses right or wrong.
    public void StartTheGame()
    {
        RollNewNumber(); //Randomises CorrectNumber immediately.
        LanuageValue = LanuageManager.GetComponent<LanuageToggle>().LanuageValue; //Checks which language the player chose at the start.
        StartCoroutine(BalloonSpotted()); //Starts Round 1 with it's respective voice clip.
    } 

    void RollNewNumber() //Rolls (aka Randomises) the CorrectNumber every time the player 'ChoseRight'.
    {
        CorrectNumber = RemainingNumbers[Random.Range(0, RemainingNumbers.Count)]; //Roll a new number from the entries in the RemainingNumber list.
        RemainingNumbers.Remove(CorrectNumber); //Remove rolled number from play, preventing repeats.

        NumberBalloonText.text = CorrectNumber.ToString(); //Sets the balloon's textbox to the new CorrectNumber.

        RightNumberText.text = CorrectNumber.ToString(); //Sets the right button's textbox to the new CorrectNumber.
        WrongNumberText1.text = (CorrectNumber + 1).ToString(); //Sets the wrong button's textbox to a different number.
        WrongNumberText2.text = (CorrectNumber - 1).ToString(); //Sets the wrong button's textbox to a different number.

        UIBottomBar.GetComponent<ShuffleButtonPositions>().Shuffle(); //Shuffles the button's positions. [See: ShuffleButtonPositions.cs]
    }


    //The following two methods are triggered when you tap a button.
    public void ChoseRight() //Triggered when correct number is tapped.
    {  
        if(RemainingNumbers.Count == 0) //When the game is won, play victory sequence.
        {Victory(); return;}

        Score += 1; //Increases Score counter by 1.
        ScoreText.text = Score.ToString() + "/10"; //Displays score count in UI textbox. 

        AudioManager.GetComponent<AudioManager>().PlayGood(); //Play the victory sound. [See: SoundManager.cs]
        AudioManager.GetComponent<AudioManager>().PlayPop(); //Play the balloon pop sound. [See: SoundManager.cs]

        FeedbackText.GetComponent<FeedbackText>().GoodJob(); //Celebrates the player's success with a "Good Job!" [See: FeedbackText.cs]
        FeedbackVOManager[LanuageValue].GetComponent<FeedbackVOManager>().VOPraise(); //Audibly cheers on the player for their success.

        BalloonParent.GetComponent<ChangeBalloonColour>().ChangeBalloonHue(); //Changes the balloon to the next colour. [See: ChangeBalloonHue.cs]
        Camera.GetComponent<CheckIfFacingBalloon>().LookingForNextBalloon = true; //Alerts CheckIfFacingBalloon to go off when the player sees the next balloon.
        StartCoroutine(PopManager.GetComponent<PopManager>().Pop()); //Sets off fireworks at the balloon location. [See: PopManager.cs]

        RollNewNumber(); //Randomises CorrectNumber for the next round.

        int random = Random.Range(1,4);//Randomly picks between 1, 2 or 3.
        print(random);
        int rotationamount; //The balloon's position is chosen between three options.
        switch (random)
        {
            case 1: rotationamount = 90; break; //If we roll a one, the balloon goes left.
            case 2: rotationamount = 180; break; //If we roll a two, the balloon goes behind.
            case 3: rotationamount = 270; break; //If we roll a three, the balloon goes right.
            default: rotationamount = 0; print("balloon parent script broke somehow?! INCONCEIVABLE!!!"); break;
        }
        BalloonParent.transform.rotation = ////Changes balloon's position for the next round.
        Quaternion.Euler(BalloonParent.transform.eulerAngles.x, BalloonParent.transform.eulerAngles.y + rotationamount, BalloonParent.transform.eulerAngles.z);

        UIBottomBar.SetActive(false); //Disables the buttons so the player can't press them without seeing the balloon.
    }
    public void ChoseWrong() //Triggered when the wrong number is tapped.
    {
        FeedbackText.GetComponent<FeedbackText>().TryAgain(); //Aknowledges the player's failure with a "Try Again..." [See: FeedbackText.cs]
        AudioManager.GetComponent<AudioManager>().PlayBad(); //Play the failure sound. [See: SoundManager.cs]
        FeedbackVOManager[LanuageValue].GetComponent<FeedbackVOManager>().VORepremand(); //Audibly chastises the player for their failure.
        ListOfErrors[CorrectNumber - 1] = true; //Marks the current question as Incorrect for the winscreen's percentage.
    }

    public IEnumerator BalloonSpotted()
    {
        StartCoroutine(NumberVOManager[LanuageValue].GetComponent<NumberVOManager>().VOIntroduceNumber(CorrectNumber)); //Plays the "This is the number X. Say X." voice clip of your chosen language.
        if(LanuageValue == 2){yield return new WaitForSeconds(10);} //Pause to allow the player to say it outloud. Spanish VO is slower, so the pause is 10 seconds for spanish, 8 seconds for the others.
        else{yield return new WaitForSeconds(8);}
        UIBottomBar.SetActive(true); //Enables the buttons after the player has seen the balloon.
        
    }
    public void Victory()
    {
        NumberBalloonText.text = ";)"; //Changes the number on the balloon to a winking face if the player clears the game.
        ScoreText.text = "YAY"; //Changes the number on the balloon to a winking face if the player clears the game.
        FeedbackVOManager[LanuageValue].GetComponent<FeedbackVOManager>().VOPraise(); //One last voice line to celebrate the player;
        AudioManager.GetComponent<AudioManager>().PlayGameClear(); //Play the win sound. [See: SoundManager.cs]
        WinScreen.SetActive(true); //Enables win screen, displaying the player's score and allowing them to replay the level. 

        

        foreach (bool Fail in ListOfErrors) //Tallying the player's final grade percentage.
        {
            if(Fail){HowManyWrong += 1;} //Every question marked incorrect is counted.
        }
        int FinalPercentage = Mathf.Abs(10 * (HowManyWrong - 10)); //Percentage is calculated by taking away failures from the total number of questions times ten. We use Mathf.Abs so the number is positive.

        FinalPercentageText.text = "(" + FinalPercentage.ToString() + "%)"; //Display percentage as text on the winscreen.


    }
}
