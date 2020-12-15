using UnityEditor;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    static class QRPipeline
    {
        [UnityEditor.MenuItem("Create QR Pipeline Asset", menuItem = "Assets/Reflect/Pipeline/Create QR Pipeline Asset")]
        static void CreateQRPipelineSample()
        {
            var existingPipelineAsset = AssetDatabase.LoadAssetAtPath<PipelineAsset>("Assets/Pipelines/ViewerPipeline.asset");
            if (existingPipelineAsset == null)
            {
                Debug.LogWarning("Assets / Pipelines / ViewerPipeline.asset not found");
                return;
            }

            var pipelineAsset = PipelineAsset.Instantiate<PipelineAsset>(existingPipelineAsset);
            if (pipelineAsset.TryGetNode<InstanceConverterNode>(out var instanceConverter))
            {
                var placementNode = pipelineAsset.CreateNode<QRPlacementNode>();
                pipelineAsset.CreateConnection(instanceConverter.output, placementNode.input);

                //save asset
                AssetDatabase.CreateAsset(pipelineAsset, "Assets/QR/Pipelines/QR Viewer Pipeline.asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = pipelineAsset;
            }
            else
            {
                Debug.LogWarning("No InstanceConverterNode found in Assets/Pipelines/ViewerPipeline.asset");
            }
        }
    }
}
