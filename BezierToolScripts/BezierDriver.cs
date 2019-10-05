using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BezierTool
{
    public class BezierDriver : MonoBehaviour
    {
        public Direction directionOfFace;
        [Tooltip("勾上则游戏一运行就自动开始驱动物体")]
        public bool moveOnStart;
        public float speedRatio;
        public int fitNumber;               //用以拟合的段数
        public Vector3[] ctrlPoints;    //控制点集合
        public Vector3[] fitPoints;     //拟合点集合，个数为fitNumber
        public int[] velocity;          //表示下一帧往后移动几个拟合点，用来模拟速度

        public UnityEvent HasStoped;

        [SerializeField]
        private bool underMoving;
        private int vIndex;
        private int pIndex;

        public enum Direction
        {
            forward,
            back,
            up,
            down,
            left,
            right
        }


        void Reset()
        {
            //构造函数，默认100段的拟合精度，3个控制点
            this.fitNumber = 100;
            this.ctrlPoints = new Vector3[3];
            for (int i = 0; i < ctrlPoints.Length; i++)
            {
                ctrlPoints[i] = new Vector3(i, i, i);
            }
            this.fitPoints = new Vector3[this.fitNumber];
            this.velocity = new int[this.fitNumber];
            this.moveOnStart = true;
            this.underMoving = false;
            this.speedRatio = 10;
            this.vIndex = 0;
            this.pIndex = 0;
            for (int i = 0; i < velocity.Length; i++)
            {
                //默认线性移动
                velocity[i] = 1;
            }
        }

        private void FixedUpdate()
        {
            if(underMoving)
            {
                if (pIndex > fitNumber - 2)
                {
                    StopObject();
                    HasStoped.Invoke();
                    return;
                }
                Vector3 dest = Vector3.Lerp(fitPoints[pIndex], fitPoints[pIndex + 1], (vIndex / speedRatio));

                Vector3 tarVector = dest - this.transform.position;
                switch (directionOfFace)
                {
                    case Direction.forward:
                        this.transform.forward = tarVector;
                        break;
                    case Direction.back:
                        this.transform.forward = -tarVector;
                        break;
                    case Direction.up:
                        this.transform.up = tarVector;
                        break;
                    case Direction.down:
                        this.transform.up = -tarVector;
                        break;
                    case Direction.left:
                        this.transform.right = -tarVector;
                        break;
                    case Direction.right:
                        this.transform.right = tarVector;
                        break;
                    default:
                        break;
                }


                if ( ++vIndex == speedRatio)
                {
                    pIndex++;
                    vIndex = 0;
                }
                this.transform.position = dest;
            }
        }

        private void Start()
        {
            if(moveOnStart)
            {
                DriveObject();
            }
        }

        //开始和停止驱动
        public void DriveObject()
        {
            underMoving = true;
        }
        public void StopObject()
        {
            underMoving = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawIcon(ctrlPoints[0], "BezierToolGizmo-Start.png", true);
            Gizmos.DrawIcon(ctrlPoints[ctrlPoints.Length-1], "BezierToolGizmo-Stop.png", true);
        }
    }
}
