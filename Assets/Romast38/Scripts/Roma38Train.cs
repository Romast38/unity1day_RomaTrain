using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2018/06/21 Romast38 ろますた38 が作成。
public class Roma38Train : MonoBehaviour {

    public Transform line_parent; // 線路。この子オブジェクトが通過点になる。
    public Transform train; // 列車。これがlineの子オブジェクトの上を走る。

    List<Vector3> linePath = new List<Vector3>(); // 線路の軌跡となるオブジェクトのトランスフォーム

    int section = 0; // おそらく、lineの要素数-1個ある。
    float t = 0; // 0<t<1の変数。Lerp関数の引数に使う。

    public float speed = 100; // 列車のスピード

    AudioSource train_sound; // 列車の音。

    void Start ()
    {
        train_sound = train.gameObject.AddComponent<AudioSource>();
        train_sound.clip = Resources.Load<AudioClip>("Romast38/trainnoise");
        train_sound.loop = true;
        train_sound.Play();

        UpdateLine();
    }
	
	void Update ()
    {
        int section_next = section + 1;
        if (section == linePath.Count - 1)
        {
            section_next = 0;
        }

        Vector3 p1 = linePath[section]; // 一区間の始まりの位置
        Vector3 p2 = linePath[section_next]; // 一区間の終わりの位置
        Vector3 pos = Vector3.Lerp(p1, p2, t); // 線形補間
        train.position = pos;

        train.LookAt(linePath[section_next]);
        t += speed * 0.005f / Vector3.Distance(p1, p2);

        if (t >= 1)
        {
            t = 0;
            section++;
            if (section >= linePath.Count)
            {
                section = 0;
            }
        }
    }

    // 線路の状況を更新する。line_parentの子オブジェクトを追加した場合や、減らした場合に実行する。具体的には、line_parentからlineを取得する。
    public void UpdateLine()
    {
        linePath.Clear();

        for (int i = 0; i < line_parent.childCount; i++)
        {
            if (line_parent.childCount < 4)
            {
                Debug.LogError("最低4つの線路マーカーが必要です.");
                continue;
            }

            int ii = i + 1;
            if (ii == line_parent.childCount)
            {
                ii = 0;
            }

            int iii = ii + 1;
            if (iii == line_parent.childCount)
            {
                iii = 0;
            }

            Vector3 now = line_parent.GetChild(i).position;
            Vector3 next = line_parent.GetChild(ii).position;
            Vector3 nextNext = line_parent.GetChild(iii).position;

            Vector3 p0 = now;
            Vector3 p1 = next;
            Vector3 v0 = (next - now);
            Vector3 v1 = (nextNext - next);

            float divNum = 25; // カーブ分割数
            for (float t = 0; t < 1; t += 1 / divNum)
            {
                Vector3 pos = Ferguson(p0, p1, v0, v1, t);
                linePath.Add(pos);
            }
        }
    }

    // 線路のデバック表示
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return; // ゲームの実行が終わってもGizmo関数は実行されるので、エラーが出る。それを回避する。

        Gizmos.color = Color.black;

        for (int i = 0; i < linePath.Count; i++)
        {
            int ii = i + 1;
            if (ii == linePath.Count)
            {
                ii = 0; // おしりに来たら頭に戻る。
            }

            Gizmos.DrawLine(linePath[i], linePath[ii]);
            Gizmos.DrawSphere(linePath[i], 0.1f);
        }

        //{
        //    // ファーガソン曲線デバック
        //    Vector3 p0 = new Vector3();
        //    Vector3 p1 = new Vector3(1, 1, 0);
        //    Vector3 v0 = new Vector3(1, 0, 0);
        //    Vector3 v1 = new Vector3(0, 1, 0);
        //    for (float t = 0; t < 1; t += 0.1f)
        //    {
        //        Gizmos.DrawSphere(Ferguson(p0, p1, v0, v1, t), 0.1f);
        //    }
        //    Gizmos.DrawLine(p0, p0 + v0);
        //    Gizmos.DrawLine(p1, p1 + v1);
        //}
    }

    // ファーガソン曲線
    // 2つの位置ベクトルと、2つ速度ベクトル、パラーメタ0<t<1を引数に指定すると、2つの位置ベクトルを補完する曲線上の点を返す。
    Vector3 Ferguson(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1, float t)
    {
        float a = (1 - t);
        float aa = a * a;
        float tt = t * t;

        // エルミート関数の計算
        float h0 = (2 * t + 1) * aa;
        float h1 = t * aa;
        float h2 = -tt * a;
        float h3 = tt * (3 - 2 * t);

        return p0 * h0 + v0 * h1 + v1 * h2 + p1 * h3;
    }
}