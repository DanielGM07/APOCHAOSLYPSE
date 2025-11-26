using System.Collections.Generic;

namespace POCHAOSLYPSE
{
  public class SceneManager
  {
    private Stack<IScene> scenes = new();

    public SceneManager() { }

    public void AddScene(IScene scene)
    { scenes.Push(scene); }

    public void RemoveScene()
    {
      if(scenes.Count == 1)
      {
        throw new System.InvalidOperationException("Tried removing a scene but the stack has only one");
      }
      scenes.Pop();
    }

    public IScene getScene()
    { return scenes.Peek(); }
  }
}
