using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Text;

public class ParkourCounter : MonoBehaviour
{
    public OriginalGameMechanics ogm;
    public bool isStageChange;
    // banners
    public GameObject startBanner;
    public GameObject firstBanner;
    public GameObject secondBanner;
    public GameObject finalBanner;
    // coins
    public GameObject firstCoins;
    public GameObject secondCoins;
    public GameObject finalCoins;
    // Object Interaction Task
    public GameObject objIX1;
    public GameObject objIX2;
    public GameObject objIX3;
    // respawn points
    public Transform start2FirstRespawn;
    public Transform first2SecondRespawn;
    public Transform second2FinalRespawn;
    public Vector3 currentRespawnPos;

    public float timeCounter;
    private float part1Time;
    private float part2Time;
    private float part3Time;
    public int coinCount;

    private int part1Count; // 17
    private int part2Count; // 33
    private int part3Count; // 24
    public bool parkourStart;

    public TMP_Text timeText;
    public TMP_Text coinText;
    public TMP_Text recordText;
    public GameObject timeTextGO;
    public GameObject coinTextGO;
    public GameObject recordTextGO;
    public GameObject endTextGO;
    public AudioSource backgroundMusic;
    public AudioSource endSoundEffect;
    public SelectionTaskMeasure selectionTaskMeasure;

    //new metrics
    public int TeleportActionUses;
    public int MountActionUses;
    public int ThrowActionUses;
    public int RemoteActionUses;
    public int RecallActionUses;
    public int FollowActionUses;
    public float AvgDistToGround;
    public int AvgCount;
    private MetricDto metric;

