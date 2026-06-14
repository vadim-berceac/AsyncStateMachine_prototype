using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<Camera>()
            .FromComponentInNewPrefabResource("Camera/SceneCamera")
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<ClickHandler>()
            .AsSingle()
            .NonLazy();
    }
}