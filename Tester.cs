using Sirenix.OdinInspector;
using UnityEngine;
using VisualNovelFramework.Outfitting;

namespace VisualNovelFramework
{
    public class Tester : MonoBehaviour
    {
        [Button]
        public void Test()
        {
            var l = CoffeeAssetDatabase.FindAssetsOfType<CharacterOutfit>();

            l.ForEach((e) => Debug.Log(e.name));
        }

        public string targetGUID;
        [Button]
        public void FindItemWithGUID()
        {
            var q = CoffeeAssetDatabase.FindAssetWithCoffeeGUID<CharacterOutfit>(targetGUID);
            if (q != null)
            {
                Debug.Log(q.name);
            }
        }
    }
}