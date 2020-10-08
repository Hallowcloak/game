using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Mirror;
using System.Threading;

namespace ControllerExperiment
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager gm;
        //public static CinemachineVirtualCamera myCinemachine = null;

        void Start()
        {
            if (gm == null)
            {
                gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
            }
        }

        bool gameHasEnded = false;
        // Start is called before the first frame update

        public GameObject completeLevelUI;

        public void CompleteLevel()
        {
            Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
            Invoke("Restart", 3f);
            Invoke("ShowLevelComplete", 2f);
        }

        public void KillPlayer(PlayerController player)
        {
            Destroy(player.gameObject);
            //Debug.Log("Player Killed");
            gm.Respawn();
            //myCinemachine = GetComponent<CinemachineVirtualCamera>();
        }

        public Transform playerPrefab;
        public Transform spawnPoint;
        public Transform spawnPrefab;

        //private CinemachineTransposer transposer;
        private void ShowLevelComplete()
        {
            completeLevelUI.SetActive(true);
        }

        public void Respawn()
        {
            Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("TODO: Add Spawn Particles");

        }

        public void Respawn(GameObject player)
        {
            NetworkServer.UnSpawn(player);
            Transform newPos = NetworkManager.singleton.GetStartPosition();
            player.transform.position = newPos.position;
            player.transform.rotation = newPos.rotation;
            return;
            //NetworkServer.Spawn(player, player);
        }

        public void EndGame()
        {
            if (gameHasEnded == false)
            {
                gameHasEnded = true;
                Debug.Log("Game Over");
                //Restart Game + Hosting durdurması lazım yeniden başlarken

                Invoke("Restart", 2f);
            }
        }

        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}