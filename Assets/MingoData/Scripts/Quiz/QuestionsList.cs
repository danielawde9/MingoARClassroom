using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionsList
{
    public List<Question> questions;

    [System.Serializable]
    public class Question
    {
        public string type;
        public string question;
        public List<string> answers;
        public int correctAnswerIndex;
    }
}