    void Start()
    {
        coinCount = 0;
        timeCounter = 0.0f;
        firstBanner.SetActive(false);
        secondBanner.SetActive(false);
        finalBanner.SetActive(false);
        firstCoins.SetActive(false);
        secondCoins.SetActive(false);
        finalCoins.SetActive(false);
        objIX2.SetActive(false);
        objIX3.SetActive(false);
        objIX1.SetActive(false);
        parkourStart = false;
        endTextGO.SetActive(false);
        selectionTaskMeasure = GetComponent<SelectionTaskMeasure>();
        TeleportActionUses = 0;
        MountActionUses = 0;
        ThrowActionUses = 0;
        RemoteActionUses = 0;
        RecallActionUses = 0;
        FollowActionUses = 0;
        AvgDistToGround = 0f;
        AvgCount = 0;
        metric = new MetricDto { filename = System.DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss") + "TRBLog.txt" };
    }

    void Update()
    {
        if (isStageChange)
        {
            isStageChange = false;
            if (ogm.stage == startBanner.name)
            {
                parkourStart = true;
                startBanner.SetActive(false);
                firstBanner.SetActive(true);
                firstCoins.SetActive(true);
                objIX1.SetActive(true);
                GetComponent<SelectionTaskMeasure>().taskUI.transform.position = objIX1.transform.position + 0.75f * Vector3.up;
                currentRespawnPos = start2FirstRespawn.position;
            }
            else if (ogm.stage == firstBanner.name)
            {
                firstBanner.SetActive(false);
                firstCoins.SetActive(false);
                objIX1.SetActive(false);
                secondBanner.SetActive(true);
                secondCoins.SetActive(true);
                objIX2.SetActive(true);
                GetComponent<SelectionTaskMeasure>().taskUI.transform.position = objIX2.transform.position + 0.75f * Vector3.up;
                part1Time = timeCounter;
                part1Count = coinCount;
                currentRespawnPos = first2SecondRespawn.position;
                UpdateRecordText(1, part1Time, part1Count, 16);
            }
            else if (ogm.stage == secondBanner.name)
            {
                secondBanner.SetActive(false);
                secondCoins.SetActive(false);
                objIX2.SetActive(false);
                finalBanner.SetActive(true);
                finalCoins.SetActive(true);
                objIX3.SetActive(true);
                GetComponent<SelectionTaskMeasure>().taskUI.transform.position = objIX3.transform.position + 0.75f * Vector3.up;
                part2Time = timeCounter - part1Time;
                part2Count = coinCount - part1Count;
                currentRespawnPos = second2FinalRespawn.position;
                UpdateRecordText(2, part2Time, part2Count, 30);
            }
            else if (ogm.stage == finalBanner.name)
            {
                parkourStart = false;
                finalCoins.SetActive(false);
                objIX3.SetActive(false);
                part3Time = timeCounter - (part1Time + part2Time);
                part3Count = coinCount - (part1Count + part2Count);
                UpdateRecordText(3, part3Time, part3Count, 23);
                timeTextGO.SetActive(false);
                coinTextGO.SetActive(false);
                recordTextGO.SetActive(false);
                endTextGO.SetActive(true);
                endTextGO.GetComponent<TMP_Text>().text = "Parkour Finished!\n" + recordText.text +
                    "\ntotal: " + timeCounter.ToString("F1") + ", " + coinCount.ToString() + "/69";
                Debug.Log(endTextGO.GetComponent<TMP_Text>().text);
                endSoundEffect.Play();
                WriteMetrics();
            }
        }

        if (parkourStart)
        {
            timeCounter += Time.deltaTime;
            timeText.text = "time: " + timeCounter.ToString("F1");
            coinText.text = "coins: " + coinCount.ToString();
        }
    }

    void UpdateRecordText(int part, float time, int coinsCount, int coinsInPart)
    {
        string newRecords = "loco" + part.ToString() + ": " + time.ToString("F1") + ", " + coinsCount + "/" + coinsInPart + "\n" +
                            "obj" + part.ToString() + ": " + (selectionTaskMeasure.partSumTime / 5f).ToString("F1") + "," + (selectionTaskMeasure.partSumErr / 5).ToString("F2");
        recordText.text = recordText.text + "\n" + newRecords;

        //record metrics for log and reset all counters for next stage
        metric.stageMetrics[part - 1] = new PartMetric
        {
            PartNr = part,
            TeleportActionUses = TeleportActionUses,
            MountActionUses = MountActionUses,
            ThrowActionUses = ThrowActionUses,
            RemoteActionUses = RemoteActionUses,
            RecallActionUses = RecallActionUses,
            FollowActionUses = FollowActionUses,
            AvgDistToGround = AvgDistToGround,
            Time = time,
            CoinsCollected = coinsCount,
            CoinsPossible = coinsInPart,
            AvgTaskTimeNeeded = selectionTaskMeasure.partSumTime / 5f,
            AvgTaskErrorMagnitude = selectionTaskMeasure.partSumErr / 5f,
        };
        TeleportActionUses = 0;
        MountActionUses = 0;
        ThrowActionUses = 0;
        RemoteActionUses = 0;
        RecallActionUses = 0;
        FollowActionUses = 0;
        AvgDistToGround = 0f;
        AvgCount = 0;
    }

    private void WriteMetrics()
    {
        string path = Path.Combine(Application.persistentDataPath, metric.filename);
        File.WriteAllText(path, JsonUtility.ToJson(metric, true));
    }

    [Serializable]
    public sealed class MetricDto
    {
        public string filename;
        public PartMetric[] stageMetrics = new PartMetric[3];

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine(filename);
            foreach (var item in stageMetrics)
            {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }
    }

    [Serializable]
    public sealed class PartMetric
    {
        public int PartNr;
        public int TeleportActionUses;
        public int MountActionUses;
        public int ThrowActionUses;
        public int RemoteActionUses;
        public int RecallActionUses;
        public int FollowActionUses;
        public float AvgDistToGround;
        public float Time;
        public int CoinsCollected;
        public int CoinsPossible;
        public float AvgTaskTimeNeeded;
        public float AvgTaskErrorMagnitude;

        public override string ToString() {
            StringBuilder sb = new();
            sb.Append("Part#: "); sb.Append(PartNr);
            sb.Append("\nActions: TPs:"); sb.Append(TeleportActionUses);
            sb.Append("  MTs:"); sb.Append(MountActionUses);
            sb.Append("  TWs:"); sb.Append(ThrowActionUses);
            sb.Append("  RTs:"); sb.Append(RemoteActionUses);
            sb.Append("  RCs:"); sb.Append(RecallActionUses);
            sb.Append("  FLs:"); sb.Append(FollowActionUses);
            sb.Append("\nAvgGroundDistance: "); sb.Append(AvgDistToGround);
            sb.Append("\nTime: "); sb.Append(Time);
            sb.Append("\nCoins: "); sb.Append(CoinsCollected); sb.Append('/'); sb.Append(CoinsPossible);
            sb.Append("\nAvgTask: Time:"); sb.Append(AvgTaskTimeNeeded);
            sb.Append("  Error:"); sb.Append(AvgTaskErrorMagnitude);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
