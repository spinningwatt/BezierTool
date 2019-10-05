using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BezierTool
{
    [CustomEditor(typeof(BezierDriver))]
    public class BezierEditor : Editor
    {
        public int fitNumber;               //用以拟合的段数
        public List<Vector3> ctrlPoints;    //控制点集合
        public List<Vector3> fitPoints;     //拟合点集合，个数为fitNumber
        public List<int> velocity;          //表示下一帧往后移动几个拟合点，用来模拟速度

        private int[] aFactor;

        private BezierDriver bd;
        private Quaternion handleRo;

        private void OnSceneGUI()
        {
            bd = target as BezierDriver;
            this.fitNumber = bd.fitNumber;
            this.ctrlPoints = bd.ctrlPoints.ToList();
            this.fitPoints = bd.fitPoints.ToList();
            this.velocity = bd.velocity.ToList();

            handleRo = Tools.pivotRotation == PivotRotation.Local ? bd.transform.rotation : Quaternion.identity;

            FitBezier();

            Handles.color = Color.white;
            Vector3[] toDraw = new Vector3[fitPoints.Count];
            for (int i = 0; i < fitPoints.Count; i++)
            {
                toDraw[i] = fitPoints[i];
            }
            Handles.DrawLines(toDraw);

            Handles.color = Color.grey;
            for (int i = 1; i < ctrlPoints.Count; i++)
            {
                Handles.DrawDottedLine(ctrlPoints[i - 1], ctrlPoints[i], 2f);
            }

            for (int i = 0; i < ctrlPoints.Count; i++)
            {
                ShowHandlePoint(i);
            }
        }

        private Vector3 ShowHandlePoint(int index)
        {
            //Vector3 point = bd.transform.TransformPoint(bd.ctrlPoints[index]);
            Vector3 point = bd.ctrlPoints[index];
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRo);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bd, "Move Point");
                EditorUtility.SetDirty(bd);
                bd.ctrlPoints[index] = point;//bd.transform.InverseTransformPoint(point);
            }
            return point;
        }

        //计算曲线的拟合点集
        private void FitBezier()
        {
            aFactor = YangHui(ctrlPoints.Count);
            bd.fitPoints[0] = fitPoints[0] = ctrlPoints[0];
            bd.fitPoints[fitNumber - 1] = fitPoints[fitNumber - 1] = ctrlPoints[ctrlPoints.Count-1];
            //Debug.Log(ctrlPoints[ctrlPoints.Count - 1].ToString());
            float t = 0;
            Vector3 b = new Vector3();
            for (int i = 1; i < fitNumber-1; i++) //从1循环到fitNumber-2，首末是控制点
            {
                t = i * 1.0f / (fitNumber-1);
                //最外层循环t
                for (int j = 0; j < ctrlPoints.Count; j++)
                {
                    b.x = (float)(b.x + aFactor[j] * ctrlPoints[j].x * Math.Pow(1f - t, ctrlPoints.Count - 1 - j) * Math.Pow(t, j));
                    b.y = (float)(b.y + aFactor[j] * ctrlPoints[j].y * Math.Pow(1f - t, ctrlPoints.Count - 1 - j) * Math.Pow(t, j));
                    b.z = (float)(b.z + aFactor[j] * ctrlPoints[j].z * Math.Pow(1f - t, ctrlPoints.Count - 1 - j) * Math.Pow(t, j));

                        //Debug.Log("when j="+j.ToString()+" ctrlP=" + ctrlPoints[j].ToString());
                }
                if(i>50)
                {
                    //Debug.Log(b.ToString()+" - t = "+t.ToString("f4"));
                }
                bd.fitPoints[i] = fitPoints[i] = b;
                b = Vector3.zero;
            }
        }


        //计算杨辉三角，作为曲线计算的系数
        private int[] YangHui(int n)
        {
            //传入第n行的参数

            int[,] array = new int[n, n];
            for (int i = 0; i < n; i++)
            {

                for (int j = 0; j <= i; j++) //注意:j<=i, 因为第1行有1列，第2行有2列，第3行有3列。。。
                {
                    if (j == 0 || i == j)  //第一列和最后一列
                    {
                        array[i, j] = 1; //值为1
                    }
                    else
                    {
                        array[i, j] = array[i - 1, j - 1] + array[i - 1, j]; //中间列的值 = 上一行和它所在列-1的值 + 上一行和它所在列的值
                    }
                }
            }
            int[] yh = new int[n];
            for(int i = 0; i < n; i++)
            {
                yh[i] = array[n - 1, i];
                //Debug.Log("delta="+yh[i]);
            }
            return yh;
        }

    }
}
