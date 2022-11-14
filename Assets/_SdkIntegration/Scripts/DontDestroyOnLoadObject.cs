using UnityEngine;

namespace ATSoft
{
    public class DontDestroyOnLoadObject : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}