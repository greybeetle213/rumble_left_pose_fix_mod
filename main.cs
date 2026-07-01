using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Poses;
using MelonLoader;
using UnityEngine;
using RumbleModdingAPI.RMAPI;
using static Il2CppRUMBLE.Poses.PoseSet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LeftPoseFix
{

    public class main : MelonMod
    {
        private Il2CppSystem.Collections.Generic.List<PoseInputSource> inputSources; // the list of all poses the player has access to
        private bool sceneLoading = true;
        public override void OnLateInitializeMelon() {
            Actions.onMapInitialized += (string sceneName) => {
                inputSources = Calls.Players.GetLocalPlayerController().gameObject.transform.GetComponentInChildren<PlayerPoseSystem>().currentInputPoses;
                sceneLoading = false;
            };
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            sceneLoading = true;
        }
        public override void OnUpdate() {
            if (sceneLoading == true) {
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
            Calls.Players.GetLocalPlayerController().PlayerAnimator.animator.SetBool(Animator.StringToHash("PoseFistsActive"), doingPose); //close hands if doing left handed pose.
        }
    }
}

