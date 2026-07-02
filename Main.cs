using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Poses;
using MelonLoader;
using RumbleModdingAPI.RMAPI;
using static Il2CppRUMBLE.Poses.PoseSet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using Il2CppRUMBLE.Input;

namespace LeftPoseFix
{
    public class Main : MelonMod
    {
        public static Il2CppSystem.Collections.Generic.List<PoseInputSource> inputSources; // the list of all poses the player has access to
        public static bool sceneLoading = true;
        
        public override void OnLateInitializeMelon() {
            Actions.onMapInitialized += (string sceneName) => {
                inputSources = Calls.Players.GetLocalPlayerController().gameObject.transform.GetComponentInChildren<PlayerPoseSystem>().currentInputPoses;
                sceneLoading = false;
            };
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            sceneLoading = true;
        }


        // UpdateHandPresenceAnimationStates takes a hand (ie left or right) and some controller inputs, then animates the given hand based on the inputs.
        // this adds a prefix that changes the inputs it recieves to all the buttons being pressed (ie a fist) if a left handed pose is detected.
        [HarmonyPatch(typeof(PlayerHandPresence), nameof(PlayerHandPresence.UpdateHandPresenceAnimationStates))]
        class Patch {
            private static void Prefix(ref PlayerHandPresence __instance, ref InputManager.Hand hand, ref PlayerHandPresence.HandPresenceInput input) {
                if (sceneLoading) {
                    return;
                };
                if (__instance.transform.parent != Calls.Players.GetLocalPlayerController().transform) { // check to make sure these are not your opponents hands
                    return;
                }
                //loop through all poses and see if the left handed varient is being done.
                bool doingPose = false;
                for (int i = 0; i < inputSources.Count; i++) {
                    Il2CppReferenceArray<PoseConfiguration> poseConfigs = inputSources[i].poseSet.configurations;
                    PoseData poseData = poseConfigs[poseConfigs.Count - 1].Pose; //The pose config will either be just one modifyer pose, or the base pose then a structure pose.
                    if (poseData.IsDoingPose(Calls.Players.GetLocalPlayerController(), PoseData.ComparisonMethod.Mirrored).IsValidPose) {
                        doingPose = true;
                    }
                }
                if (doingPose) {
                    input = new PlayerHandPresence.HandPresenceInput(1f, 1f, 1f, 0f);
                }
            }
        }
    }


}

