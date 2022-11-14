using ATSoft;
using DG.Tweening;
using UnityEngine;

public class UICoinLabel : Singleton<UICoinLabel>
{
    public float flyTime = 1;
    public Transform coinDestination;
    public GameObject[] coins;
    public AudioSource sfx;
    
    public Tween Flying(long coin, Vector3 from)
    {
        sfx.Play();
        float angleUnit = 2 * Mathf.PI / coins.Length;
        for (int i = 0; i < coins.Length; i++)
        {
            int cp_i = i;

            Sequence seq = DOTween.Sequence();
            coins[i].transform.position = from;

            coins[i].SetActive(true);

            // Firework
            float angle = angleUnit * i;
            seq.Append(coins[i].transform.DOMove(
                from + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 200,
                flyTime));

            seq.Append(coins[i].transform.DOMove(coinDestination.position, flyTime));

            seq.onComplete = delegate { coins[cp_i].SetActive((false)); };

            if (i == coins.Length - 1)
            {
                seq.AppendCallback(delegate
                {
                    //todo: Save coin after Spin
                    Debug.Log($"Spin complete: {coin} -> Save coin after Spin here");
                });
                return seq;
            }
        }

        return null;
    }
    
    private Vector3 RandomPointOnCircleEdge(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        return new Vector3(vector2.x, vector2.y, 0);
    }
}
