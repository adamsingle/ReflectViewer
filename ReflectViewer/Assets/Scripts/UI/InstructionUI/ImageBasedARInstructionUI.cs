using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [CreateAssetMenu(fileName = "ImageBasedARInstruction", menuName = "Reflect/ImageBasedARInstruction", order = 54)]
    public class ImageBasedARInstructionUI : ScriptableObject, IInstructionUI, IUIButtonValidator
    {
        private enum ImageBasedInstructionUI
        {
            Init = 0,
            FindTheImage,
            OnBoardingComplete
        };
        public ARMode arMode => ARMode.ImageBased;

        public ExposedReference<Canvas> InstructionUICanvesRef;
        public ExposedReference<Raycaster> RaycasterRef;

        Raycaster _raycaster;

        ARModeUIController _arModeUIController;

        ImageBasedInstructionUI _imageBasedInstructionUI;

        const string _instrucationFindImageText = "Pan your device to find the image marker";

        Dictionary<ImageBasedInstructionUI, InstructionUIStep> _states;

        public InstructionUIStep CurrentInstructionStep => _states[_imageBasedInstructionUI];

        public void Back()
        {
            var transition = _states[--_imageBasedInstructionUI].onBack;
            if (transition != null)
            {
                transition();
            }
        }

        public bool ButtonValidate()
        {
            switch(_imageBasedInstructionUI)
            {
                case ImageBasedInstructionUI.FindTheImage:
                    return _raycaster.ValidTarget;
                default:
                    return false;
            }
        }

        public void Cancel()
        {
            _arModeUIController.StartCoroutine(AcknowledgeCancel());
        }

        private IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        public void Initialize(ARModeUIController resolver)
        {
            _arModeUIController = resolver;
            _raycaster = RaycasterRef.Resolve(resolver);

            _states = new Dictionary<ImageBasedInstructionUI, InstructionUIStep>
            {
                {ImageBasedInstructionUI.Init, new InstructionUIStep{stepIndex = (int) ImageBasedInstructionUI.Init, onNext = StartInstruction} },
                {ImageBasedInstructionUI.FindTheImage, new InstructionUIStep{stepIndex= (int)ImageBasedInstructionUI.FindTheImage, onNext = FindImageNext, onBack = FindImageBack} },
                {ImageBasedInstructionUI.OnBoardingComplete, new InstructionUIStep {stepIndex = (int)ImageBasedInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } }
            };
        }

        public void Next()
        {
            var transition = _states[++_imageBasedInstructionUI].onNext;
            if(transition != null)
            {
                transition();
            }
        }

        public void Restart()
        {
            _imageBasedInstructionUI = ImageBasedInstructionUI.Init;
            _arModeUIController.StartCoroutine(ResetInstructionUI());
        }

        private IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0);
            _imageBasedInstructionUI = ImageBasedInstructionUI.Init;
            _states[_imageBasedInstructionUI].onNext();
        }

        void StartInstruction()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));

            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(false);
            navigationState.showScaleReference = true;

            _raycaster.Reset();
            _raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.ImagePlacementRule));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOne));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Instruction));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
            Next();
        }

        void FindImageNext()
        {
            FindTheImage();
        }

        void FindImageBack()
        {
            FindTheImage();
        }

        void FindTheImage()
        {
            UIStateManager.current.m_PlacementRules.SetActive(true);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnablePlacement, true));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text = _instrucationFindImageText, level = StatusMessageLevel.Instruction }));
            _raycaster.ActiveScanning = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
        }

        void OnBoardingCompleteNext()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            _raycaster.SwapModel(UIStateManager.current.m_BoundingBoxRootNode, UIStateManager.current.m_RootNode);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.selectionEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            var settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }
    }
}
