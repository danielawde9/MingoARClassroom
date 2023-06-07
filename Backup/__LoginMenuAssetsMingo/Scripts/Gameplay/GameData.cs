using Unity;
using System.Collections.Generic;
using UnityEngine;

// GameData is a scriptable object that stores the list of questions and the users score
// scriptable object allow for east data managment and persistence without being tied to a specific game obejct 
[CreateAssetMenu(fileName ="GameData", menuName ="ScriptableObjects/GameData", order = 0)]
public class GameData : ScriptableObject
{
	public List<Question> questions;
	public int userScore;
}
