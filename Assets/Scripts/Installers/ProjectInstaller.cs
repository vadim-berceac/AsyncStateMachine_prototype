using UnityEngine.InputSystem;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<InputActionAsset>()
            .FromScriptableObjectResource("Input/InputActionAsset")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<GameManager>()
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<PlayerInputService>()
            .AsSingle()
            .NonLazy();
    }
}