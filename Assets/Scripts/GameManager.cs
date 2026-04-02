using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Transform spawnPoint;
    
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        
    }
}
