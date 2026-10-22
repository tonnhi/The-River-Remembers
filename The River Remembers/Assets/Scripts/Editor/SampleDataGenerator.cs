#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ConNuoc.Data;

namespace ConNuoc.EditorTools
{
    public static class SampleDataGenerator
    {
        private const string FolderPath = "Assets/ScriptableObjects/XomSen";

        [MenuItem("Con Nuoc/Generate Xom Sen Decisions")]
        public static void Generate()
        {
            EnsureFolder();

            CreateBlockedDrain();
            CreateCanalReeds();
            CreateMarketWaste();
            CreateCleanUp();
            CreateCommuneRequest();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("The River Remembers: Xom Sen decisions generated in " + FolderPath);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(FolderPath))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "XomSen");
        }

        private static DecisionSO CreateAsset(string fileName)
        {
            string path = FolderPath + "/" + fileName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<DecisionSO>(path);
            if (existing != null)
            {
                return existing;
            }

            var so = ScriptableObject.CreateInstance<DecisionSO>();
            AssetDatabase.CreateAsset(so, path);
            return so;
        }

        private static MeterDelta Delta(MeterType meter, int amount)
        {
            return new MeterDelta { meter = meter, amount = amount };
        }

        private static void CreateBlockedDrain()
        {
            var d = CreateAsset("Decision_BlockedDrain");
            d.decisionId = "blocked_drain";
            d.npcName = "The drain by the school";
            d.requiredTier = AgencyTier.Personal;
            d.oneTimeOnly = true;
            d.promptText = "Floodwater is pooling on the road to school. Rubbish has jammed the drain, but you suspect the blockage isn't the whole story.";

            var clearIt = new DecisionOption
            {
                optionText = "Clear the rubbish by hand.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Ecology, 5),
                    Delta(MeterType.Resources, -5),
                },
                setsFlag = "cleared_drain",
                minigameSceneName = "RubbishMinigame"
            };

            var lookCloser = new DecisionOption
            {
                optionText = "Look closer before touching anything.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Trust, 5),
                    Delta(MeterType.Resources, -5),
                },
                setsFlag = "inspected_drain",
                minigameSceneName = "RubbishMinigame",
                minigameTimeBonusSeconds = 5f
            };

            d.options = new[] { clearIt, lookCloser };
            EditorUtility.SetDirty(d);
        }

        private static void CreateCanalReeds()
        {
            var d = CreateAsset("Decision_CanalReeds");
            d.decisionId = "canal_reeds";
            d.npcName = "Chú Tư (farmer)";
            d.requiredTier = AgencyTier.Personal;
            d.oneTimeOnly = true;
            d.promptText = "\"I'm clearing the reeds along the canal bank for one more row of rice, con. Land is tight this year, and reeds don't feed anyone.\"";

            var keepReeds = new DecisionOption
            {
                optionText = "Ask him to leave a strip of reeds by the drain.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Ecology, 10),
                    Delta(MeterType.Resources, -5),
                    Delta(MeterType.Trust, 5),
                },
                unlocksTier = true,
                tierUnlocked = AgencyTier.Community,
                setsFlag = "reeds_kept",
                hasDelayedConsequence = true,
                delayedConsequence = new DelayedConsequence
                {
                    seasonsFromNow = 2,
                    setsFlag = "rain_mild",
                    explanationText = "Bà Sáu: \"Heavy rain came overnight. The reeds held the bank and slowed the water, so the road to school stayed open.\"",
                    deltas = new[]
                    {
                        Delta(MeterType.Ecology, 5),
                        Delta(MeterType.Trust, 5),
                    }
                }
            };

            var clearReeds = new DecisionOption
            {
                optionText = "Let him clear them. The rice matters more right now.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Resources, 10),
                    Delta(MeterType.Ecology, -10),
                },
                setsFlag = "reeds_cleared",
                hasDelayedConsequence = true,
                delayedConsequence = new DelayedConsequence
                {
                    seasonsFromNow = 2,
                    setsFlag = "rain_severe",
                    explanationText = "Bà Sáu: \"Heavy rain came overnight. With the reeds gone, nothing slowed the water. It ran over the bank and onto the road to school.\"",
                    deltas = new[]
                    {
                        Delta(MeterType.Ecology, -10),
                        Delta(MeterType.Trust, -10),
                    }
                }
            };

            d.promptVariants = new[]
            {
                new PromptVariant
                {
                    requiredFlag = "bins_set",
                    promptText = "\"Cô Hoa's bins are up already? The market is cleaner, I'll give you that. But I still need that extra row of rice, con. The reeds don't feed anyone.\""
                },
                new PromptVariant
                {
                    requiredFlag = "kept_clearing_alone",
                    promptText = "\"I hear you've been clearing that drain on your own, con. You can't do everything yourself. And I'm still clearing these reeds for one more row of rice.\""
                },
            };

            d.options = new[] { keepReeds, clearReeds };
            EditorUtility.SetDirty(d);
        }

        private static void CreateMarketWaste()
        {
            var d = CreateAsset("Decision_MarketWaste");
            d.decisionId = "market_waste";
            d.npcName = "Cô Hoa (market)";
            d.requiredTier = AgencyTier.Personal;
            d.oneTimeOnly = true;
            d.promptText = "\"People tip their rubbish by the drain because the market has nowhere else to put it. When the rain comes it washes straight into the water, and my customers can't reach the stalls when the road floods.\"";

            var setUpBins = new DecisionOption
            {
                optionText = "Help her set up bins and a collection spot at the market.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Trust, 10),
                    Delta(MeterType.Ecology, 5),
                    Delta(MeterType.Resources, -10),
                },
                unlocksTier = true,
                tierUnlocked = AgencyTier.Community,
                setsFlag = "bins_set",
                minigameSceneName = "BinPlacementMinigame"
            };

            var keepClearing = new DecisionOption
            {
                optionText = "Keep clearing the drain yourself when it blocks.",
                requiredTier = AgencyTier.Personal,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Ecology, 3),
                    Delta(MeterType.Resources, -5),
                    Delta(MeterType.Trust, -5),
                },
                setsFlag = "kept_clearing_alone",
                minigameSceneName = "BinPlacementMinigame",
                minigameTimeBonusSeconds = 5f
            };

            d.promptVariants = new[]
            {
                new PromptVariant
                {
                    requiredFlag = "reeds_kept",
                    promptText = "\"Chú Tư told me you asked him to leave the reeds. Thank you, con. People are starting to notice the small things. Now, about the rubbish by the drain: the market has nowhere to put it.\""
                },
                new PromptVariant
                {
                    requiredFlag = "reeds_cleared",
                    promptText = "\"I hear the reeds are going from the bank. When the rain comes, the water will have a clear run to my stalls. Can you help with the rubbish at least? The market has nowhere to put it.\""
                },
            };

            d.options = new[] { setUpBins, keepClearing };
            EditorUtility.SetDirty(d);
        }

        private static void CreateCleanUp()
        {
            var d = CreateAsset("Decision_CleanUp");
            d.decisionId = "clean_up";
            d.npcName = "Bà Sáu";
            d.requiredTier = AgencyTier.Community;
            d.oneTimeOnly = true;
            d.promptText = "\"You've listened, con. If the neighbours pitch in, we could clear the drain together and replant the bank. It's more than one girl can do, but it isn't more than a village can do.\"";

            var organise = new DecisionOption
            {
                optionText = "Organise the neighbourhood clean-up and replanting.",
                requiredTier = AgencyTier.Community,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Trust, 15),
                    Delta(MeterType.Ecology, 10),
                    Delta(MeterType.Resources, -10),
                },
                unlocksTier = true,
                tierUnlocked = AgencyTier.Institutional,
                setsFlag = "cleanup_done"
            };

            var notYet = new DecisionOption
            {
                optionText = "Not yet. I need to think about it more.",
                requiredTier = AgencyTier.Community,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Resources, 5),
                    Delta(MeterType.Trust, -5),
                }
            };

            d.promptVariants = new[]
            {
                new PromptVariant
                {
                    requiredFlag = "bins_set",
                    promptText = "\"The market bins are working, and people are talking about it. You've been listening, con. If the neighbours pitch in, we could clear the drain together and replant the bank.\""
                },
                new PromptVariant
                {
                    requiredFlag = "reeds_kept",
                    promptText = "\"You asked Chú Tư to leave the reeds, and he did. People noticed. If the neighbours pitch in, we could clear the drain together and replant the bank. It isn't more than a village can do.\""
                },
            };

            d.options = new[] { organise, notYet };
            EditorUtility.SetDirty(d);
        }

        private static void CreateCommuneRequest()
        {
            var d = CreateAsset("Decision_CommuneRequest");
            d.decisionId = "commune_request";
            d.npcName = "Chú Hùng (commune officer)";
            d.requiredTier = AgencyTier.Institutional;
            d.oneTimeOnly = true;
            d.promptText = "\"A drain upgrade means digging, pipes and money the commune doesn't have this year. Show me this isn't one family's complaint, and I can put it forward.\"";

            var presentEvidence = new DecisionOption
            {
                optionText = "Present what you've gathered: the drain, the reeds, the market and the neighbours behind you.",
                requiredTier = AgencyTier.Institutional,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Trust, 10),
                    Delta(MeterType.Ecology, 15),
                    Delta(MeterType.Resources, -5),
                },
                setsFlag = "petition_evidence"
            };

            var quickFix = new DecisionOption
            {
                optionText = "Ask for a quick fix: dig the drain deeper.",
                requiredTier = AgencyTier.Institutional,
                immediateDeltas = new[]
                {
                    Delta(MeterType.Trust, 5),
                    Delta(MeterType.Ecology, -5),
                    Delta(MeterType.Resources, -10),
                },
                setsFlag = "petition_quickfix"
            };

            d.promptVariants = new[]
            {
                new PromptVariant
                {
                    requiredFlag = "rain_severe",
                    promptText = "\"I saw the road after the rain. Families waded to school and the market flooded. I can't ignore that. But a drain upgrade still means money the commune doesn't have. Show me this isn't one family's complaint.\""
                },
                new PromptVariant
                {
                    requiredFlag = "rain_mild",
                    promptText = "\"I hear the road to school stayed open in the rain. Good. But the drain is still weak. An upgrade means digging, pipes and money the commune doesn't have. Show me it's worth it.\""
                },
            };

            d.options = new[] { presentEvidence, quickFix };
            EditorUtility.SetDirty(d);
        }
    }
}
#endif
