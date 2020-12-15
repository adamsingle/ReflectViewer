using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Reflect.Pipeline
{
    [Serializable]
    public class QRPlacementNodeSettings
    {
        [Serializable]
        public class PlacementEntry
        {
            public string Property;
            public string Value;
        }

        public bool EnablePlacement = true;
        public List<PlacementEntry> Entries;
    }

    public class QRPlacementNode : ReflectNode<QRPlacement>
    {
        public GameObjectInput input = new GameObjectInput();

        [SerializeField]
        ExposedReference<Transform> _root;

        public QRPlacementNodeSettings Settings;

        public void SetRoot(Transform root, IExposedPropertyTable resolver)
        {
            resolver.SetReferenceValue(_root.exposedName, root);
        }

        protected override QRPlacement Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var root = _root.Resolve(resolver);
            if (root == null)
            {
                root = new GameObject("placement root").transform;
            }

            var node = new QRPlacement(root, Settings);
            input.streamEvent = node.OnGameObjectEvent;
            return node;
        }
    }

    public class QRPlacement : IReflectNodeProcessor
    {
        readonly QRPlacementNodeSettings _settings;
        readonly Transform _root;

        public QRPlacement(Transform root, QRPlacementNodeSettings settings)
        {
            _settings = settings;
            _root = root;
        }

        public void OnGameObjectEvent(SyncedData<GameObject> stream, StreamEvent streamEvent)
        {
            if (streamEvent == StreamEvent.Added)
            {
                var gameObject = stream.data;
                if(!gameObject.TryGetComponent(out Metadata metadata))
                {
                    return;
                }


                //we need to put the model or move the user to the position of this object
                foreach (var entry in _settings.Entries)
                {
                    if (!metadata.parameters.dictionary.TryGetValue(entry.Property, out var category))
                    {
                        continue;
                    }
                    if (category.value.Contains(entry.Value) )
                    {
                        Debug.Log("Found marker in Reflect model");
                        var position = gameObject.transform.position;
                        //do what now??
                    }
                }
            }
        }

        public void RefreshObjects()
        {
            _root.gameObject.SetActive(_settings.EnablePlacement);
        }

        public void OnPipelineInitialized()
        {
            _root.gameObject.SetActive(_settings.EnablePlacement);
        }

        public void OnPipelineShutdown()
        {

        }
    }
}
