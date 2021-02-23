using UnityEditor.Experimental.GraphView;

namespace VisualNovelFramework.GraphFramework.Editor
{
    public class PropertyBlackboard : UnityEditor.Experimental.GraphView.Blackboard
    {
        private CoffeeGraphView coffeeGraphView;

        public PropertyBlackboard(CoffeeGraphView coffeeGraphView)
        {
            this.coffeeGraphView = coffeeGraphView;
            
            Add(new BlackboardSection {title = "Properties: "});
        }
    }
}