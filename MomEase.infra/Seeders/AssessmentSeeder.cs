using Microsoft.EntityFrameworkCore;
using MomEase.core.Entities;
using MomEase.infra.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Seeders
{
    public static class AssessmentSeeder
    {
        public static async Task SeedAsync(MomEaseDbContext context)
        {
            // لو في أي assessment موجود خلاص، مش هنعمل seed تاني
            if (await context.Assessments.AnyAsync()) return;

            // ══════════════════════════════════════════════
            // 1️⃣ EPDS - Edinburgh Postnatal Depression Scale
            // ══════════════════════════════════════════════
            var epds = new Assessment
            {
                Name = "EPDS - Edinburgh Postnatal Depression Scale",
                Description = "A 10-question screening tool specifically designed for postpartum depression in new mothers.",
                TotalQuestions = 10,
                MaxScore = 30,

                ScoreLevels = new List<ScoreLevel>
                {
                    new() { LevelName = "Minimal",  MinScore = 0,  MaxScore = 8,  Advice = "You're doing well emotionally. Take small moments for yourself and stay connected with loved ones." },
                    new() { LevelName = "Mild",      MinScore = 9,  MaxScore = 11, Advice = "Some signs of emotional difficulty. Consider talking to someone you trust or your healthcare provider." },
                    new() { LevelName = "Moderate",  MinScore = 12, MaxScore = 13, Advice = "Moderate signs detected. We encourage you to speak with your doctor as soon as possible." },
                    new() { LevelName = "Severe",    MinScore = 14, MaxScore = 30, Advice = "Please reach out to your healthcare provider immediately. You deserve support and care." }
                },

                Questions = new List<Question>
                {
                    new()
                    {
                        QuestionText  = "I have been able to laugh and see the funny side of things.",
                        QuestionOrder = 1,
                        IsReverse     = true,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "As much as I always could",    Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Not quite so much now",         Score = 1, OptionOrder = 2 },
                            new() { OptionText = "Definitely not so much now",    Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Not at all",                    Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have looked forward with enjoyment to things.",
                        QuestionOrder = 2,
                        IsReverse     = true,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "As much as I ever did",         Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Rather less than I used to",     Score = 1, OptionOrder = 2 },
                            new() { OptionText = "Definitely less than I used to", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Hardly at all",                  Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have blamed myself unnecessarily when things went wrong.",
                        QuestionOrder = 3,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, most of the time",          Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, some of the time",           Score = 2, OptionOrder = 2 },
                            new() { OptionText = "Not very often",                  Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, never",                       Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have been anxious or worried for no good reason.",
                        QuestionOrder = 4,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "No, not at all",                 Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Hardly ever",                    Score = 1, OptionOrder = 2 },
                            new() { OptionText = "Yes, sometimes",                 Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Yes, very often",                Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have felt scared or panicky for no very good reason.",
                        QuestionOrder = 5,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, quite a lot",               Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, sometimes",                 Score = 2, OptionOrder = 2 },
                            new() { OptionText = "No, not much",                   Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, not at all",                 Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Things have been getting on top of me.",
                        QuestionOrder = 6,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, most of the time I haven't been able to cope", Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, sometimes I haven't been coping as well",       Score = 2, OptionOrder = 2 },
                            new() { OptionText = "No, most of the time I have coped quite well",       Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, I have been coping as well as ever",             Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have been so unhappy that I have had difficulty sleeping.",
                        QuestionOrder = 7,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, most of the time",          Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, sometimes",                 Score = 2, OptionOrder = 2 },
                            new() { OptionText = "Not very often",                 Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, not at all",                 Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have felt sad or miserable.",
                        QuestionOrder = 8,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, most of the time",          Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, quite often",               Score = 2, OptionOrder = 2 },
                            new() { OptionText = "Not very often",                 Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, not at all",                 Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "I have been so unhappy that I have been crying.",
                        QuestionOrder = 9,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, most of the time",          Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Yes, quite often",               Score = 2, OptionOrder = 2 },
                            new() { OptionText = "Only occasionally",              Score = 1, OptionOrder = 3 },
                            new() { OptionText = "No, never",                      Score = 0, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "The thought of harming myself has occurred to me.",
                        QuestionOrder = 10,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Yes, quite often",               Score = 3, OptionOrder = 1 },
                            new() { OptionText = "Sometimes",                      Score = 2, OptionOrder = 2 },
                            new() { OptionText = "Hardly ever",                    Score = 1, OptionOrder = 3 },
                            new() { OptionText = "Never",                          Score = 0, OptionOrder = 4 }
                        }
                    }
                }
            };

            // ══════════════════════════════════════════════
            // 2️⃣ PHQ-9 - Patient Health Questionnaire
            // ══════════════════════════════════════════════
            var phq9 = new Assessment
            {
                Name = "PHQ-9 - Patient Health Questionnaire",
                Description = "A 9-question tool that measures the severity of depression symptoms over the past two weeks.",
                TotalQuestions = 9,
                MaxScore = 27,

                ScoreLevels = new List<ScoreLevel>
                {
                    new() { LevelName = "Minimal",  MinScore = 0,  MaxScore = 4,  Advice = "Minimal depression. Keep maintaining healthy habits and social connections." },
                    new() { LevelName = "Mild",      MinScore = 5,  MaxScore = 9,  Advice = "Mild depression. Consider lifestyle changes and talking to someone you trust." },
                    new() { LevelName = "Moderate",  MinScore = 10, MaxScore = 14, Advice = "Moderate depression. Speaking with a healthcare professional is recommended." },
                    new() { LevelName = "Severe",    MinScore = 15, MaxScore = 19, Advice = "Moderately severe depression. Please contact your doctor or a mental health professional." },
                    new() { LevelName = "Very Severe", MinScore = 20, MaxScore = 27, Advice = "Severe depression. Please seek immediate professional help. You are not alone." }
                },

                Questions = new List<Question>
                {
                    new()
                    {
                        QuestionText  = "Over the past two weeks, how often have you felt down, depressed, or hopeless?",
                        QuestionOrder = 1,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Little interest or pleasure in doing things?",
                        QuestionOrder = 2,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Trouble falling or staying asleep, or sleeping too much?",
                        QuestionOrder = 3,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Feeling tired or having little energy?",
                        QuestionOrder = 4,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Poor appetite or overeating?",
                        QuestionOrder = 5,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Feeling bad about yourself — or that you are a failure or have let yourself or your family down?",
                        QuestionOrder = 6,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Trouble concentrating on things, such as reading the newspaper or watching television?",
                        QuestionOrder = 7,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Moving or speaking so slowly that other people could have noticed? Or being so fidgety or restless?",
                        QuestionOrder = 8,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Thoughts that you would be better off dead, or of hurting yourself in some way?",
                        QuestionOrder = 9,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    }
                }
            };

            // ══════════════════════════════════════════════
            // 3️⃣ GAD-7 - Generalized Anxiety Disorder
            // ══════════════════════════════════════════════
            var gad7 = new Assessment
            {
                Name = "GAD-7 - Generalized Anxiety Disorder",
                Description = "A 7-question screening tool for generalized anxiety disorder symptoms over the past two weeks.",
                TotalQuestions = 7,
                MaxScore = 21,

                ScoreLevels = new List<ScoreLevel>
                {
                    new() { LevelName = "Minimal",  MinScore = 0,  MaxScore = 4,  Advice = "Minimal anxiety. Keep up your self-care routines and stay connected with support." },
                    new() { LevelName = "Mild",      MinScore = 5,  MaxScore = 9,  Advice = "Mild anxiety. Try relaxation techniques and consider speaking with someone you trust." },
                    new() { LevelName = "Moderate",  MinScore = 10, MaxScore = 14, Advice = "Moderate anxiety. Consider speaking with a healthcare professional for guidance." },
                    new() { LevelName = "Severe",    MinScore = 15, MaxScore = 21, Advice = "Severe anxiety. Please seek professional support as soon as possible. Help is available." }
                },

                Questions = new List<Question>
                {
                    new()
                    {
                        QuestionText  = "Feeling nervous, anxious, or on edge?",
                        QuestionOrder = 1,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Not being able to stop or control worrying?",
                        QuestionOrder = 2,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Worrying too much about different things?",
                        QuestionOrder = 3,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Trouble relaxing?",
                        QuestionOrder = 4,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Being so restless that it's hard to sit still?",
                        QuestionOrder = 5,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Becoming easily annoyed or irritable?",
                        QuestionOrder = 6,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    },
                    new()
                    {
                        QuestionText  = "Feeling afraid as if something awful might happen?",
                        QuestionOrder = 7,
                        IsReverse     = false,
                        AnswerOptions = new List<AnswerOption>
                        {
                            new() { OptionText = "Not at all",            Score = 0, OptionOrder = 1 },
                            new() { OptionText = "Several days",          Score = 1, OptionOrder = 2 },
                            new() { OptionText = "More than half the days", Score = 2, OptionOrder = 3 },
                            new() { OptionText = "Nearly every day",      Score = 3, OptionOrder = 4 }
                        }
                    }
                }
            };

            context.Assessments.AddRange(epds, phq9, gad7);
            await context.SaveChangesAsync();
        }
    }
}
